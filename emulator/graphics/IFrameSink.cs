
namespace emulator;

public interface IFrameSink
{
    event EventHandler? FramePushed;

    void Draw();
    void Write(ReadOnlySpan<byte> buffer);
}