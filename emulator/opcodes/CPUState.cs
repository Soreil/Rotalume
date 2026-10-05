using emulator.registers;

namespace emulator.opcodes;

public class CPUState
{
    public required ushort PC { get; init; }
    public required RegistersState Registers { get; init; }
    public required HaltState Halted { get; init; }
}
