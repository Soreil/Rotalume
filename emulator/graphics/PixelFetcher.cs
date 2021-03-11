using System;
using System.Collections.Generic;

namespace emulator
{

    public class PixelFetcher
    {
        private const int tileWidth = 8;
        private readonly PPU p;
        public readonly FIFO<FIFOPixel> BGFIFO = new();
        private readonly FIFO<FIFOSpritePixel> SpriteFIFO = new();
        public PixelFetcher(PPU P) => p = P;

        public int scanlineX;
        private byte tileIndex;
        private byte tileDataLow;
        private byte tileDataHigh;

        public int FetcherStep;
        public bool PushedEarly;
        private readonly HashSet<int> WindowLY = new();

        //Line finished resets all state which is only relevant for a single line
        internal void LineFinished()
        {
            FetcherStep = 0;
            PushedEarly = false;
            scanlineX = 0;
            doneWithFirstSpriteFetch = false;
            BGFIFO.Clear();
            SpriteFIFO.Clear();
        }

        //Frame finished resets all state relevant for an entire frame
        internal void FrameFinished()
        {
            LineFinished();
            WindowLY.Clear();
        }

        private bool doneWithFirstSpriteFetch;
        //Fetch runs one of the steps of the background fetcher and returns the amount of cycles used
        public int Fetch()
        {
            switch (FetcherStep)
            {
                case 0:
                //We only want this to happen on the second background tile fetch
                if (!doneWithFirstSpriteFetch && BGFIFO.Count != 0)
                {
                    RenderSpriteToTheLeftOfTheVisibleArea();
                    doneWithFirstSpriteFetch = true;
                }

                tileIndex = FetchTileID();
                FetcherStep = 1;
                return 2;
                case 1:
                tileDataLow = FetchLow();
                FetcherStep = 2;
                return 2;
                case 2:
                tileDataHigh = FetchHigh();
                PushedEarly = Pushrow();
                FetcherStep = 3;
                return 2;
                case 3:
                //Sleep();
                FetcherStep = PushedEarly ? 0 : 4;
                return 2;
                case 4:
                FetcherStep = Pushrow() ? 0 : 4;
                return 1;
                default:
                throw new Exception("Illegal fetcher state");
            }
        }

        public SpriteAttributes[] SpriteAttributes = new SpriteAttributes[10];
        public int SpriteCount;
        public int SpritesFinished;

        private void PushSpriteRow(byte low, byte high, SpriteAttributes sprite)
        {
            if (SpriteFIFO.Count <= 8)
            {
                for (var i = tileWidth; i > 0; i--)
                {
                    var paletteIndex = low.GetBit(i - 1) ? 1 : 0;
                    paletteIndex += high.GetBit(i - 1) ? 2 : 0;

                    var pos = sprite.XFlipped ? (i - 1) : tileWidth - i;
                    var existingSpritePixel = SpriteFIFO.At(pos);
                    var candidate = new FIFOSpritePixel((byte)paletteIndex, sprite.SpriteToBackgroundPriority, sprite.Palette);

                    if (ShouldReplace(existingSpritePixel, candidate))
                    {
                        SpriteFIFO.Replace(pos, candidate);
                    }
                }
            }
        }
        private void PushSpriteRowPartial(byte low, byte high, SpriteAttributes sprite, int count)
        {
            if (SpriteFIFO.Count + count < 16)
            {
                for (var i = count; i > 0; i--)
                {
                    var paletteIndex = low.GetBit(i - 1) ? 1 : 0;
                    paletteIndex += high.GetBit(i - 1) ? 2 : 0;

                    var pos = sprite.XFlipped ? (i - 1) : count - i;
                    var existingSpritePixel = SpriteFIFO.At(pos);
                    var candidate = new FIFOSpritePixel((byte)paletteIndex, sprite.SpriteToBackgroundPriority, sprite.Palette);

                    if (ShouldReplace(existingSpritePixel, candidate))
                    {
                        SpriteFIFO.Replace(pos, candidate);
                    }
                }
            }
        }

        private static bool ShouldReplace(FIFOSpritePixel existingSpritePixel, FIFOSpritePixel candidate) => candidate.color != 0 && existingSpritePixel.color == 0 || candidate.priority && !existingSpritePixel.priority;

        public void RenderSpriteToTheLeftOfTheVisibleArea()
        {
            //Sprites are enabled and there is a sprite starting on the current X position
            if (p.OBJDisplayEnable && SpriteCount - SpritesFinished != 0 && ContainsSprite())
            {
                //We can't start the sprite fetching yet if the background fifo is empty
                if (BGFIFO.Count == 0)
                {
                    throw new Exception("wrong stage");
                }

                var sprite = FirstMatchingSprite();

                var pixelsVisible = sprite.X;

                for (int i = SpriteFIFO.Count; i < pixelsVisible; i = SpriteFIFO.Count)
                {
                    SpriteFIFO.Push(new FIFOSpritePixel(0, false, 0));
                }

                var y = p.LY - (sprite.Y - 16);
                if (sprite.YFlipped)
                {
                    y = p.SpriteHeight == 8 ? 7 - y : 15 - y;
                }

                var ID = p.SpriteHeight == 8 ? sprite.ID : sprite.ID & 0xfe;
                var addr = 0x8000 + ID * 16 + (2 * y);
                var low = p.VRAM[addr];
                var high = p.VRAM[addr + 1];
                PushSpriteRowPartial(low, high, sprite, pixelsVisible);
                SpritesFinished++;
            }
        }

