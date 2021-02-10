
using NUnit.Framework;

namespace Tests
{
    class FIFORenderer
    {
        [Test]
        public void RenderBackGroundTile()
        {
            //gradient from white to black and back
            byte[] expected = new byte[8] { 0, 1, 2, 3, 3, 2, 1, 0 };
            byte[] got = new byte[8];

            int clock = 0;
            var ppu = new emulator.PPU(() => clock, () => { }, () => { });
            var fetcher = new emulator.PixelFetcher(ppu);

            //black dark light white
            ppu.BGP = 0b11100100;
            //Screen and background on
            ppu.LCDC = 0b10010011;

            //gradient from white to black and back
            ppu.VRAM[emulator.VRAM.Start + 0] = 0b01011010;
            ppu.VRAM[emulator.VRAM.Start + 1] = 0b00111100;

            var elapsed = fetcher.Fetch();
            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(1, fetcher.FetcherStep);
            elapsed = fetcher.Fetch();
            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(2, fetcher.FetcherStep);
            elapsed = fetcher.Fetch();
            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(3, fetcher.FetcherStep);
            elapsed = fetcher.Fetch();
            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(0, fetcher.FetcherStep);

            for (int i = 0; i < 8; i++)
            {
                var s = fetcher.RenderPixel();
                Assert.NotNull(s);
                got[i] = (byte)s;
            }

            Assert.AreEqual(expected, got);
        }
    }
}