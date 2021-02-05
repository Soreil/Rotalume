using System;

namespace emulator
{
    public class FIFORenderer
    {

    }

    public record FIFOPixel(byte color, bool priority);
    public record FIFOSpritePixel(byte color, bool priority, byte[] Palette) : FIFOPixel(color, priority);

    public class PixelFetcher
    {
        PPU p;
        private readonly FIFO<FIFOPixel> BGFIFO = new();
        private readonly FIFO<FIFOSpritePixel> SpriteFIFO = new();
        public PixelFetcher(PPU P)
        {
            p = P;
        }

        int scanlineX = 0;

        public void Fetch()
        {
            ushort tilemap = ((p.BGTileMapDisplaySelect == 0x9c00 && (scanlineX <= ((p.WX + 7) / 8) && p.LY >= p.WY)) ||
            (p.TileMapDisplaySelect == 0x9c00 && scanlineX >= ((p.WX + 7) / 8)) && p.LY >= p.WY) ? 0x9c00 : 0x9800;
            bool inWindow = scanlineX >= (p.WX + 7) / 8 && p.LY >= p.WY;

            var fetcherX = inWindow ? scanlineX : ((p.SCX / 8) + scanlineX) & 0x1f;
            var fetcherY = inWindow ? p.LY : (p.LY + p.SCY) & 0xff;

            var tileIndex = p.VRAM[tilemap + fetcherX + ((fetcherY / 8) * 32)];
            //clock advances 2 cycles

            var tiledatamap = p.BGAndWindowTileDataSelect;

            int at = 0;
            if (tiledatamap != 0x8800) at = tiledatamap + (tileIndex * 16) + (p.LY * 2);
            else at = 0x9000 + (((sbyte)tileIndex) * 16) + (p.LY * 2);

            var tileDataLow = p.VRAM[at];
            //clock advances 2 cycles

            var tileDataHigh = p.VRAM[at + 1];
            pushrow();
            //clock advances 2 cycles

            sleep();
            //clock advances 2 cycles

            pushrow();
            //takes one cycle every time it is attempted, won't go back until succeeds
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
            {
                buffer[i] = buffer[i + 1];
            }

            return res;
        }
    }
}