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
