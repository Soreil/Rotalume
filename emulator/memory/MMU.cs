using System;

namespace emulator
{
    public class MMU
    {
        private readonly Func<bool> _bootROMActive;

        readonly MBC Card;
        readonly VRAM VRAM;
        readonly WRAM WRAM;
        readonly OAM OAM;
        readonly ControlRegister IORegisters;
        readonly HRAM HRAM;
        readonly ControlRegister InterruptEnable;
        readonly UnusableMEM UnusableMEM;

        public byte this[int at]
        {
            get
            {
                if (_bootROMActive() && at < 0x100) //Bootrom is read only so we don't need a corresponding function in set
                    return bootROM[at];

#pragma warning disable CS8509 // Exhaustive
                return at switch
#pragma warning restore CS8509
                {
                    >= 0 and < 0x4000 => Card[at],//bank0
                    >= 0x4000 and < 0x8000 => Card[at],//bank1
                    >= 0x8000 and < 0xa000 => VRAM.Locked ? 0xff : VRAM[at],
                    >= 0xa000 and < 0xc000 => Card[at],//ext_ram
                    >= 0xc000 and < 0xe000 => WRAM[at],//wram
                    >= 0xe000 and < 0xFE00 => WRAM[at],//wram mirror
                    >= 0xfe00 and < 0xfea0 => OAM.Locked ? 0xff : OAM[at],
                    >= 0xfea0 and < 0xff00 => UnusableMEM[at],//This should be illegal?
                    >= 0xff00 and < 0xff80 => IORegisters[at],
                    >= 0xff80 and < 0xffff => HRAM[at],
                    0xffff => InterruptEnable[at],
                };
            }

            set
            {
                switch (at)
                {
                    case >= 0 and < 0x4000:
                        Card[at] = value;//bank0
                        break;
                    case >= 0x4000 and < 0x8000:
                        Card[at] = value;//bank1
                        break;
                    case >= 0x8000 and < 0xa000:
                        if (!VRAM.Locked)
                            VRAM[at] = value;
                        break;
                    case >= 0xa000 and < 0xc000:
                        Card[at] = value;//ext_ram
                        break;
                    case >= 0xc000 and < 0xe000:
                        WRAM[at] = value;//wram
                        break;
                    case >= 0xe000 and < 0xFE00:
                        WRAM[at] = value;//wram mirror
                        break;
                    case >= 0xfe00 and < 0xfea0:
                        if (!OAM.Locked)
                            OAM[at] = value;
                        break;
                    case >= 0xfea0 and < 0xff00:
                        UnusableMEM[at] = value; //This should be illegal?
                        break;
                    case >= 0xff00 and < 0xff80:
                        IORegisters[at] = value;
                        break;
                    case >= 0xff80 and < 0xffff:
                        HRAM[at] = value;
                        break;
                    case 0xffff:
                        InterruptEnable[at] = value;
                        break;
                }
            }
        }
        private readonly byte[] bootROM;

        private readonly Func<byte> ReadInput;
        private readonly Func<ushort> ReadInputWide;

        public MMU(Func<byte> readInput)
        {
            ReadInput = readInput;
            ReadInputWide = () => BitConverter.ToUInt16(new byte[] { ReadInput(), ReadInput() });

            _bootROMActive = () => false;
        }
        public MMU(Func<byte> readInput,
            byte[] boot,
            Func<bool> bootROMActive,
            MBC card,
            VRAM vram,
            OAM oam,
            ControlRegister ioRegisters,
            ControlRegister interruptEnable)
        {
            ReadInput = readInput;
            ReadInputWide = () => BitConverter.ToUInt16(new byte[] { ReadInput(), ReadInput() });

            _bootROMActive = bootROMActive;
            bootROM = boot; //Bootrom should be 256 bytes

            var wram = new WRAM();
            var hram = new HRAM();

            Card = card;
            VRAM = vram;
            WRAM = wram;
            OAM = oam;
            IORegisters = ioRegisters;
            HRAM = hram;
            InterruptEnable = interruptEnable;
            UnusableMEM = new UnusableMEM();
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