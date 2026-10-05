using emulator.memory;
using emulator.opcodes;
using emulator.sound;

namespace emulator.glue;

public class SerializedGameboyState
{
    public required CPUState CPU { get; init; }
    public required MMUState Memory { get; init; }
    public required APUState APU { get; init; }
    public required long MasterClock { get; init; }
}