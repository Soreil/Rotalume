using emulator;

namespace Tests;

internal class TestRenderDevice : IFrameSink
{
    private readonly byte[] backingBuffer;
    private int index;

    public TestRenderDevice()
    {
        backingBuffer = new byte[144 * 160];
        Image = new byte[144 * 160];
    }

    public event EventHandler? FramePushed;

    public void Draw()
    {
        backingBuffer.CopyTo(Image,0);

        index = 0;
        FramePushed?.Invoke(this, EventArgs.Empty);
    }

    public byte[] Image { get; set; }
    public bool Paused { get; private set; }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        buffer.CopyTo(new Span<byte>(backingBuffer, index, buffer.Length));
        index += buffer.Length;
    }

    public void Pause() => Paused= true;
    public void Resume() => Paused= false;
}