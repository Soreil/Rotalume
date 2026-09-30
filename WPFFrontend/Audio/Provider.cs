using emulator.sound;

using NAudio.Wave;

namespace WPFFrontend.Audio;

internal class Provider(int sampleRate, Samples samples) : WaveProvider16(sampleRate, 2)
{
    public int SampleRate { get; } = sampleRate;
    public Samples Samples { get; } = samples;

    public override int Read(Span<short> buffer)
    {
        var sampled = Samples.GetSamples(buffer, SampleRate);
        return sampled;
    }

    //public override int Read(short[] buffer, int offset, int sampleCount)
    //    => Samples.GetSamples(buffer, offset, sampleCount, SampleRate);
}
