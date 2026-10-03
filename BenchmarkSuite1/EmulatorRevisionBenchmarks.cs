extern alias PreviousEmulator;

using BenchmarkDotNet.Attributes;

using emulator.glue;
using emulator.input;

using PreviousCore = PreviousEmulator::emulator.glue.Core;
using PreviousInputDevices = PreviousEmulator::emulator.input.InputDevices;
using PreviousKeypad = PreviousEmulator::emulator.input.Keypad;

namespace BenchmarkSuite1;

[SimpleJob(launchCount: 2, warmupCount: 4, iterationCount: 12, invocationCount: 1)]
public partial class EmulatorRevisionBenchmarks
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

    [Params(false, true)]
    public bool AudioEnabled { get; set; }

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
        current.Memory[0xff26] = AudioEnabled ? (byte)0x80 : (byte)0;
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
            if (((current.Memory[0xff26] & 0x80) != 0) != AudioEnabled)
                throw new InvalidOperationException("ROM changed APU enable during the measured interval.");
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
        previous.Memory[0xff26] = AudioEnabled ? (byte)0x80 : (byte)0;
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
            if (((previous.Memory[0xff26] & 0x80) != 0) != AudioEnabled)
                throw new InvalidOperationException("ROM changed APU enable during the measured interval.");
            if (GraphicsEnabled && previousSink.Frames < 119)
                throw new InvalidOperationException("The graphics workload did not produce the expected frames.");
        }
        finally
        {
            previous.Dispose();
        }
    }
}