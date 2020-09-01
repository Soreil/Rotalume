using System;

namespace generator
{
    internal class Storage
    {
        readonly byte[] mem = new byte[0x10000];
        private readonly Func<byte> ReadInput;
        private readonly Func<ushort> ReadInputWide;
        internal object Read(DMGInteger arg)
        {
            return arg switch
            {
                DMGInteger.d16 => ReadInputWide(),
                DMGInteger.d8 => ReadInput(),
                DMGInteger.a16 => mem[ReadInputWide()],
                DMGInteger.a8 => mem[0xFF00 + ReadInput()],
                DMGInteger.r8 => (sbyte)ReadInput(),
                _ => throw new Exception("Expected a valid DMGInteger"),
            };
        }

        internal void Write(ushort at, byte arg)
        {
            mem[at] = arg;
        }
    }
}