using System;
using System.Collections.Generic;
using System.Linq;

namespace emulator
{
    public class MMU
    {
        private readonly byte[] _mem;

        private readonly Func<bool> _bootROMActive;
        private bool BootROMActive
        {
            get => _bootROMActive();
        }

        public abstract record Range(int Begin, int End, Func<int, bool> Exists);
        public record GetRange(int Begin, int End, Func<int, bool> Exists, Func<int, byte> At) : Range(Begin, End, Exists);
        public record SetRange(int Begin, int End, Func<int, bool> Exists, Action<int, byte> At) : Range(Begin, End, Exists);

        private List<GetRange> getRanges;
        private List<SetRange> setRanges;

        public byte this[int at]
        {
            get
            {
                //if (at == 0xff00) System.Diagnostics.Debugger.Break();

                var possible = getRanges.FirstOrDefault((x) => x.Begin <= at && x.End > at && x.Exists != null && x.Exists(at));

                if (BootROMActive && at < 0x100) //Bootrom is read only so we don't need a corresponding function in set
                    return bootROM[at];

                if (possible != null)
                {
                    return possible.At(at);
                }
                else return _mem[at];
            }

            set
            {
                var possible = setRanges.FirstOrDefault((x) => x.Begin <= at && x.End > at && x.Exists != null && x.Exists(at));

                if (possible != null)
                {
                    possible.At(at, value);
                }
                else _mem[at] = value;
            }
        }
        private readonly List<byte> bootROM;

        private readonly Func<byte> ReadInput;
        private readonly Func<ushort> ReadInputWide;

        public MMU(Func<byte> readInput)
        {
            ReadInput = readInput;
            ReadInputWide = () => BitConverter.ToUInt16(new byte[] { ReadInput(), ReadInput() });

            getRanges = new();
            setRanges = new();

            _mem = new byte[0x10000];
            for (int i = 0; i < 0x10000; i++) _mem[i] = 0xff; //Initialize to zero
            _bootROMActive = () => false;
        }
        public MMU(Func<byte> readInput,
            List<byte> boot,
            List<byte> game,
            Func<bool> bootROMActive,
            List<GetRange> _getRanges,
            List<SetRange> _setRanges)
        {
            ReadInput = readInput;
            ReadInputWide = () => BitConverter.ToUInt16(new byte[] { ReadInput(), ReadInput() });

            getRanges = _getRanges;
            setRanges = _setRanges;

            _mem = new byte[0x10000];
            for (int i = 0; i < 0x10000; i++) _mem[i] = 0xff; //Initialize to zero
            game.CopyTo(_mem); //Game should be at most 0x8000 in size

            _bootROMActive = bootROMActive;
            bootROM = boot; //Bootrom should be 256 bytes
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