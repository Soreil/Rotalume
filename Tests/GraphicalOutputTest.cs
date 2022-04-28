
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

    [TestCase(@"..\..\..\..\Tests\rom\blargg\cpu_instrs\cpu_instrs.gb", @"..\..\..\..\Tests\rom\blargg\cpu_instrs\expected.png", "outputBlargCPUTest.bmp", 4000)]
    [TestCase(@"..\..\..\..\Tests\rom\blargg\instr_timing\instr_timing.gb", @"..\..\..\..\Tests\rom\blargg\instr_timing\expected.png", "outputINSTTiming.bmp", 100)]
    [TestCase(@"..\..\..\..\Tests\rom\blargg\mem_timing\mem_timing.gb", @"..\..\..\..\Tests\rom\blargg\mem_timing\expected.png", "outputMEMTiming.bmp", 100)]
    [TestCase(@"..\..\..\..\Tests\rom\blargg\mem_timing-2\mem_timing.gb", @"..\..\..\..\Tests\rom\blargg\mem_timing-2\expected.png", "outputMEMTiming2.bmp", 200)]
    [TestCase(@"..\..\..\..\Tests\rom\blargg\halt_bug\halt_bug.gb", @"..\..\..\..\Tests\rom\blargg\halt_bug\expected.png", "outputHaltBug.bmp", 300)]

    [TestCase(@"..\..\..\..\Tests\rom\dmg-acid2\dmg-acid2.gb", @"..\..\..\..\Tests\rom\dmg-acid2\expected.png", "outputDMG-ACID2.bmp", 10)]

    [TestCase(@"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\basic.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputBasicOAM.bmp", 10)]
    [TestCase(@"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\reg_read.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputRegReadOAM.bmp", 10)]
    [TestCase(@"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\sources-GS.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\oam_dma\expected.png", "outputSourcesGS.bmp", 10)]

    [TestCase(@"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\mem_oam.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\expected.png", "outputMEMOAM.bmp", 100)]
    //[TestCase(@"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\unused_hwio-GS.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\expected.png", "outputUnusedHWIO.bmp", 100)]
    [TestCase(@"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\reg_f.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\bits\expected_regf.png", "outputRegF.bmp", 100)]

    [TestCase(@"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\instr\daa.gb", @"..\..\..\..\Tests\rom\mooneye-test-suite\acceptance\instr\expected.png", "outputDAA.bmp", 100)]

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

        Assert.IsTrue(TestHelpers.AreEqual((Image<L8>)expectedImage, outputImage));
    }


    [TestCase(@"C:\Users\sjon\Documents\Pokemon - Gold Version (UE) [C][!].gbc", 40)]
    [TestCase(@"C:\Users\sjon\Documents\Pokemon Pinball (Europe) (En,Fr,De,Es,It) (SGB Enhanced).gbc", 20)]
    [TestCase(@"C:\Users\sjon\Documents\Kirby's Dream Land (USA, Europe).gb", 40)]
    [Category("RequiresBootROM")]
    public void SoundFromPokemonGoldIntro(string path, int duration)
    {
        var render = new TestRenderDevice();

        var data = File.ReadAllBytes(path);

        var fn = Path.GetFileName(path);

        var core = TestHelpers.NewCore(data, fn, render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) => FramesDrawn++;

        List<short> samples = new();
        var sampleRate = emulator.cpu.Constants.Frequency / 44100.0;
        var sampleCount = 0;

        while (sampleCount < 44100.0 * duration)
        {
            core.Step();

            if (core.masterclock > sampleRate * sampleCount)
            {
                (short left, short right) = core.Sample();
                samples.Add(left);
                samples.Add(right);
                sampleCount++;
            }
        }

        var span = new ReadOnlySpan<short>(samples.ToArray());
        var wav = new WAV.WAVFile<short>(span, 2, 44100, 16);

        using var file = File.Open(fn + ".wav", FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(file);
        wav.Write(writer, span);
    }

    [Test]
    [Category("RequiresBootROM")]
    public void BootromSound()
    {
        var render = new TestRenderDevice();

        var core = TestHelpers.NewBootCore(render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) => FramesDrawn++;

        List<short> samples = new();
        var sampleRate = emulator.cpu.Constants.Frequency / 44100.0;
        var sampleCount = 0;
        while (core.CPU.PC != 0x100)
        {
            core.Step();

            if (core.masterclock > sampleRate * sampleCount)
            {
                (short left, short right) = core.Sample();
                samples.Add(left);
                samples.Add(right);
                sampleCount++;
            }
        }

        var SampleCount = samples.Count;
        var SamplesWithSound = samples.Count(x => x != 0);

        Assert.NotZero(SamplesWithSound);

        var span = new ReadOnlySpan<short>(samples.ToArray());
        var wav = new WAV.WAVFile<short>(span, 2, 44100, 16);

        using var file = File.Open("bootromSound.wav", FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(file);
        wav.Write(writer, span);

    }
}
