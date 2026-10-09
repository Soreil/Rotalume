using emulator.extensions;

namespace emulator.graphics;

public class PixelFetcher(PPU p, VRAM vram, OAM oam)
{
    private readonly FIFO<FIFOPixel> BGFIFO = new();
    private readonly FIFO<FIFOSpritePixel> SpriteFIFO = new();
    private int scanlineX;
    private byte tileIndex;
    private byte tileDataLow;
    private byte tileDataHigh;

    private int FetcherStep;
    private bool PushedEarly;
    private int SpriteFetchDotsRemaining;
    private SpriteAttributes? SpriteBeingFetched;
    private readonly HashSet<int> WindowLY = [];

    //Line finished resets all state which is only relevant for a single line
    internal void LineFinished()
    {
        FetcherStep = 0;
        PushedEarly = false;
        SpriteFetchDotsRemaining = 0;
        SpriteBeingFetched = null;
        delaying = false;
        scanlineX = 0;
        BGFIFO.Clear();
        SpriteFIFO.Clear();
        PixelsPopped = 0;
        PixelsSentToLCD = 0;
    }

    //Frame finished resets all state relevant for an entire frame
    internal void FrameFinished()
    {
        LineFinished();
        WindowLY.Clear();
    }

    //Some of the fetcher step take two cycles
    private bool delaying;

    public void Fetch()
    {
        // Object fetching owns the fetcher until its dot budget has elapsed.
        if (SpriteFetchDotsRemaining > 0) return;

        if (delaying)
        {
            delaying = false;
            return;
        }

        switch (FetcherStep)
        {
            case 0:
            tileIndex = FetchTileID();
            FetcherStep = 1;
            delaying = true;
            break;
            case 1:
            tileDataLow = FetchLow();
            FetcherStep = 2;
            delaying = true;
            break;
            case 2:
            tileDataHigh = FetchHigh();
            PushedEarly = Pushrow();
            FetcherStep = 3;
            delaying = true;
            break;
            case 3:
            FetcherStep = PushedEarly ? 0 : 4;
            delaying = true;
            break;
            case 4:
            FetcherStep = Pushrow() ? 0 : 4;
            delaying = false;
            break;
            default:
            throw new IllegalFetcherState("Illegal fetcher state");
        }
    }

    private readonly SpriteAttributes[] SpriteAttributes = new SpriteAttributes[10];
    private int SpriteCount;
    private int SpritesFinished;

    private void PushSpriteRow(byte low, byte high, SpriteAttributes sprite)
    {
        if (SpriteFIFO.Count <= 8)
        {
            int clippedPixels = int.Clamp(scanlineX + 8 - (Ppu.SCX & 7) - sprite.X, 0, GraphicConstants.SpriteWidth);
            for (int pos = 0; pos < GraphicConstants.SpriteWidth - clippedPixels; pos++)
            {
                int sourcePixel = pos + clippedPixels;
                int bit = sprite.XFlipped ? sourcePixel : 7 - sourcePixel;
                var paletteIndex = (byte)(Convert.ToByte(low.GetBit(bit)) | (byte)(Convert.ToByte(high.GetBit(bit)) << 1));

                ref var existingSpritePixel = ref SpriteFIFO.At(pos);
                var candidate = new FIFOSpritePixel(sprite.Palette, paletteIndex, sprite.SpriteToBackgroundPriority);

                if (ShouldReplace(existingSpritePixel, candidate))
                {
                    existingSpritePixel = candidate;
                }
            }
        }
    }

    private static bool ShouldReplace(FIFOSpritePixel existingSpritePixel, FIFOSpritePixel candidate) =>
        (candidate.Color != 0 && existingSpritePixel.Color == 0) ||
        (candidate.Priority && !existingSpritePixel.Priority);

