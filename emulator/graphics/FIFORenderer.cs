using System;
using System.Collections.Generic;

namespace emulator
{
    public struct FIFOPixel
    {
        public readonly byte color;
        public FIFOPixel(byte color) => this.color = color;
    }
    public struct FIFOSpritePixel
    {
        public readonly int Palette;
        public readonly byte color;
        public readonly bool priority;

        public FIFOSpritePixel(byte paletteIndex, bool spriteToBackgroundPriority, int palette)
        {
            color = paletteIndex;
            priority = spriteToBackgroundPriority;
            Palette = palette;
        }
    }

    public class PixelFetcher
    {
        private const int tileWidth = 8;
        private readonly PPU p;
        public readonly FIFO<FIFOPixel> BGFIFO = new();
        private readonly FIFO<FIFOSpritePixel> SpriteFIFO = new();
        public PixelFetcher(PPU P) => p = P;

        public int scanlineX = 0;
        private byte tileIndex;
        private byte tileDataLow;
        private byte tileDataHigh;

        public int FetcherStep = 0;
        public bool PushedEarly = false;
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

        private bool doneWithFirstSpriteFetch = false;
        //Fetch runs one of the steps of the background fetcher and returns the amount of cycles used
        public int Fetch()
        {
            switch (FetcherStep)
            {
                case 0:
                //We only want this to happen on the second background tile fetch
                if (!doneWithFirstSpriteFetch && BGFIFO.count != 0)
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
                Sleep();
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
        public int SpriteCount = 0;
        public int SpritesFinished = 0;

        private bool PushSpriteRow(byte low, byte high, SpriteAttributes sprite)
        {
            if (SpriteFIFO.count <= 8)
            {
                for (var i = tileWidth; i > 0; i--)
                {
                    var paletteIndex = low.GetBit(i - 1) ? 1 : 0;
                    paletteIndex += high.GetBit(i - 1) ? 2 : 0;

                    var pos = sprite.XFlipped ? (i - 1) : tileWidth - i;
                    var existingSpritePixel = SpriteFIFO.At(pos);
                    var candidate = new FIFOSpritePixel((byte)paletteIndex, sprite.SpriteToBackgroundPriority, sprite.Palette);

                    if (shouldReplace(existingSpritePixel, candidate))
                    {
                        SpriteFIFO.Replace(pos, candidate);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool PushSpriteRowPartial(byte low, byte high, SpriteAttributes sprite, int count)
        {
            if (SpriteFIFO.count + count < 16)
            {
                for (var i = count; i > 0; i--)
                {
                    var paletteIndex = low.GetBit(i - 1) ? 1 : 0;
                    paletteIndex += high.GetBit(i - 1) ? 2 : 0;

                    var pos = sprite.XFlipped ? (i - 1) : count - i;
                    var existingSpritePixel = SpriteFIFO.At(pos);
                    var candidate = new FIFOSpritePixel((byte)paletteIndex, sprite.SpriteToBackgroundPriority, sprite.Palette);

                    if (shouldReplace(existingSpritePixel, candidate))
                    {
                        SpriteFIFO.Replace(pos, candidate);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool shouldReplace(FIFOSpritePixel existingSpritePixel, FIFOSpritePixel candidate)
        {
            if (candidate.color != 0 && existingSpritePixel.color == 0)
            {
                return true;
            }

            if (candidate.priority && !existingSpritePixel.priority)
            {
                return true;
            }

            return false;
        }

        public void RenderSpriteToTheLeftOfTheVisibleArea()
        {
            //Sprites are enabled and there is a sprite starting on the current X position
            if (p.OBJDisplayEnable && SpriteCount - SpritesFinished != 0 && ContainsSprite())
            {
                //We can't start the sprite fetching yet if the background fifo is empty
                if (BGFIFO.count == 0)
                {
                    throw new Exception("wrong stage");
                }

                var sprite = FirstMatchingSprite();

                var pixelsVisible = sprite.X;

                for (int i = SpriteFIFO.count; i < pixelsVisible; i = SpriteFIFO.count)
                {
                    SpriteFIFO.Push(new FIFOSpritePixel(0, false, 0));
                }

                var y = p.LY - (sprite.Y - 16);
                if (sprite.YFlipped)
                {
                    if (p.SpriteHeight == 8)
                    {
                        y = 7 - y;
                    }
                    else
                    {
                        y = 15 - y;
                    }
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
                if (BGFIFO.count == 0)
                {
                    return Shade.Empty;
                }

                for (int i = SpriteFIFO.count; i < 8; i = SpriteFIFO.count)
                {
                    SpriteFIFO.Push(new FIFOSpritePixel(0, false, 0));
                }

                var sprite = FirstMatchingSprite();

                var y = p.LY - (sprite.Y - 16);
                if (sprite.YFlipped)
                {
                    if (p.SpriteHeight == 8)
                    {
                        y = 7 - y;
                    }
                    else
                    {
                        y = 15 - y;
                    }
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

            if (BGFIFO.count != 0 && SpriteFIFO.count != 0)
            {
                var bp = BGFIFO.Pop();
                var sp = SpriteFIFO.Pop();
                if (sp.color != 0 && p.OBJDisplayEnable)
                {
                    //obj to bg priority bit is set to true so the sprite pixel
                    //will be behind bg color 1,2,3
                    if (sp.priority && bp.color != 0)
                    {
                        return p.BackgroundColor(p.BGDisplayEnable ? bp.color : 0);
                    }
                    else
                    {
                        if (sp.Palette == 0)
                        {
                            return p.SpritePalette0(sp.color);
                        }
                        else if (sp.Palette == 1)
                        {
                            return p.SpritePalette1(sp.color);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                }
                else
                {
                    return p.BackgroundColor(p.BGDisplayEnable ? bp.color : 0);
                }
            }
            else if (BGFIFO.count > 8)
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

            int at;
            if (tiledatamap != 0x8800)
            {
                at = tiledatamap + (tileIndex * 16) + ((((p.LY + p.SCY) & 0xff) & 7) * 2);
            }
            else
            {
                at = 0x9000 + (((sbyte)tileIndex) * 16) + ((((p.LY + p.SCY) & 0xff) & 7) * 2);
            }

            return at;
        }
        private byte FetchTileID()
        {
            int tilemap;
            bool inWindow = (scanlineX + BGFIFO.count) >= (p.WX - 7) && p.LY >= p.WY && p.WindowDisplayEnable;
            if (inWindow)
            {
                WindowLY.Add(p.LY);
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

            var tileX = inWindow ? ((scanlineX + BGFIFO.count) / 8) - (windowStartX / 8) :
                                   ((p.SCX / 8) + ((scanlineX + BGFIFO.count) / 8)) & 0x1f;
            var tileY = inWindow ? windowStartY :
                                   (p.LY + p.SCY) & 0xff;

            var tileIndex = p.VRAM[tilemap + tileX + ((tileY / 8) * 32)];
            return tileIndex;
        }

        private bool Pushrow()
        {
            if (BGFIFO.count <= 8)
            {
                for (var i = tileWidth; i > 0; i--)
                {
                    var paletteIndex = tileDataLow.GetBit(i - 1) ? 1 : 0;
                    paletteIndex += tileDataHigh.GetBit(i - 1) ? 2 : 0;

                    BGFIFO.Push(new((byte)paletteIndex));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Sleep()
        {
            return;
        }

    }
}