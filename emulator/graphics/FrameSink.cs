using System.Diagnostics;
using System.Runtime.InteropServices;

namespace emulator;

public class FrameSink : IFrameSink
{
    private readonly byte[] frameData;

    private readonly Func<IntPtr> Lock;
    private readonly Action Unlock;

    private const long timePerFrame = (long)(TimeSpan.TicksPerSecond / (cpu.Constants.Frequency / (double)graphics.Constants.TicksPerFrame));
    private readonly Stopwatch stopWatch = new();
    private readonly Func<bool> LimitFPS;
    public FrameSink(Func<IntPtr> Lock, Action Unlock, Func<bool> LimitFPS)
    {
        frameData = new byte[graphics.Constants.ScreenHeight * graphics.Constants.ScreenWidth];

        this.Lock = Lock;
        this.Unlock = Unlock;
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

    public void Draw()
    {
        if (LimitFPS())
        {
            while (stopWatch.ElapsedTicks < timePerFrame)
            {

            }
        }
        stopWatch.Restart();

        var ptr = Lock();
        if (ptr != IntPtr.Zero)
        {
            unsafe
            {
                Marshal.Copy(frameData, 0, ptr, frameData.Length);
            }
        }
        Unlock();

        Position = 0;
        OnFramePushed(EventArgs.Empty);
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        buffer.CopyTo(new Span<byte>(frameData, Position, buffer.Length));
        Position += buffer.Length;
    }
}
