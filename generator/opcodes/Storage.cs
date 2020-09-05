using System;

namespace generator
{
    public record Storage
    {
        readonly byte[] mem = new byte[0x10000];
        private readonly Func<byte> ReadInput;
        private readonly Func<ushort> ReadInputWide;

        public Storage(Func<byte> readInput)
        {
            ReadInput = readInput;
            ReadInputWide = () => BitConverter.ToUInt16(new byte[] { ReadInput(), ReadInput() });
        }
        internal object Fetch(DMGInteger arg)
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
        public byte Read(ushort at) => mem[at];

        public ushort ReadWide(ushort at)
            => BitConverter.ToUInt16(new byte[] { mem[at], mem[at + 1] });

        public void Write(ushort at, byte arg)
        {
            mem[at] = arg;
        }

        public void Write(DMGInteger at, byte arg)
        {
            if (at == DMGInteger.a8)
                Write((ushort)(0xff00 + (byte)Fetch(DMGInteger.d8)), arg);
            else if (at == DMGInteger.a16)
                Write((ushort)Fetch(DMGInteger.d8), arg);
            else
                throw new Exception("Not an adress");
        }

        public void Write(ushort at, ushort arg)
        {
            var bytes = BitConverter.GetBytes(arg);
            for (int i = 0; i < bytes.Length; i++)
                mem[at + i] = bytes[i];
        }
    }
}