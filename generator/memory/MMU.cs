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

        public abstract record Range(int Begin, int End);
        public record GetRange(int Begin, int End, Func<int, byte> At) : Range(Begin, End);
        public record SetRange(int Begin, int End, Action<int, byte> At) : Range(Begin, End);

        private List<GetRange> getRanges;
        private List<SetRange> setRanges;

        public byte this[int at]
        {
            get
            {

                if (BootROMActive && at < 0x100) //Bootrom is read only so we don't need a corresponding function in set
                    return bootROM[at];

                var possible = getRanges.First((x) => x.Begin <= at && x.End > at);
                return possible.At(at);
            }

            set
            {
                var possible = setRanges.First((x) => x.Begin <= at && x.End > at);
                possible.At(at, value);
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

            _mem = Array.Empty<byte>();
            getRanges.Add(new GetRange(0, 0x8000, (x) => game[x]));
            setRanges.Add(new SetRange(0, 0x8000, (x, v) => { }));

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