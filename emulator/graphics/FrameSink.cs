using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace emulator;

public class FrameSink : IFrameSink
{
    private byte[] frameData;
    private byte[] lastFrame;

    private const long timePerFrame = (long)(TimeSpan.TicksPerSecond / (cpu.Constants.Frequency / (double)graphics.Constants.TicksPerFrame));
    private readonly Stopwatch stopWatch = new();
    private readonly Func<bool> LimitFPS;
    private readonly ILogger<FrameSink> logger;

    public FrameSink(Func<bool> LimitFPS, ILogger<FrameSink> logger)
    {
        frameData = new byte[graphics.Constants.ScreenHeight * graphics.Constants.ScreenWidth];
        lastFrame = new byte[graphics.Constants.ScreenHeight * graphics.Constants.ScreenWidth];

        this.LimitFPS = LimitFPS;
        this.logger = logger;
        stopWatch.Start();
    }

    private int Position { get; set; }

    private bool paused;
    public bool Paused => paused;

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
        var elapsed = stopWatch.ElapsedTicks;
        stopWatch.Restart();
        //In case we take over double the expected frame time something probably went wrong quite badly
        //TODO: generate a serialization of where time was spent to render the offending frame.
        if (elapsed > timePerFrame * 2)
        {
            logger.LogInformation($"frame duration in ms:{elapsed / 10000.0}");
        }

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

    public void Pause()
    {
        stopWatch.Stop();
        paused = true;
    }
    public void Resume()
    {
        stopWatch.Restart();
        paused = false;
    }
}
