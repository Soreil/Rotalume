using System;

namespace emulator
{
    public record FIFOPixel(byte color);
    public record FIFOSpritePixel(byte color, bool priority, int Palette) : FIFOPixel(color);

    public class PixelFetcher
    {
        const int tileWidth = 8;

        PPU p;
        private readonly FIFO<FIFOPixel> BGFIFO = new();
        private readonly FIFO<FIFOSpritePixel> SpriteFIFO = new();
        public PixelFetcher(PPU P)
        {
            p = P;
        }

        private int scanlineX = 0;

        byte tileIndex;
        int at;
        byte tileDataLow;
        byte tileDataHigh;

        public int FetcherStep = 0;
        public bool PushedEarly = false;

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
                    at = GetAdress();
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

        //Only uses the BG FIFO for now
        public Shade? RenderPixel()
        {
            if (BGFIFO.count != 0)
            {
                var pix = BGFIFO.Pop();
                //Do we need to pop in order to do this?
                //Do we need pixels in the fifo to do this?
                return p.BackgroundColor(p.BGDisplayEnable ? pix.color : 0);
            }
            return null;
        }

        private byte FetchHigh() => p.VRAM[at + 1];
        private byte FetchLow() => p.VRAM[at];

        private int GetAdress()
        {
            var tiledatamap = p.BGAndWindowTileDataSelect;

            int at;
            if (tiledatamap != 0x8800) at = tiledatamap + (tileIndex * 16) + ((((p.LY + p.SCY) & 0xff) & 7) * 2);
            else at = 0x9000 + (((sbyte)tileIndex) * 16) + ((((p.LY + p.SCY) & 0xff) & 7) * 2);
            return at;
        }

        bool AlreadyInWindow = false;
        public bool JustEnteredWindow = false;
        int WindowLY = 0;
        private byte FetchTileID()
        {
            ushort tilemap = ((p.BGTileMapDisplaySelect == 0x9c00 && scanlineX <= ((p.WX + 7) / 8) && p.LY >= p.WY) ||
            p.TileMapDisplaySelect == 0x9c00 && scanlineX >= ((p.WX + 7) / 8) && p.LY >= p.WY) ? 0x9c00 : 0x9800;
            bool inWindow = scanlineX >= (p.WX + 7) / 8 && p.LY >= p.WY && p.WindowDisplayEnable;
            if (inWindow && !AlreadyInWindow)
            {
                JustEnteredWindow = true;
                AlreadyInWindow = true;
            }
            if (!!inWindow && AlreadyInWindow) AlreadyInWindow = false;

            var windowStartX = p.WX - 7;
            var windowStartY = WindowLY;
            if (windowStartX < 0) windowStartX = 0;

            var tileX = inWindow ? (scanlineX / 8) - (windowStartX / 8) : ((p.SCX / 8) + (scanlineX / 8)) & 0x1f;
            var tileY = inWindow ? windowStartY : (p.LY + p.SCY) & 0xff;

            var tileIndex = p.VRAM[tilemap + tileX + ((tileY / 8) * 32)];
            return tileIndex;
        }

        private bool Pushrow()
        {
            if (BGFIFO.count == 0)
            {
                for (var i = tileWidth; i > 0; i--)
                {
                    var paletteIndex = tileDataLow.GetBit(i - 1) ? 1 : 0;
                    paletteIndex += tileDataHigh.GetBit(i - 1) ? 2 : 0;

                    BGFIFO.Push(new((byte)paletteIndex));
                }
                scanlineX += 8;
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