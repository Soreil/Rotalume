using NUnit.Framework;

namespace Tests;

internal class SoundTests
{
    [TestCase(@"rom\game\Pokemon - Gold Version (UE) [C][!].gbc", 60)]
    [TestCase(@"rom\game\Pokemon Pinball (Europe) (En,Fr,De,Es,It) (SGB Enhanced).gbc", 20)]
    [TestCase(@"rom\game\Kirby's Dream Land (USA, Europe).gb", 40)]
    [TestCase(@"C:\Users\sjon\source\repos\rgbds\sound\hello-world.gb", 20)]
    [Category("RequiresBootROM")]
    public void SoundSample(string path, int duration)
    {
        var render = new TestRenderDevice();

        var data = File.ReadAllBytes(path);

        var fn = Path.GetFileName(path);

        var core = TestHelpers.NewCore(data, fn, render);

        int FramesDrawn = 0;
        render.FramePushed += (sender, e) => FramesDrawn++;


        var samplesWanted = emulator.sound.Samples.SampleRate * duration;

        while (core.Samples.Buffer.Count < samplesWanted)
        {
            core.Step();
        }

        var span = new ReadOnlySpan<short>(core.Samples.Buffer.ToArray());
        var wav = new WAV.WAVFile<short>(span, 2, emulator.sound.Samples.SampleRate, 16);

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


        while (core.CPU.PC != 0x100)
        {
            core.Step();
        }


        var SampleCount = core.Samples.Buffer.Count;
        var SamplesWithSound = core.Samples.Buffer.Any(x => x != 0);

        Assert.IsTrue(SamplesWithSound);

        var span = new ReadOnlySpan<short>(core.Samples.Buffer.ToArray());
        var wav = new WAV.WAVFile<short>(span, 2, emulator.sound.Samples.SampleRate, 16);

        using var file = File.Open("bootromSound.wav", FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(file);
        wav.Write(writer, span);

    }

}
