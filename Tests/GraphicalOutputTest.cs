
using NUnit.Framework;

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

        Assert.IsTrue(TestHelpers.AreEqual((Image<L8>)expectedImage, outputImage));
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

        Assert.IsTrue(TestHelpers.AreEqual((Image<L8>)expectedImage, outputImage));
    }
}
