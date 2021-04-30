using NUnit.Framework;

using System;
using System.Runtime.InteropServices;

namespace Tests
{
    class GraphicalOutputTest
    {
        [Test]
        public void NintendoLogoShowsUpInTheCenterAtTheEndOfBooting()
        {
            IntPtr pointer = Marshal.AllocHGlobal(144 * 160);

            var fs = new emulator.FrameSink(() => pointer, () => { }, () => false);

            fs.FramePushed += DrewAFrame;

            var c = TestHelpers.NewBootCore(fs);

            while (c.CPU.PC != 0x100) c.Step();

            ReadOnlySpan<byte> image;
            unsafe
            {
                image = new ReadOnlySpan<byte>((void*)pointer, 144 * 160);
            }

            var logoHeight = 2 * 8;
            //var logoWidth = 13 * 8;
            const int logoOffsetX = 64;
            //const int logoOffsetY = 32;
            const int rowWidth = 160;

            for (var initialRow = logoOffsetX; initialRow < logoOffsetX + logoHeight; initialRow++)
            {
                var s = image.Slice(initialRow * rowWidth, rowWidth);
                Assert.IsTrue(s.Contains((byte)0));
            }
        }

        private void DrewAFrame(object? sender, EventArgs e)
        {
        }
    }
}
