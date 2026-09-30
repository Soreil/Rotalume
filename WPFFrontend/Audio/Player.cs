using emulator.sound;

using NAudio.Wave;

namespace WPFFrontend.Audio;

public class Player : IDisposable
{
    private readonly Provider provider;
    private readonly WasapiPlayer waveOut;
    private bool disposedValue;

    public Player(Samples samples)
    {
        waveOut = new WasapiPlayerBuilder().WithSharedMode().WithLatency(100).Build();

        provider = new Provider(44100, samples);

        waveOut.Init(provider);
    }

    public bool Playing { get; private set; }

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
            }

            disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}