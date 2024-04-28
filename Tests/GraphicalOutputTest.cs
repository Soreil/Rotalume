
using NUnit.Framework;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tests;

internal class GraphicalOutputTest
{
    [Test]
    [Category("RequiresBootROM")]
    [TestCase(@"rom\boot\expected.png")]
    public void NintendoLogoShowsUpInTheCenterAtTheEndOfBooting(string imagePath)
    {
        var render = new TestRenderDevice();

        var expectedImage = Image.Load(imagePath);

        var core = TestHelpers.NewBootCore(render);

        var outputDir = Directory.CreateDirectory(nameof(NintendoLogoShowsUpInTheCenterAtTheEndOfBooting));

        int FramesDrawn = 0;
        render.FramePushed += async (sender, e) =>
        {
            var img = Image.LoadPixelData<L8>(render.Image, 160, 144);
            await img.SaveAsBmpAsync(Path.Combine(outputDir.FullName, $"output{FramesDrawn}.bmp"));
            FramesDrawn++;
        };

        while (core.CPU.PC != 0x100)
            core.Step();

        var outputImage = Image.LoadPixelData<L8>(render.Image, 160, 144);

        Console.WriteLine($"Wrote debug image for bootrom to:{outputDir.FullName}");
        outputImage.SaveAsBmp(Path.Combine(outputDir.FullName, "outputBootROM.bmp"));

        //For some reason we are missing the (R) part of the image on the righthand side here.
        Assert.That(TestHelpers.AreEqual((Image<L8>)expectedImage, outputImage), Is.True);
    }

    [TestCase(@"rom\blargg\cpu_instrs\cpu_instrs.gb", @"..\..\..\..\Tests\rom\blargg\cpu_instrs\expected.png", "outputBlargCPUTest.bmp", 4000)]
    [TestCase(@"rom\blargg\instr_timing\instr_timing.gb", @"..\..\..\..\Tests\rom\blargg\instr_timing\expected.png", "outputINSTTiming.bmp", 100)]
    [TestCase(@"rom\blargg\mem_timing\mem_timing.gb", @"..\..\..\..\Tests\rom\blargg\mem_timing\expected.png", "outputMEMTiming.bmp", 100)]
    [TestCase(@"rom\blargg\mem_timing-2\mem_timing.gb", @"..\..\..\..\Tests\rom\blargg\mem_timing-2\expected.png", "outputMEMTiming2.bmp", 200)]
    [TestCase(@"rom\blargg\halt_bug\halt_bug.gb", @"..\..\..\..\Tests\rom\blargg\halt_bug\expected.png", "outputHaltBug.bmp", 300)]

    [TestCase(@"rom\dmg-acid2\dmg-acid2.gb", @"..\..\..\..\Tests\rom\dmg-acid2\expected.png", "outputDMG-ACID2.bmp", 10)]

    [TestCase(@"rom\mooneye-test-suite\acceptance\oam_dma\basic.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputBasicOAM.bmp", 10)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\oam_dma\reg_read.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputRegReadOAM.bmp", 10)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\oam_dma\sources-GS.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputSourcesGS.bmp", 10)]

    [TestCase(@"rom\mooneye-test-suite\acceptance\bits\mem_oam.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\expected.png", "outputMEMOAM.bmp", 100)]
    //[TestCase(@"rom\mooneye-test-suite\acceptance\bits\unused_hwio-GS.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\expected.png", "outputUnusedHWIO.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\bits\reg_f.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\expected_regf.png", "outputRegF.bmp", 100)]

    [TestCase(@"rom\mooneye-test-suite\acceptance\instr\daa.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\instr\expected.png", "outputDAA.bmp", 100)]

    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\div_write.gb", @"rom\mooneye-test-suite\acceptance\timer\div_write.png", "div_write.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\rapid_toggle.gb", @"rom\mooneye-test-suite\acceptance\timer\rapid_toggle.png", "rapid_toggle.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim00.gb", @"rom\mooneye-test-suite\acceptance\timer\tim00.png", "tim00.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim00_div_trigger.gb", @"rom\mooneye-test-suite\acceptance\timer\tim00_div_trigger.png", "tim00_div_trigger.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim01.gb", @"rom\mooneye-test-suite\acceptance\timer\tim01.png", "tim01.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim01_div_trigger.gb", @"rom\mooneye-test-suite\acceptance\timer\tim01_div_trigger.png", "tim01_div_trigger.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim10.gb", @"rom\mooneye-test-suite\acceptance\timer\tim10.png", "tim10.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim10_div_trigger.gb", @"rom\mooneye-test-suite\acceptance\timer\tim10_div_trigger.png", "tim10_div_trigger.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim11.gb", @"rom\mooneye-test-suite\acceptance\timer\tim11.png", "tim11.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tim11_div_trigger.gb", @"rom\mooneye-test-suite\acceptance\timer\tim11_div_trigger.png", "tim11_div_trigger.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tima_reload.gb", @"rom\mooneye-test-suite\acceptance\timer\tima_reload.png", "tima_reload.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tima_write_reloading.gb", @"rom\mooneye-test-suite\acceptance\timer\tima_write_reloading.png", "tima_write_reloading.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\timer\tma_write_reloading.gb", @"rom\mooneye-test-suite\acceptance\timer\tma_write_reloading.png", "tma_write_reloading.bmp", 100)]


    public void TestFrameMatchesExpectedFrame(string romPath, string imagePath, string outputFile, int frameToCheck)
    {
        var render = new TestRenderDevice();

        var rom = File.ReadAllBytes(romPath);
        var expectedImage = Image.Load(imagePath);

        var core = TestHelpers.NewCore(rom, Path.GetFileNameWithoutExtension(romPath), render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) => FramesDrawn++;

        while (FramesDrawn != frameToCheck)
            core.Step();
        core.Dispose();

        var outputImage = Image.LoadPixelData<L8>(render.Image, 160, 144);

        outputImage.SaveAsBmp(outputFile);

        Assert.That(TestHelpers.AreEqual((Image<L8>)expectedImage, outputImage), Is.True);
    }
}
