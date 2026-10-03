extern alias PreviousEmulator;
using emulator.graphics;

namespace BenchmarkSuite1;

public sealed class CountingFrameSink : IFrameSink, PreviousEmulator::emulator.graphics.IFrameSink
{
    public int Frames;
    public bool Paused { get; private set; }

    public event EventHandler? FramePushed;
    public void Draw()
    {
        Frames++;
        FramePushed?.Invoke(this, EventArgs.Empty);
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
    }

    public void Pause() => Paused = true;
    public void Resume() => Paused = false;
}
