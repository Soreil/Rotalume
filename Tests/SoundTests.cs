
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

        List<short> samples = new();
        var sampleRate = emulator.cpu.Constants.Frequency / 44100.0;
        var sampleCount = 0;

        while (sampleCount < 44100.0 * duration)
        {
            core.Step();

            if (core.Time() > sampleRate * sampleCount)
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

    [TestCase(@"rom\game\Pokemon - Gold Version (UE) [C][!].gbc", 60)]
    [Category("RequiresBootROM")]
    public void PokemonGoldChannel1(string path, int duration)
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

            if (core.Time() > sampleRate * sampleCount)
            {
                (short left, short right) = core.SampleChannel1();
                samples.Add(left);
                samples.Add(right);
                sampleCount++;
            }
        }

        var span = new ReadOnlySpan<short>(samples.ToArray());
        var wav = new WAV.WAVFile<short>(span, 2, 44100, 16);

        using var file = File.Open(fn + " Channel 1" + ".wav", FileMode.Create, FileAccess.Write);
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

            if (core.Time() > sampleRate * sampleCount)
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
