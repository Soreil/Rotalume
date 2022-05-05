using System.Diagnostics;

namespace emulator;

public class FrameSink : IFrameSink
{
    private byte[] frameData;
    private byte[] lastFrame;

    private const long timePerFrame = (long)(TimeSpan.TicksPerSecond / (cpu.Constants.Frequency / (double)graphics.Constants.TicksPerFrame));
    private readonly Stopwatch stopWatch = new();
    private readonly Func<bool> LimitFPS;
    public FrameSink(Func<bool> LimitFPS)
    {
        frameData = new byte[graphics.Constants.ScreenHeight * graphics.Constants.ScreenWidth];
        lastFrame = new byte[graphics.Constants.ScreenHeight * graphics.Constants.ScreenWidth];

        this.LimitFPS = LimitFPS;

        stopWatch.Start();
    }

    private int Position { get; set; }

    protected virtual void OnFramePushed(EventArgs e)
    {
        if (FramePushed is not null)
            FramePushed(this, e);
    }

    public event EventHandler? FramePushed;

    public byte[] GetFrame()
    {
        var buffer = new byte[frameData.Length];
        lastFrame.CopyTo(buffer, 0);
        return buffer;
    }

    public void Draw()
    {
        if (LimitFPS())
        {
            while (stopWatch.ElapsedTicks < timePerFrame)
            {

            }
        }
        stopWatch.Restart();

        //Swap current buffer and last buffer
        (lastFrame, frameData) = (frameData, lastFrame);
        Position = 0;
        OnFramePushed(EventArgs.Empty);
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        buffer.CopyTo(new Span<byte>(frameData, Position, buffer.Length));
        Position += buffer.Length;
    }
}
