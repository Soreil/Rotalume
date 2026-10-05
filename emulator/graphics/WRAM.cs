namespace emulator.graphics;

public class WRAM
{
    private readonly byte[] mem;

    public const int Start = 0xC000;
    public const int MirrorStart = 0xE000;
    public const int Size = 0x2000;
    public const int MirrorEnd = 0xFE00;
    public WRAM()
    {
        mem = new byte[Size];
        for (int i = 0; i < Size; i++)
        {
            mem[i] = 0xff;
        }
    }
    public byte this[int n]
    {
        get => mem[n & 0x1fff];
        set => mem[n & 0x1fff] = value;
    }

    internal WRAMState SerializeState()
    {
        return new WRAMState
        {
            Mem = mem,
        };
    }
}

public class WRAMState
{
    public required byte[] Mem { get; init; }
}