    public Shade? TryRenderPixel()
    {
        if (SpriteFetchDotsRemaining > 0)
        {
            if (--SpriteFetchDotsRemaining > 0) return null;
            PushSpriteRowToPixelFetcher(SpriteBeingFetched!.Value);
            SpriteBeingFetched = null;
        }

        // Start only when a background pixel could otherwise be emitted. In
        // particular, an object at the left edge must not bypass BG startup.
        if (BGFIFO.Count > 8 || FIFOsNotEmpty())
        {
            var sprite = CanRenderASprite();
            if (sprite is SpriteAttributes sa)
            {
                SpriteBeingFetched = sa;
                SpriteFetchDotsRemaining = 6 + Math.Max(0, 5 - ((sa.X + Ppu.SCX) & 7));
                return null;
            }
        }

        if (FIFOsNotEmpty())
        {
            return RenderPixelFromCombinedFIFOs();
        }
        else if (BGFIFO.Count > 8)
        {
            var pix = BGFIFO.Pop();
            //Do we need to pop in order to do this?
            //Do we need pixels in the fifo to do this?

            return Ppu.BackgroundColor(Ppu.BGEnabledForOutput ? pix.Color : 0);
        }
        else
        {
            return null;
        }
    }

    private Shade RenderPixelFromCombinedFIFOs()
    {
        var bp = BGFIFO.Pop();
        var sp = SpriteFIFO.Pop();
        if (sp.Color != 0 && Ppu.OBJDisplayEnable)
        {
            //obj to bg priority bit is set to true so the sprite pixel
            //will be behind bg color 1,2,3
            return sp.Priority && bp.Color != 0
                ? Ppu.BackgroundColor(Ppu.BGEnabledForOutput ? bp.Color : 0)
                : sp.Palette switch
                {
                    0 => Ppu.SpritePalette0(sp.Color),
                    1 => Ppu.SpritePalette1(sp.Color),
                    _ => throw new IllegalSpritePalette()
                };

        }
        else
        {
            return Ppu.BackgroundColor(Ppu.BGEnabledForOutput ? bp.Color : 0);
        }
    }

    private bool FIFOsNotEmpty() => BGFIFO.Count != 0 && SpriteFIFO.Count != 0;

    private void PushSpriteRowToPixelFetcher(SpriteAttributes sprite)
    {
        //Fill the fifo lower half with transparant pixels
        for (int i = SpriteFIFO.Count; i < GraphicConstants.SpriteWidth; i = SpriteFIFO.Count)
        {
            SpriteFIFO.Push(new FIFOSpritePixel(0, 0, false));
        }


        //16 pixel offset before lines can be offscreen taken out
        var y = Ppu.LY - (sprite.Y - GraphicConstants.DoubleSpriteHeight);
        if (sprite.YFlipped)
        {
            y = Ppu.SpriteHeight == 8 ? 7 - y : 15 - y;
        }

        if (y < 0)
        {
            throw new SpriteDomainError("Illegal Y position in sprite");
        }

        //0xfe is 11111110 in binary. This means that we are masking the least significant bit of the sprite ID.
        //The reason for this is that when the sprite height is 16 pixels,
        //each sprite occupies two consecutive tiles in memory.
        //The first tile has an even ID, and the second tile has an odd ID. By masking the least significant bit,
        //we ensure that we always fetch the correct tile for the sprite,
        //regardless of whether it is the first or second tile.
        var ID = Ppu.SpriteHeight == 8 ? sprite.ID : sprite.ID & 0xfe;
        var addr = VRAM.Start + ID * GraphicConstants.BitsPerSpriteTile + (2 * y);
        var low = VRAM[addr];
        var high = VRAM[addr + 1];
        PushSpriteRow(low, high, sprite);
        SpritesFinished++;
    }

    private SpriteAttributes? CanRenderASprite()
    {
        var states = BGFIFO.Count != 0 &&
        Ppu.OBJDisplayEnable && SpriteCount - SpritesFinished != 0;
        return !states ? null : FirstMatchingSprite();
    }

