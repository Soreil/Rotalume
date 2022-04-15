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
    [TestCase(@"..\..\..\..\Tests\rom\boot\expected.png")]
    public void NintendoLogoShowsUpInTheCenterAtTheEndOfBooting(string imagePath)
    {
        var render = new TestRenderDevice();

        var expectedImage = Image.Load(imagePath);

        var core = TestHelpers.NewBootCore(render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) => FramesDrawn++;

        while (core.CPU.PC != 0x100)
            core.Step();

        var outputImage = Image.LoadPixelData<L8>(render.Image, 160, 144);

        outputImage.SaveAsBmp("outputBootROM.bmp");

        Assert.IsTrue(AreEqual((Image<L8>)expectedImage, outputImage));
    }

    private void DrewAFrame(object? sender, EventArgs e)
    {
    }

    [TestCase(@"..\..\..\..\Tests\rom\blargg\cpu_instrs\cpu_instrs.gb", @"..\..\..\..\Tests\rom\blargg\cpu_instrs\expected.png")]
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

        var outputImage = Image.LoadPixelData<L8>(render.Image, 160, 144);

        outputImage.SaveAsBmp("outputBlargCPUTest.bmp");

        Assert.IsTrue(AreEqual((Image<L8>)expectedImage, outputImage));
    }

    private static bool AreEqual(Image<L8> expectedImage, Image<L8> outputImage)
    {
        L8[] pixelArray = new L8[expectedImage.Width * expectedImage.Height];
        expectedImage.CopyPixelDataTo(pixelArray);

        L8[] pixelArray2 = new L8[outputImage.Width * outputImage.Height];
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
        Image = new byte[144 * 160];
    }

    public event EventHandler? FramePushed;

    public void Draw()
    {
        backingBuffer.CopyTo(Image,0);

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