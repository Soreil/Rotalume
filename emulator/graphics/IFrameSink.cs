
namespace emulator.graphics;

public interface IFrameSink
{
    event EventHandler? FramePushed;

    bool Paused { get; }
    void Pause();
    void Resume();
    void Draw();
    void Write(ReadOnlySpan<byte> buffer);
}