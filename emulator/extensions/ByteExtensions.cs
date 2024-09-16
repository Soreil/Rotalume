namespace emulator;

// Half Carry extensions are only used in the implementation of opcodes.
public static class ByteExtensions
{
    public static void SetBit(this ref byte input, int at, bool value)
        => input = (byte)(value ? (input | (1 << at)) : (input & ~(1 << at)));

    public static void SetBit(this ref byte input, int at)
        => input |= (byte)(1 << at);

    public static void ClearBit(this ref byte input, int at)
        => input &= (byte)~(1 << at);

    public static bool GetBit(this byte input, int at)
        => (input & (1 << at)) != 0;

    public static bool IsHalfCarryAdd(this byte input, byte arg)
        => (((input & 0xf) + (arg & 0xf)) & 0x10) == 0x10;

    public static bool IsHalfCarrySub(this byte input, byte arg)
        => (input & 0xf) - (arg & 0xf) < 0;
}
