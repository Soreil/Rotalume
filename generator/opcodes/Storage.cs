using System;
using System.Collections.Generic;

namespace generator
{
    public delegate void ControlRegister();

    public record Storage
    {
        public ControlRegister[] Handlers = new ControlRegister[0x80];
        private readonly byte[] _mem;

        private readonly Func<bool> _bootROMActive;
        private bool bootROMActive
        {
            get => _bootROMActive();
        }

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

                //TODO: turn the bootrom active flag in to a handler.
                //We have to pull the flag out of the storage system and remove
                //the handler after we have have triggered it once with a 1 at
                //0xff50. Removing the handler from inside the handler might not
                //work so we might need a disable flag in the handler instead.
                if (at >= 0xff00 && at < 0xff80) Handlers[at & 0xFF]?.Invoke();
            }
        }
        private readonly List<byte> bootROM;

        private readonly Func<byte> ReadInput;
        private readonly Func<ushort> ReadInputWide;

        public Storage(Func<byte> readInput, List<byte> boot, List<byte> game, Func<bool> bootROMActive)
        {
            ReadInput = readInput;
            ReadInputWide = () => BitConverter.ToUInt16(new byte[] { ReadInput(), ReadInput() });

            _mem = new byte[0x10000];
            game.CopyTo(_mem);

            _bootROMActive = bootROMActive;
            bootROM = boot;
        }
        internal object Fetch(DMGInteger arg)
        {
            return arg switch
            {
                DMGInteger.d16 => ReadInputWide(),
                DMGInteger.d8 => ReadInput(),
                DMGInteger.a16 => ReadInputWide(),
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