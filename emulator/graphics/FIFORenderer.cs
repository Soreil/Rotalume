using System;

namespace emulator
{
    public class FIFORenderer
    {

    }

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

        int scanlineX = 0;

        byte tileIndex;
        int at;
        byte tileDataLow;
        byte tileDataHigh;
        public void Fetch()
        {
            tileIndex = FetchTileID();
            //clock advances 2 cycles

            at = GetAdress();
            tileDataLow = FetchLow();
            //clock advances 2 cycles

            tileDataHigh = FetchHigh();
            Pushrow();
            //clock advances 2 cycles

            Sleep();
            //clock advances 2 cycles

            Pushrow();
            //takes one cycle every time it is attempted, won't go back until succeeds
        }

        private byte FetchHigh() => p.VRAM[at + 1];
        private byte FetchLow() => p.VRAM[at];

        private int GetAdress()
        {
            var tiledatamap = p.BGAndWindowTileDataSelect;

            int at;
            if (tiledatamap != 0x8800) at = tiledatamap + (tileIndex * 16) + (p.LY * 2);
            else at = 0x9000 + (((sbyte)tileIndex) * 16) + (p.LY * 2);
            return at;
        }

        int WindowLY = 0;
        private byte FetchTileID()
        {
            ushort tilemap = ((p.BGTileMapDisplaySelect == 0x9c00 && scanlineX <= ((p.WX + 7) / 8) && p.LY >= p.WY) ||
            p.TileMapDisplaySelect == 0x9c00 && scanlineX >= ((p.WX + 7) / 8) && p.LY >= p.WY) ? 0x9c00 : 0x9800;
            bool inWindow = scanlineX >= (p.WX + 7) / 8 && p.LY >= p.WY;

            var windowStartX = p.WX - 7;
            var windowStartY = WindowLY++;
            if (windowStartX < 0) windowStartX = 0;

            var fetcherX = inWindow ? (scanlineX/8) - (windowStartX / 8) : ((p.SCX / 8) + scanlineX) & 0x1f;
            var fetcherY = inWindow ? windowStartY : (p.LY + p.SCY) & 0xff;

            var tileIndex = p.VRAM[tilemap + fetcherX + (fetcherY / 8 * 32)];
            return tileIndex;
        }

        private bool Pushrow()
        {
            if (BGFIFO.count == 8 || BGFIFO.count == 0)
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