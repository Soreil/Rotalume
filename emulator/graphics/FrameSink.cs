using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace emulator.graphics;

public class FrameSink : IFrameSink
{
    private byte[] frameData;
    private byte[] lastFrame;

    private const long timePerFrame = (long)(TimeSpan.TicksPerSecond / (opcodes.CPUTimingConstants.Frequency / (double)GraphicConstants.TicksPerFrame));
    private readonly Stopwatch stopWatch = new();
    private readonly Func<bool> LimitFPS;
    private readonly ILogger<FrameSink> logger;

    public FrameSink(Func<bool> LimitFPS, ILogger<FrameSink> logger)
    {
        frameData = new byte[GraphicConstants.ScreenHeight * GraphicConstants.ScreenWidth];
        lastFrame = new byte[GraphicConstants.ScreenHeight * GraphicConstants.ScreenWidth];

        this.LimitFPS = LimitFPS;
        this.logger = logger;
        stopWatch.Start();
    }

    private int position;

    public bool Paused { get; private set; }

    protected virtual void OnFramePushed(EventArgs e) => FramePushed?.Invoke(this, e);

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
        position = 0;
        OnFramePushed(EventArgs.Empty);
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        var span = MemoryMarshal.CreateSpan(ref frameData[position], buffer.Length);
        buffer.CopyTo(span);
        position += buffer.Length;
    }

    public void Pause()
    {
        stopWatch.Stop();
        Paused = true;
    }
    public void Resume()
    {
        stopWatch.Restart();
        Paused = false;
    }
}