        public Shade RenderPixel()
        {
            //Sprites are enabled and there is a sprite starting on the current X position
            if (p.OBJDisplayEnable && SpriteCount - SpritesFinished != 0 && ContainsSprite())
            {
                //We can't start the sprite fetching yet if the background fifo is empty
                if (BGFIFO.Count == 0)
                {
                    return Shade.Empty;
                }

                for (int i = SpriteFIFO.Count; i < 8; i = SpriteFIFO.Count)
                {
                    SpriteFIFO.Push(new FIFOSpritePixel(0, false, 0));
                }

                var sprite = FirstMatchingSprite();

                var y = p.LY - (sprite.Y - 16);
                if (sprite.YFlipped)
                {
                    y = p.SpriteHeight == 8 ? 7 - y : 15 - y;
                }

                if (y < 0)
                {
                    throw new Exception("Illegal Y position in sprite");
                }

                var ID = p.SpriteHeight == 8 ? sprite.ID : sprite.ID & 0xfe;
                var addr = 0x8000 + ID * 16 + (2 * y);
                var low = p.VRAM[addr];
                var high = p.VRAM[addr + 1];
                PushSpriteRow(low, high, sprite);
                SpritesFinished++;
            }

            if (BGFIFO.Count != 0 && SpriteFIFO.Count != 0)
            {
                var bp = BGFIFO.Pop();
                var sp = SpriteFIFO.Pop();
                if (sp.color != 0 && p.OBJDisplayEnable)
                {
                    //obj to bg priority bit is set to true so the sprite pixel
                    //will be behind bg color 1,2,3
                    return sp.priority && bp.color != 0
                        ? p.BackgroundColor(p.BGDisplayEnable ? bp.color : 0)
                        : sp.Palette switch
                        {
                            0 => p.SpritePalette0(sp.color),
                            1 => p.SpritePalette1(sp.color),
                            _ => throw new Exception()
                        };

                }
                else
                {
                    return p.BackgroundColor(p.BGDisplayEnable ? bp.color : 0);
                }
            }
            else if (BGFIFO.Count > 8)
            {
                var pix = BGFIFO.Pop();
                //Do we need to pop in order to do this?
                //Do we need pixels in the fifo to do this?
                return p.BackgroundColor(p.BGDisplayEnable ? pix.color : 0);
            }
            else
            {
                return Shade.Empty;
            }
        }

        private bool ContainsSprite()
        {
            var wanted = scanlineX + 8 - (p.SCX & 7);
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
            var wanted = scanlineX + 8 - (p.SCX & 7);
            for (int i = SpritesFinished; i < SpriteCount; i++)
            {
                if (SpriteAttributes[i].X == wanted)
                {
                    return SpriteAttributes[i];
                }
            }
            throw new Exception("Illegal call");
        }

        private byte FetchHigh() => p.VRAM[GetAdress() + 1];

        private byte FetchLow() => p.VRAM[GetAdress()];

        private int GetAdress()
        {
            var tiledatamap = p.BGAndWindowTileDataSelect;

            if (inWindow)
                return tiledatamap != 0x8800
                    ? tiledatamap + (tileIndex * 16) + (((WindowLY.Count - 1) & 7) * 2)
                    : 0x9000 + (((sbyte)tileIndex) * 16) + (((WindowLY.Count - 1) & 7) * 2);
            else
                return tiledatamap != 0x8800
                    ? tiledatamap + (tileIndex * 16) + (((p.LY + p.SCY) & 0xff & 7) * 2)
                    : 0x9000 + (((sbyte)tileIndex) * 16) + (((p.LY + p.SCY) & 0xff & 7) * 2);
        }

        private bool inWindow;
        private byte FetchTileID()
        {
            int tilemap;
            inWindow = (scanlineX + BGFIFO.Count) >= (p.WX - 7) && p.LY >= p.WY && p.WindowDisplayEnable;
            if (inWindow)
            {
                _ = WindowLY.Add(p.LY);
                tilemap = p.TileMapDisplaySelect;
            }
            else
            {
                tilemap = p.BGTileMapDisplaySelect;
            }

            var windowStartX = p.WX - 7;
            var windowStartY = WindowLY.Count - 1;

            //TODO: handle tick cost of this condition
            if (windowStartX < 0)
            {
                windowStartX = 0;
            }

            var tileX = inWindow ? ((scanlineX + BGFIFO.Count) / 8) - (windowStartX / 8) :
                                   ((p.SCX / 8) + ((scanlineX + BGFIFO.Count) / 8)) & 0x1f;
            var tileY = inWindow ? windowStartY :
                                   (p.LY + p.SCY) & 0xff;

            var tileIndex = p.VRAM[tilemap + tileX + ((tileY / 8) * 32)];
            return tileIndex;
        }

        private bool Pushrow()
        {
            if (BGFIFO.Count <= 8)
            {
                for (var i = tileWidth; i > 0; i--)
                {
                    var paletteIndex = tileDataLow.GetBit(i - 1) ? 1 : 0;
                    paletteIndex += tileDataHigh.GetBit(i - 1) ? 2 : 0;

                    BGFIFO.Push(new((byte)paletteIndex));
                }
                return true;
            }

            return false;
        }
    }
}