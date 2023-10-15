using emulator.graphics;
using emulator.memory;

namespace emulator.glue;

public class DMARegister
{
    public const int DMADuration = 160;

    public int TicksLeft;
    public ushort BaseAddr;
    public byte Register
    {
        get => (byte)(BaseAddr >> 8);

        set
        {
            TicksLeft = DMADuration;
            BaseAddr = (ushort)(value << 8);
        }
    }
}

public class DMAControl(OAM OAM, MMU MMU, DMARegister dMARegister)
{
    public OAM OAM { get; } = OAM;
    public MMU MMU { get; } = MMU;

    public DMARegister Register { get; } = dMARegister;
    public void DMA()
    {
        if (Register.TicksLeft != 0)
        {
            //DMA values greater than or equal to A000 always go to the external RAM
            var r = Register.BaseAddr < 0xa000
            ? MMU[(ushort)(Register.BaseAddr + (DMARegister.DMADuration - Register.TicksLeft))]
            : MMU.ExternalBusRAM((ushort)(Register.BaseAddr + (DMARegister.DMADuration - Register.TicksLeft)));

            OAM[OAM.Start + (DMARegister.DMADuration - Register.TicksLeft)] = r;
            Register.TicksLeft--;
        }
    }
}
