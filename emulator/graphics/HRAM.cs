namespace emulator.graphics;

public class HRAM
{
    private readonly byte[] mem;

    public const int Start = 0xff80;
    public const int Size = 0x7f;
    public HRAM()
    {
        mem = new byte[Size];
        for (int i = 0; i < Size; i++)
        {
            mem[i] = 0xff;
        }
    }
    public byte this[int n]
    {
        get => mem[n - Start];
        set => mem[n - Start] = value;
    }
}

