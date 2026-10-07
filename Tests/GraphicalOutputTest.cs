using emulator.opcodes;

using ImageMagick;

using NUnit.Framework;

using System.Diagnostics;

namespace Tests;

internal class GraphicalOutputTest
{
    [Test]
    [Category("RequiresBootROM")]
    [TestCase(@"rom\boot\expected.png")]
    public void NintendoLogoShowsUpInTheCenterAtTheEndOfBooting(string imagePath)
    {
        var render = new TestRenderDevice();

        using var expectedImage = new MagickImage(imagePath);

        var core = TestHelpers.NewBootCore(render);

        var outputDir = Directory.CreateDirectory(nameof(NintendoLogoShowsUpInTheCenterAtTheEndOfBooting));

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) =>
        {
            // Read image that has no predefined dimensions.
            var settings = new MagickReadSettings
            {
                Width = 160,
                Height = 144,
                Format = MagickFormat.Gray
            };
            using var img = new MagickImage(render.Image, settings);

            img.Write(Path.Combine(outputDir.FullName, $"output{FramesDrawn}.bmp"), MagickFormat.Bmp);
            FramesDrawn++;
        };

        while (core.CPU.PC != 0x100)
            core.Step();

        var settings = new MagickReadSettings
        {
            Width = 160,
            Height = 144,
            Format = MagickFormat.Gray
        };
        using var outputImage = new MagickImage(render.Image, settings);

        Console.WriteLine($"Wrote debug image for bootrom to:{outputDir.FullName}");
        outputImage.Write(Path.Combine(outputDir.FullName, "outputBootROM.bmp"), MagickFormat.Bmp);

