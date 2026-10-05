using emulator.memory;
using emulator.opcodes;
using emulator.registers;

using Microsoft.Extensions.Logging;

namespace emulator.glue;

public class CPUFactory
{
    private Registers Registers { get; set; } = new Registers();
    private ushort? PC { get; set; }
    internal void SetStateWithoutBootrom()
    {
        PC = 0x100;
        Registers.AF = 0x0100;
        Registers.BC = 0xff13;
        Registers.DE = 0x00c1;
        Registers.HL = 0x8403;
        Registers.SP = 0xfffe;
    }

    public CPU CreateCPU(MMU memory, InterruptRegisters interruptRegisters, Cycler cycler, ILogger<CPU> logger)
    {
        return new CPU(memory, interruptRegisters, Registers, cycler, PC, logger);
    }
}
