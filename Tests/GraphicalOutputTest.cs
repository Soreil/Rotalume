using emulator;

using NUnit.Framework;

using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tests;

internal class GraphicalOutputTest
{
    [Test]
    [Category("RequiresBootROM")]
    public void NintendoLogoShowsUpInTheCenterAtTheEndOfBooting()
    {
        IntPtr pointer = Marshal.AllocHGlobal(144 * 160);

        var fs = new FrameSink(() => pointer, () => { }, () => false);

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

    [TestCase(@"..\..\..\..\Tests\rom\blargg\cpu_instrs\cpu_instrs.gb", @"..\..\..\..\Tests\rom\blargg\cpu_instrs\dmg_cpu_instrs.png")]
    public void BlarggCPUTestsPasses(string romPath, string imagePath)
    {
        var render = new TestRenderDevice();

        var rom = File.ReadAllBytes(romPath);
        var expectedImage = Image.Load(imagePath);


        var core = TestHelpers.NewCore(rom, render);


        int FramesDrawn = 0;
        render.FramePushed += (sender, e) => FramesDrawn++;

        while (FramesDrawn != 4000)
            core.Step();

        var outputImage = Image.LoadPixelData<Rgb24>(render.Image, 160, 144);

        outputImage.SaveAsBmp("output.bmp");

        Assert.IsTrue(AreEqual((Image<Rgb24>)expectedImage, outputImage));
    }

    private static bool AreEqual(Image<Rgb24> expectedImage, Image<Rgb24> outputImage)
    {
        Rgb24[] pixelArray = new Rgb24[expectedImage.Width * expectedImage.Height];
        expectedImage.CopyPixelDataTo(pixelArray);

        Rgb24[] pixelArray2 = new Rgb24[outputImage.Width * outputImage.Height];
        outputImage.CopyPixelDataTo(pixelArray2);

        for (int i = 0; i < pixelArray2.Length; i++)
        {
            if (pixelArray[i] != pixelArray2[i]) 
                return false;
        }

        return true;
    }
}

internal class TestRenderDevice : IFrameSink
{
    private readonly byte[] backingBuffer;
    private int index;

    public TestRenderDevice()
    {
        backingBuffer = new byte[144 * 160];
        Image = new byte[144 * 160 * 3];
    }

    public event EventHandler? FramePushed;

    public void Draw()
    {
        for (int i = 0; i < backingBuffer.Length; i++)
        {
            var val = backingBuffer[i];
            Image[i * 3 + 0] = val;
            Image[i * 3 + 1] = val;
            Image[i * 3 + 2] = val;
        }

        index = 0;
        FramePushed?.Invoke(this, EventArgs.Empty);
    }

    public byte[] Image { get; set; }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        buffer.CopyTo(new Span<byte>(backingBuffer, index, buffer.Length));
        index += buffer.Length;
    }
}