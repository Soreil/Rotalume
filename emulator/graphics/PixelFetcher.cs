namespace emulator;

public class PixelFetcher
{
    private readonly FIFO<FIFOPixel> BGFIFO = new();
    private readonly FIFO<FIFOSpritePixel> SpriteFIFO = new();
    public PixelFetcher(PPU p, VRAM vram, OAM oam)
    {
        ppu = p;
        VRAM = vram;
        OAM = oam;
    }

    private int scanlineX;
    private byte tileIndex;
    private byte tileDataLow;
    private byte tileDataHigh;

    private int FetcherStep;
    private bool PushedEarly;
    private readonly HashSet<int> WindowLY = new();

    //Line finished resets all state which is only relevant for a single line
    internal void LineFinished()
    {
        FetcherStep = 0;
        PushedEarly = false;
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
            for (var i = graphics.Constants.SpriteWidth; i > 0; i--)
            {
                var paletteIndex = Convert.ToByte(low.GetBit(i - 1));
                paletteIndex |= (byte)(Convert.ToByte(high.GetBit(i - 1)) << 1);

                var pos = sprite.XFlipped ? (i - 1) : graphics.Constants.SpriteWidth - i;
                var existingSpritePixel = SpriteFIFO.At(pos);
                var candidate = new FIFOSpritePixel(paletteIndex, sprite.SpriteToBackgroundPriority, sprite.Palette);

                if (ShouldReplace(existingSpritePixel, candidate))
                {
                    SpriteFIFO.Replace(pos, candidate);
                }
            }
        }
    }

    private static bool ShouldReplace(FIFOSpritePixel existingSpritePixel, FIFOSpritePixel candidate) => (candidate.color != 0 && existingSpritePixel.color == 0) || (candidate.priority && !existingSpritePixel.priority);

    public Shade? RenderPixel()
    {
        //Sprites are enabled and there is a sprite starting on the current X position
        //We can't start the sprite fetching yet if the background fifo is empty
        if (CanRenderASprite())
        {
            PushSpriteRowToPixelFetcher();
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
            return ppu.BackgroundColor(ppu.BGDisplayEnable ? pix.color : 0);
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
        if (sp.color != 0 && ppu.OBJDisplayEnable)
        {
            //obj to bg priority bit is set to true so the sprite pixel
            //will be behind bg color 1,2,3
            return sp.priority && bp.color != 0
                ? ppu.BackgroundColor(ppu.BGDisplayEnable ? bp.color : 0)
                : sp.Palette switch
                {
                    0 => ppu.SpritePalette0(sp.color),
                    1 => ppu.SpritePalette1(sp.color),
                    _ => throw new IllegalSpritePalette()
                };

        }
        else
        {
            return ppu.BackgroundColor(ppu.BGDisplayEnable ? bp.color : 0);
        }
    }

    private bool FIFOsNotEmpty() => BGFIFO.Count != 0 && SpriteFIFO.Count != 0;

    private void PushSpriteRowToPixelFetcher()
    {
        //Fill the fifo lower half with transparant pixels
        for (int i = SpriteFIFO.Count; i < graphics.Constants.SpriteWidth; i = SpriteFIFO.Count)
        {
            SpriteFIFO.Push(new FIFOSpritePixel(0, false, 0));
        }

        var sprite = FirstMatchingSprite();

        //16 pixel offset before lines can be offscreen taken out
        var y = ppu.LY - (sprite.Y - graphics.Constants.DoubleSpriteHeight);
        if (sprite.YFlipped)
        {
            y = ppu.SpriteHeight == 8 ? 7 - y : 15 - y;
        }

        if (y < 0)
        {
            throw new SpriteDomainError("Illegal Y position in sprite");
        }

        var ID = ppu.SpriteHeight == 8 ? sprite.ID : sprite.ID & 0xfe;
        var addr = 0x8000 + ID * graphics.Constants.BitsPerSpriteTile + (2 * y);
        var low = VRAM[addr];
        var high = VRAM[addr + 1];
        PushSpriteRow(low, high, sprite);
        SpritesFinished++;
    }

    private bool CanRenderASprite() => ppu.OBJDisplayEnable && SpriteCount - SpritesFinished != 0 && ContainsSprite() && BGFIFO.Count != 0;

    private int PixelsPopped;
    public int PixelsSentToLCD;
    public readonly Shade[] LineShadeBuffer = new Shade[graphics.Constants.ScreenWidth];
    internal void AttemptToPushAPixel()
    {
        var pix = RenderPixel();
        if (pix is null) return;

        PixelsPopped++;
        scanlineX++;

        if (PixelsPopped > (ppu.SCX & 7))
        {
            LineShadeBuffer[PixelsSentToLCD++] = (Shade)pix;
        }

        bool windowStart = PixelsSentToLCD == ppu.WX - 7 && ppu.LY >= ppu.WY && ppu.WindowDisplayEnable;
        if (windowStart)
        {
            FetcherStep = 0;
            BGFIFO.Clear();
        }
    }

    public void GetSprites()
    {
        SpriteCount = OAM.SpritesOnLine(SpriteAttributes, ppu.LY, ppu.SpriteHeight);
        SpritesFinished = 0;
    }

    private bool ContainsSprite()
    {
        var wanted = scanlineX + 8 - (ppu.SCX & 7);
        for (int i = SpritesFinished; i < SpriteCount; i++)
        {
            if (SpriteAttributes[i].X == wanted)
            {
                return true;
            }
        }
        return false;
    }

    private SpriteAttributes FirstMatchingSprite()
    {
        var wanted = scanlineX + 8 - (ppu.SCX & 7);
        for (int i = SpritesFinished; i < SpriteCount; i++)
        {
            if (SpriteAttributes[i].X == wanted)
            {
                return SpriteAttributes[i];
            }
        }
        throw new NoMatchingSprites("Illegal call");
    }

    private byte FetchHigh() => VRAM[GetAdress() + 1];

    private byte FetchLow() => VRAM[GetAdress()];

    private int GetAdress()
    {
        var tiledatamap = ppu.BGAndWindowTileDataSelect;

        return inWindow
            ? tiledatamap == 0x8000
                ? tiledatamap + (tileIndex * 16) + (((WindowLY.Count - 1) & 7) * 2)
                : 0x9000 + (((sbyte)tileIndex) * 16) + (((WindowLY.Count - 1) & 7) * 2)
            : tiledatamap == 0x8000
                ? tiledatamap + (tileIndex * 16) + (((ppu.LY + ppu.SCY) & 0xff & 7) * 2)
                : 0x9000 + (((sbyte)tileIndex) * 16) + (((ppu.LY + ppu.SCY) & 0xff & 7) * 2);
    }

    private bool inWindow;

    public PPU ppu { get; }
    public VRAM VRAM { get; }
    public OAM OAM { get; }

    private byte FetchTileID()
    {
        int tilemap;
        inWindow = (scanlineX + BGFIFO.Count) >= (ppu.WX - 7) && ppu.LY >= ppu.WY && ppu.WindowDisplayEnable;
        if (inWindow)
        {
            _ = WindowLY.Add(ppu.LY);
            tilemap = ppu.TileMapDisplaySelect;
        }
        else
        {
            tilemap = ppu.BGTileMapDisplaySelect;
        }

        var windowStartX = ppu.WX - 7;
        var windowStartY = WindowLY.Count - 1;

        //TODO: handle tick cost of this condition
        if (windowStartX < 0)
        {
            windowStartX = 0;
        }

        var tileX = inWindow ? ((scanlineX + BGFIFO.Count) / 8) - (windowStartX / 8) :
                               ((ppu.SCX / 8) + ((scanlineX + BGFIFO.Count) / 8)) & 0x1f;
        var tileY = inWindow ? windowStartY :
                               (ppu.LY + ppu.SCY) & 0xff;

        var tileIndex = VRAM[tilemap + tileX + ((tileY / 8) * 32)];
        return tileIndex;
    }

    private bool Pushrow()
    {
        if (BGFIFO.Count <= 8)
        {
            for (var i = graphics.Constants.SpriteWidth; i > 0; i--)
            {
                var paletteIndex = Convert.ToByte(tileDataLow.GetBit(i - 1));
                paletteIndex |= (byte)(Convert.ToByte(tileDataHigh.GetBit(i - 1)) << 1);

                BGFIFO.Push(new(paletteIndex));
            }
            return true;
        }

        return false;
    }
}
