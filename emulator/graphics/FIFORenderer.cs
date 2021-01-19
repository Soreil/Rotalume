namespace emulator
{
    public class FIFORenderer
    {

    }

    public record FIFOPixel(byte color, bool priority);
    public record FIFOSpritePixel(byte color, bool priority, byte[] Palette) : FIFOPixel(color, priority);

    public class Fetcher
    {
        PPU p;
        public Fetcher(PPU P)
        {
            p = P;
        }

        int scanlineX = 0;

        public void Fetch()
        {
            ushort tilemap = ((p.BGTileMapDisplaySelect == 0x9c00 && scanlineX <= ((p.WX + 7) / 8)) ||
            (p.TileMapDisplaySelect == 0x9c00 && scanlineX >= ((p.WX + 7) / 8))) ? 0x9c00 : 0x9800;
            bool inWindow = scanlineX >= (p.WX + 7) / 8;

            var fetcherX = inWindow ? scanlineX : ((p.SCX / 8) + scanlineX) & 0x1f;
            var fetcherY = inWindow ? p.LY : (p.LY + p.SCY) & 0xff;

            var tileIndex = p.VRAM[tilemap + fetcherX + ((fetcherY / 8) * 32)];
            //clock advances 2 cycles

            var tiledatamap = p.BGAndWindowTileDataSelect;
            var tileDataLow = p.VRAM[tiledatamap + (tileIndex * 2)];
            //clock advances 2 cycles

            var tileDataHigh = p.VRAM[tiledatamap + (tileIndex * 2) + 1];
            pushrow();
            //clock advances 2 cycles

            pushrow();
            //clock advances 2 cycles

            sleep();
            //clock advances 2 cycles
        }
    }
    public abstract class FIFO
    {
        public abstract void Clear();

    }
}