    private int PixelsPopped;
    public int PixelsSentToLCD;
    public readonly Shade[] LineShadeBuffer = new Shade[GraphicConstants.ScreenWidth];
    internal void AttemptToPushAPixel()
    {
        var pix = TryRenderPixel();
        if (pix is null) return;

        PixelsPopped++;
        scanlineX++;

        if (PixelsPopped > (Ppu.SCX & 7))
        {
            LineShadeBuffer[PixelsSentToLCD++] = pix.Value;
        }

        bool windowStart = PixelsSentToLCD == Ppu.WX - 7 && Ppu.LY >= Ppu.WY && Ppu.WindowDisplayEnable;
        if (windowStart)
        {
            FetcherStep = 0;
            BGFIFO.Clear();
        }
    }

    public void GetSprites()
    {
        SpriteCount = OAM.SpritesOnLine(SpriteAttributes, Ppu.LY, Ppu.SpriteHeight);
        SpritesFinished = 0;
    }

    private SpriteAttributes? FirstMatchingSprite()
    {
        var wanted = scanlineX + 8 - (Ppu.SCX & 7);

        for (int i = SpritesFinished; i < SpriteCount; i++)
        {
            if (SpriteAttributes[i].X <= wanted)
            {
                return SpriteAttributes[i];
            }
        }
        return null;
    }

    private byte FetchHigh() => VRAM[GetAdress() + 1];

    private byte FetchLow() => VRAM[GetAdress()];

    private int GetAdress()
    {
        var tiledatamap = Ppu.BGAndWindowTileDataSelect;

        return inWindow
            ? tiledatamap == VRAM.TileBlock0Start
                ? tiledatamap + (tileIndex * 16) + (((WindowLY.Count - 1) & 7) * 2)
                : VRAM.TileBlock2Start + (((sbyte)tileIndex) * 16) + (((WindowLY.Count - 1) & 7) * 2)
            : tiledatamap == VRAM.TileBlock0Start
                ? tiledatamap + (tileIndex * 16) + (((Ppu.LY + Ppu.SCY) & 0xff & 7) * 2)
                : VRAM.TileBlock2Start + (((sbyte)tileIndex) * 16) + (((Ppu.LY + Ppu.SCY) & 0xff & 7) * 2);
    }

    private bool inWindow;

    public PPU Ppu { get; } = p;
    public VRAM VRAM { get; } = vram;
    public OAM OAM { get; } = oam;

    private byte FetchTileID()
    {
        int tilemap;
        inWindow = (scanlineX + BGFIFO.Count) >= (Ppu.WX - 7) && Ppu.LY >= Ppu.WY && Ppu.WindowDisplayEnable;
        if (inWindow)
        {
            _ = WindowLY.Add(Ppu.LY);
            tilemap = Ppu.TileMapDisplaySelect;
        }
        else
        {
            tilemap = Ppu.BGTileMapDisplaySelect;
        }

        var windowStartX = Ppu.WX - 7;
        var windowStartY = WindowLY.Count - 1;

        //TODO: handle tick cost of this condition
        windowStartX = int.Clamp(windowStartX, 0, 256);

        var tileX = inWindow ? ((scanlineX + BGFIFO.Count) / 8) - (windowStartX / 8) :
                               ((Ppu.SCX / 8) + ((scanlineX + BGFIFO.Count) / 8)) & 0x1f;
        var tileY = inWindow ? windowStartY :
                               (Ppu.LY + Ppu.SCY) & 0xff;

        var tileIndex = VRAM[tilemap + tileX + ((tileY / 8) * 32)];
        return tileIndex;
    }

    private bool Pushrow()
    {
        if (BGFIFO.Count <= 8)
        {
            var buffer = new FIFOPixel[GraphicConstants.SpriteWidth];
            for (var i = GraphicConstants.SpriteWidth; i > 0; i--)
            {
                var pixel = new FIFOPixel((tileDataHigh.GetBit(i - 1), tileDataLow.GetBit(i - 1)) switch
                {
                    (false, false) => 0,
                    (false, true) => 1,
                    (true, false) => 2,
                    (true, true) => 3,
                });

                buffer[8 - i] = pixel;
            }

            BGFIFO.Push8(buffer);

            return true;
        }

        return false;
    }
}
