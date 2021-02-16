using System;
using System.Collections.Generic;

namespace emulator
{
    public record FIFOPixel(byte color);
    public record FIFOSpritePixel(byte color, bool priority, int Palette) : FIFOPixel(color);

    public class PixelFetcher
    {
        const int tileWidth = 8;
        readonly PPU p;
        public readonly FIFO<FIFOPixel> BGFIFO = new();
        private readonly FIFO<FIFOSpritePixel> SpriteFIFO = new();
        public PixelFetcher(PPU P)
        {
            p = P;
        }

        public int scanlineX = 0;

        byte tileIndex;
        byte tileDataLow;
        byte tileDataHigh;

        public int FetcherStep = 0;
        public bool PushedEarly = false;

        HashSet<int> WindowLY = new();

        //Line finished resets all state which is only relevant for a single line
        internal void LineFinished()
        {
            FetcherStep = 0;
            PushedEarly = false;
            scanlineX = 0;
            BGFIFO.Clear();
            SpriteFIFO.Clear();
        }

        //Frame finished resets all state relevant for an entire frame
        internal void FrameFinished()
        {
            LineFinished();
            WindowLY.Clear();
        }


        //Fetch runs one of the steps of the background fetcher and returns the amount of cycles used
        public int Fetch()
        {
            switch (FetcherStep)
            {
                case 0:
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

        public Shade? RenderPixel()
        {
            if (BGFIFO.count != 0 && SpriteFIFO.count > 8)
            {
                var bp = BGFIFO.Pop();
                var sp = SpriteFIFO.Pop();
                if (sp.color != 0 && p.OBJDisplayEnable)
                {
                    //obj to bg priority bit is set to true so the sprite pixel
                    //will be behind bg color 1,2,3
                    if (sp.priority && bp.color != 0)
                        return p.BackgroundColor(p.BGDisplayEnable ? bp.color : 0);
                    else
                    {
                        if (sp.Palette == 0)
                            return p.SpritePalette0(sp.color);
                        else if (sp.Palette == 1)
                            return p.SpritePalette1(sp.color);
                        else throw new Exception();
                    }

                }
                else return p.BackgroundColor(p.BGDisplayEnable ? bp.color : 0);
            }
            else if (BGFIFO.count > 8)
            {
                var pix = BGFIFO.Pop();
                //Do we need to pop in order to do this?
                //Do we need pixels in the fifo to do this?
                return p.BackgroundColor(p.BGDisplayEnable ? pix.color : 0);
            }
            else return null;
        }

        private byte FetchHigh() => p.VRAM[GetAdress() + 1];
        private byte FetchLow() => p.VRAM[GetAdress()];

        private int GetAdress()
        {
            var tiledatamap = p.BGAndWindowTileDataSelect;

            int at;
            if (tiledatamap != 0x8800) at = tiledatamap + (tileIndex * 16) + ((((p.LY + p.SCY) & 0xff) & 7) * 2);
            else at = 0x9000 + (((sbyte)tileIndex) * 16) + ((((p.LY + p.SCY) & 0xff) & 7) * 2);
            return at;
        }
        private byte FetchTileID()
        {
            int tilemap;
            bool inWindow = (scanlineX+BGFIFO.count) >= (p.WX - 7) && p.LY >= p.WY && p.WindowDisplayEnable;
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
            if (windowStartX < 0)
                windowStartX = 0;

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
            else return false;
        }

        private void Sleep()
        {
            return;
        }

    }

    public class FIFO<T>
    {
        public const int capacity = 16;
        public int count = 0;
        private readonly T[] buffer = new T[capacity];
        public void Clear() => count = 0;
        public void Push(T p) => buffer[count++] = p;
        public T Pop()
        {
            if (count == 0) throw new Exception("Empty FIFO");

            var res = buffer[0];

            count--;
            for (int i = 0; i < count; i++)
                buffer[i] = buffer[i + 1];

            return res;
        }
    }
}