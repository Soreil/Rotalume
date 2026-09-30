namespace emulator.graphics;

public class VRAM
{
    private readonly byte[] mem;

    public const int TileBlock0Start = 0x8000;
    public const int TileBlock1Start = 0x8800;
    public const int TileBlock2Start = 0x9000;
    public const int TileMap0Start = 0x9800;
    public const int TileMap1Start = 0x9C00;

    public const int Start = 0x8000;
    public const int Size = 0x2000;
    public bool Locked;

    public VRAM()
    {
        mem = new byte[Size];
        for (int i = 0; i < Size; i++)
        {
            mem[i] = 0x00;
        }
    }
    public byte this[int n]
    {
        get => mem[n - Start];
        set => mem[n - Start] = value;
    }
}
