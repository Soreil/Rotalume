using emulator.sound;

using NAudio.Wave;

namespace WPFFrontend.Audio;
internal class Provider : WaveProvider16
{
    public Provider(int SampleRate, Samples samples) : base(SampleRate, 2)
    {
        this.SampleRate = SampleRate;
        Samples = samples;
    }

    public int SampleRate { get; }
    public Samples Samples { get; }

    public override int Read(short[] buffer, int offset, int sampleCount) => Samples.GetSamples(buffer, offset, sampleCount, SampleRate);
}

public class Player : IDisposable
{
    private readonly Provider provider;
    private readonly WasapiOut waveOut;
    private bool disposedValue;

    public Player(Samples samples)
    {
        waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 100);

        provider = new Provider(44100, samples);

        waveOut.Init(provider);
    }

    public bool Playing { get; internal set; }

    public void Play()
    {
        Playing = true;
        waveOut.Play();
    }

    public void Stop()
    {
        Playing = false;
        waveOut.Stop();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                waveOut.Dispose();
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Player()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}