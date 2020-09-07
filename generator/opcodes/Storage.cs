using System;
using System.Collections.Generic;

namespace generator
{
    public record Storage
    {
        private readonly byte[] _mem;
        private readonly bool bootROMActive;

        public byte this[int at]
        {
            get
            {
                if (bootROMActive)
                {
                    if (at < 0x100)
                        return bootROM[at];
                    else return _mem[at];
                }
                else return _mem[at];
            }

            set
            {
                if (bootROMActive)
                {
                    if (at < 0x100)
                        bootROM[at] = value;
                    else _mem[at] = value;
                }
                else _mem[at] = value;
            }
        }
        private readonly List<byte> bootROM;

        private readonly Func<byte> ReadInput;
        private readonly Func<ushort> ReadInputWide;

        public Storage(Func<byte> readInput, List<byte> boot, List<byte> game)
        {
            ReadInput = readInput;
            ReadInputWide = () => BitConverter.ToUInt16(new byte[] { ReadInput(), ReadInput() });

            _mem = new byte[0x10000];
            game.CopyTo(_mem);

            bootROMActive = boot.Count != 0;
            bootROM = boot;

        }
        internal object Fetch(DMGInteger arg)
        {
            return arg switch
            {
                DMGInteger.d16 => ReadInputWide(),
                DMGInteger.d8 => ReadInput(),
                DMGInteger.a16 => this[ReadInputWide()],
                DMGInteger.a8 => this[0xFF00 + ReadInput()],
                DMGInteger.r8 => (sbyte)ReadInput(),
                _ => throw new Exception("Expected a valid DMGInteger"),
            };
        }
        public byte Read(ushort at) => this[at];

        public ushort ReadWide(ushort at) => BitConverter.ToUInt16(new byte[] { this[at], this[at + 1] });

        public void Write(ushort at, byte arg) => this[at] = arg;


        public void Write(DMGInteger at, byte arg)
        {
            if (at == DMGInteger.a8)
                Write((ushort)(0xff00 + (byte)Fetch(DMGInteger.d8)), arg);
            else if (at == DMGInteger.a16)
                Write((ushort)Fetch(DMGInteger.d16), arg);
            else
                throw new Exception("Not an adress");
        }

        public void Write(ushort at, ushort arg)
        {
            var bytes = BitConverter.GetBytes(arg);
            for (int i = 0; i < bytes.Length; i++)
                this[at + i] = bytes[i];
        }
    }
}