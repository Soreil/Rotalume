using emulator.glue;
using emulator.opcodes;

using System.Runtime.InteropServices;

namespace emulator.sound;

public class Samples(MasterClock masterClock, APU apu)
{
    public MasterClock MasterClock { get; } = masterClock;

    public static int SampleRate => SamplesPerSecond;

    public List<short> APUBuffer = [];

    public const int SamplePeriod = 64;
    private const int SamplesPerSecond = CPUTimingConstants.Frequency / SamplePeriod;

    private double sampleRatePerformanceScaler = 1;

    public void Sample()
    {
        if (MasterClock.Now() % SamplePeriod == 0)
        {
            (var left, var right) = apu.Sample();
            APUBuffer.Add(left);
            APUBuffer.Add(right);
        }
    }

    //public int GetSamples(short[] buffer, int offset, int sampleCount, int sampleRate)
    public int GetSamples(Span<short> cardBuffer, int sampleRate)
    {
        //For now we will just give it back a bunch of zeroes so it doesn't die on us
        if (APUBuffer.Count == 0)
        {
            for (int i = 0; i < cardBuffer.Length; i++)
            {
                cardBuffer[i] = 0;
            }
            return cardBuffer.Length;
        }

        //We are starving
        if (APUBuffer.Count < cardBuffer.Length)
        {
            sampleRatePerformanceScaler *= 0.9999;
        }
        //We have way too many samples
        else if (APUBuffer.Count > cardBuffer.Length * 3)
        {
            sampleRatePerformanceScaler *= 1.0001;
        }
        else
        {
            var sampleRateOffset = sampleRatePerformanceScaler - 1.0;
            sampleRatePerformanceScaler = 1.0 + (sampleRateOffset * 0.9);
        }


        //There is likely a missmatch, for example if we are sampling at 65k in the emulator
        //but 44100 is expected as output we will have about 1.48x times as much samples as
        //need to be put in to the output buffer
        var SampleRatio = SamplesPerSecond / (double)sampleRate * sampleRatePerformanceScaler;
        //SamplesNeeded is how many samples we are actually being asked to deliver
        var SamplesNeeded = cardBuffer.Length * SampleRatio;

        var samples = CollectionsMarshal.AsSpan(APUBuffer);

        var samplesWeWillConsume = Math.Min(SamplesNeeded, samples.Length);

        var outputSampleCount = (int)(samplesWeWillConsume / SampleRatio);

        var got = Map(samples, outputSampleCount, SampleRatio);

        got.CopyTo(cardBuffer);
        
        APUBuffer.RemoveRange(0, (int)samplesWeWillConsume);

        return got.Length;
    }

    private static Span<short> Map(ReadOnlySpan<short> buffer, int outputSampleCount, double sampleRatio)
    {
        var output = new short[outputSampleCount];

        for (int i = 0; i < output.Length; i++)
        {
            //We are not doing any actual resampling for now, just getting the nearest matching number
            output[i] = buffer[(int)(i * sampleRatio)];
        }

        return output;
    }
}