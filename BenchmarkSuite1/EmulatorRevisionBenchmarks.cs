extern alias PreviousEmulator;

using BenchmarkDotNet.Attributes;

using emulator.glue;
using emulator.graphics;
using emulator.input;

using PreviousCore = PreviousEmulator::emulator.glue.Core;
using PreviousInputDevices = PreviousEmulator::emulator.input.InputDevices;
using PreviousKeypad = PreviousEmulator::emulator.input.Keypad;

namespace BenchmarkSuite1;

[SimpleJob(launchCount: 2, warmupCount: 4, iterationCount: 12, invocationCount: 1)]
public class EmulatorRevisionBenchmarks
{
    private const long FrameTicks = 70224;
    private const long WorkTicks = 120 * FrameTicks;
    private byte[] rom = null!;
    private Core current = null!;
    private CountingFrameSink currentSink = null!;
    private long currentEnd;
    private PreviousCore previous = null!;
    private CountingFrameSink previousSink = null!;
    private long previousEnd;
    [Params(false, true)]
    public bool GraphicsEnabled { get; set; }

    [GlobalSetup]
    public void LoadRom()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string path = Path.Combine(directory.FullName, "Tests", "rom", "dmg-acid2", "dmg-acid2.gb");
            if (File.Exists(path))
            {
                rom = File.ReadAllBytes(path);
                return;
            }
        }

        throw new FileNotFoundException("The comparison requires Tests/rom/dmg-acid2/dmg-acid2.gb.");
    }

    [IterationSetup(Target = nameof(Current))]
    public void SetupCurrent()
    {
        currentSink = new CountingFrameSink();
        current = new Core(rom, null, "revision-comparison", new Keypad(new InputDevices(new QuietController(), [])), currentSink);
        while (current.Time() < WorkTicks)
            current.Step();
        if (!GraphicsEnabled)
            current.Memory[0xff40] = 0;
        current.Samples.APUBuffer.Clear();
        currentSink.Frames = 0;
        currentEnd = current.Time() + WorkTicks;
    }

    [Benchmark]
    public int Current()
    {
        while (current.Time() < currentEnd)
            current.Step();
        return currentSink.Frames;
    }

    [IterationCleanup(Target = nameof(Current))]
    public void CleanupCurrent()
    {
        try
        {
            if (((current.Memory[0xff40] & 0x80) != 0) != GraphicsEnabled)
                throw new InvalidOperationException("ROM changed LCD enable during the measured interval.");
            if (GraphicsEnabled && currentSink.Frames < 119)
                throw new InvalidOperationException("The graphics workload did not produce the expected frames.");
        }
        finally
        {
            current.Dispose();
        }
    }

    [IterationSetup(Target = nameof(Previous))]
    public void SetupPrevious()
    {
        previousSink = new CountingFrameSink();
        previous = new PreviousCore(rom, null, "revision-comparison", new PreviousKeypad(new PreviousInputDevices(new QuietController(), [])), previousSink);
        while (previous.Time() < WorkTicks)
            previous.Step();
        if (!GraphicsEnabled)
            previous.Memory[0xff40] = 0;
        previous.Samples.APUBuffer.Clear();
        previousSink.Frames = 0;
        previousEnd = previous.Time() + WorkTicks;
    }

    [Benchmark(Baseline = true)]
    public int Previous()
    {
        while (previous.Time() < previousEnd)
            previous.Step();
        return previousSink.Frames;
    }

    [IterationCleanup(Target = nameof(Previous))]
    public void CleanupPrevious()
    {
        try
        {
            if (((previous.Memory[0xff40] & 0x80) != 0) != GraphicsEnabled)
                throw new InvalidOperationException("ROM changed LCD enable during the measured interval.");
            if (GraphicsEnabled && previousSink.Frames < 119)
                throw new InvalidOperationException("The graphics workload did not produce the expected frames.");
        }
        finally
        {
            previous.Dispose();
        }
    }

    private sealed class CountingFrameSink : IFrameSink, PreviousEmulator::emulator.graphics.IFrameSink
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

    private sealed class QuietController : IGameController, PreviousEmulator::emulator.input.IGameController
    {
        public bool IsBPressed => false;
        public bool IsSelectPressed => false;
        public bool IsAPressed => false;
        public bool IsDPadDownPressed => false;
        public bool IsDPadLeftPressed => false;
        public bool IsDPadRightPressed => false;
        public bool IsDPadUpPressed => false;
        public bool IsStartPressed => false;

        public void AddEventHandler(EventHandler<EventArgs> e)
        {
        }

        public void RemoveEventHandler(EventHandler<EventArgs> e)
        {
        }

        public void Vibrate(double leftMotor, double rightMotor)
        {
        }
    }
}