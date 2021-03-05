namespace emulator
{
    public static class ByteExtensions
    {
        public static byte SetBit(this byte input, int at, bool value) => (byte)(input & ~(1 << at) | ((value ? 1 : 0) << at));

        public static byte SetBit(this byte input, int at) => (byte)(input & ~(1 << at) | (1 << at));

        public static byte ClearBit(this byte input, int at) => (byte)(input & ~(1 << at) | (0 << at));

        public static bool GetBit(this byte input, int at) => ((input >> at) & 1) == 1;

        public static bool IsHalfCarryAdd(this byte input, byte arg) => (((input & 0xf) + (arg & 0xf)) & 0x10) == 0x10;

        public static bool IsHalfCarrySub(this byte input, byte arg) => (input & 0xf) - (arg & 0xf) < 0;
    }
}