        Assert.That(ImageComparer.AreImagesEqual(expectedImage, outputImage), Is.True);
    }

    [Test]
    [Category("RequiresBootROM")]
    public void BootromStateMatchesExpected()
    {
        var render = new TestRenderDevice();

        var core = TestHelpers.NewBootCore(render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) =>
        {
            // Read image that has no predefined dimensions.
            var settings = new MagickReadSettings
            {
                Width = 160,
                Height = 144,
                Format = MagickFormat.Gray
            };

            FramesDrawn++;
        };

        while (core.CPU.PC != 0x100)
            core.Step();

        var state = core.SerializeState();
        Assert.That(state.CPU.PC, Is.EqualTo(0x100));
        Assert.That(state.CPU.Registers.AF & 0xfff0, Is.EqualTo(0x01b0));
        Assert.That(state.CPU.Registers.BC, Is.EqualTo(0x0013));
        Assert.That(state.CPU.Registers.DE, Is.EqualTo(0x00d8));
        Assert.That(state.CPU.Registers.HL, Is.EqualTo(0x014d));
        Assert.That(state.CPU.Registers.SP, Is.EqualTo(0xfffe));

    }

    [TestCase(@"rom\dmg-acid2\dmg-acid2.gb", @"..\..\..\..\Tests\rom\dmg-acid2\expected.png", "outputDMG-ACID2.bmp", 100)]

    [TestCase(@"rom\mooneye-test-suite\acceptance\oam_dma\basic.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputBasicOAM.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\oam_dma\reg_read.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputRegReadOAM.bmp", 100)]
    [TestCase(@"rom\mooneye-test-suite\acceptance\oam_dma\sources-GS.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputSourcesGS.bmp", 100)]

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


    public async Task TestFrameMatchesExpectedFrame(string romPath, string imagePath, string outputFile, int frameToCheck)
    {
        var render = new TestRenderDevice();

        var rom = File.ReadAllBytes(romPath);
        var expectedImage = new MagickImage(imagePath);

        var core = TestHelpers.NewCore(rom, Path.GetFileNameWithoutExtension(romPath), render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) =>
        {
            //check if we can return early
            if (FramesDrawn % 10 == 0)
            {
                var settings = new MagickReadSettings
                {
                    Width = 160,
                    Height = 144,
                    Format = MagickFormat.Gray
                };
                using var outputImage = new MagickImage(render.Image, settings);

                if (ImageComparer.AreImagesEqual(expectedImage, outputImage))
                {
                    outputImage.Write(outputFile, MagickFormat.Bmp);
                    Assert.Pass($"Images match at frame {FramesDrawn}. Wrote debug image to {outputFile}");
                }
            }
            FramesDrawn++;
        };

        while (FramesDrawn != frameToCheck)
            core.Step();
        core.Dispose();


        var settings = new MagickReadSettings
        {
            Width = 160,
            Height = 144,
            Format = MagickFormat.Gray
        };
        using var outputImage = new MagickImage(render.Image, settings);

        if (FramesDrawn == frameToCheck)
        {
            outputImage.Write(outputFile, MagickFormat.Bmp);
            Assert.Fail($"Images did not match after {frameToCheck} frames. Wrote debug image to {outputFile}");
        }
    }



    [Test]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m2_win_en_toggle.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_bgp_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_bgp_change_sprites.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_bg_en_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_bg_en_change2.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_bg_map_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_bg_map_change2.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_obj_en_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_obj_en_change_variant.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_obj_size_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_obj_size_change_scx.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_tile_sel_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_tile_sel_change2.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_tile_sel_win_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_tile_sel_win_change2.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_win_en_change_multiple.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_win_en_change_multiple_wx.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_win_map_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_lcdc_win_map_change2.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_obp0_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_scx_high_5_bits.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_scx_high_5_bits_change2.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_scx_low_3_bits.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_scy_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_scy_change2.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_window_timing.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_window_timing_wx_0.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_wx_4_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_wx_4_change_sprites.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_wx_5_change.gb")]
    [TestCase(@"rom\mealybug-tearoom-tests\ppu\m3_wx_6_change.gb")]
    public async Task TestTeaRoom(string romPath)
    {
        var imageName = Path.GetFileNameWithoutExtension(romPath) + "_dmg_blob.png";
        var imagePath = Path.Combine(Path.GetDirectoryName(romPath)!, imageName);
        await TestMatchesOnBreakCondition(romPath, imagePath, Path.ChangeExtension(romPath, ".bmp"));
    }

    public async Task TestMatchesOnBreakCondition(string romPath, string imagePath, string outputFile)
    {
        var render = new TestRenderDevice();

        var rom = File.ReadAllBytes(romPath);
        var expectedImage = new MagickImage(imagePath);
        MapMealybugImageToExpectedPalette(expectedImage);

        var core = TestHelpers.NewCore(rom, Path.GetFileNameWithoutExtension(romPath), render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) =>
        {
            FramesDrawn++;
        };

        bool breakPointHit = false;

        core.CPU.BreakInstructions.Add(Opcode.LD_B_B); // Break on a no-op instruction, Mealybug tests are designed to hit this instruction at the end of the test.
        core.CPU.BreakPointHit += (sender, e) =>
        {
            Console.WriteLine($"Hit breakpoint at PC: {core.CPU.PC:X4}, FramesDrawn: {FramesDrawn}");
            breakPointHit = true;
        };

        while (!breakPointHit)
            core.Step();
        core.Dispose();

        var settings = new MagickReadSettings
        {
            Width = 160,
            Height = 144,
            Format = MagickFormat.Gray
        };
        using var outputImage = new MagickImage(render.Image, settings);


        outputImage.Write(outputFile, MagickFormat.Bmp);
        Assert.That(ImageComparer.AreImagesEqual(expectedImage, outputImage), $"Images did not match after breakpoint. Wrote debug image to {outputFile}");

    }

    public static void MapMealybugImageToExpectedPalette(MagickImage image)
    {
        // Replace exact reference shades without assuming an RGBA pixel layout.
        image.ColorFuzz = new Percentage(0);
        byte lightGray = emulator.graphics.Renderer.ShadeToGray(emulator.graphics.Shade.LightGray);
        byte darkGray = emulator.graphics.Renderer.ShadeToGray(emulator.graphics.Shade.DarkGray);
        image.Opaque(new MagickColor(170, 170, 170), new MagickColor(lightGray, lightGray, lightGray));
        image.Opaque(new MagickColor(85, 85, 85), new MagickColor(darkGray, darkGray, darkGray));
    }

}