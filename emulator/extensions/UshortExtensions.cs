namespace emulator
{
    public static class UshortExtensions
    {
        //public static byte SetBit(this byte input, int at, bool value) =>
        //    (byte)(input & ~(1 << at) | ((value ? 1 : 0) << at));
        //public static byte SetBit(this byte input, int at) =>
        //    (byte)(input & ~(1 << at) | (1 << at));
        //public static byte ClearBit(this byte input, int at) =>
        //    (byte)(input & ~(1 << at) | (0 << at));
        //public static bool GetBit(this byte input, int at) =>
        //    ((input >> at) & 1) == 1;
        public static bool IsHalfCarryAdd(this ushort input, ushort arg) =>
            (((input & 0xfff) + (arg & 0xfff)) & 0x1000) == 0x1000;
        public static bool IsHalfCarrySub(this ushort input, ushort arg) =>
            (((input & 0xfff) - (arg & 0xfff)) & 0x1000) == 0x1000;
    }
}