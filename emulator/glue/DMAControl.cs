namespace emulator;

public class DMARegister
{
    public int TicksLeft;
    public ushort BaseAddr;
    public byte Register
    {
        get => (byte)(BaseAddr >> 8);

        set
        {
            TicksLeft = 160;
            BaseAddr = (ushort)(value << 8);
        }
    }
}

public class DMAControl
{
    public OAM OAM { get; }
    public MMU MMU { get; }

    public DMARegister Register { get; }
    public void DMA(object? o, EventArgs e)
    {
        if (Register.TicksLeft != 0)
        {
            //DMA values greater than or equal to A000 always go to the external RAM
            var r = Register.BaseAddr < 0xa000
            ? MMU[(ushort)(Register.BaseAddr + (160 - Register.TicksLeft))]
            : MMU.ExternalBusRAM((ushort)(Register.BaseAddr + (160 - Register.TicksLeft)));

            OAM[OAM.Start + (160 - Register.TicksLeft)] = r;
            Register.TicksLeft--;
        }
    }

    public DMAControl(OAM OAM, MMU MMU, DMARegister dMARegister)
    {
        this.OAM = OAM;
        this.MMU = MMU;
        Register = dMARegister;
    }


}
