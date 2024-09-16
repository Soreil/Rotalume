using emulator.graphics;
using emulator.memory;

namespace emulator.glue;

public class DMAControl(OAM oam, MMU mmu, DMARegister dMARegister)
{
    public OAM OAM { get; } = oam;
    public MMU MMU { get; } = mmu;
    public DMARegister Register { get; } = dMARegister;

    public void DMA()
    {
        if (Register.TicksLeft > 0)
        {
            ushort address = (ushort)(Register.BaseAddr + (DMARegister.DMADuration - Register.TicksLeft));
            byte value = Register.BaseAddr < 0xa000 ? MMU[address] : MMU.ExternalBusRAM(address);
            OAM[OAM.Start + (DMARegister.DMADuration - Register.TicksLeft)] = value;
            Register.TicksLeft--;
        }
    }
}
