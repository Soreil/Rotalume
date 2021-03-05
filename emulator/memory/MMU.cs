using System;

namespace emulator
{
    public class MMU
    {
        private readonly Func<bool> _bootROMActive;
        private readonly MBC Card;
        private readonly VRAM VRAM;
        private readonly WRAM WRAM;
        private readonly OAM OAM;
        private readonly ControlRegister IORegisters;
        private readonly HRAM HRAM;
        private readonly ControlRegister InterruptEnable;
        private readonly UnusableMEM UnusableMEM;

        public byte this[int at]
        {
            get
            {
                if (_bootROMActive() && at < 0x100) //Bootrom is read only so we don't need a corresponding function in set
                {
                    return bootROM![at];
                }

#pragma warning disable CS8509 // Exhaustive
                return at switch
#pragma warning restore CS8509
                {
                    >= 0 and < 0x4000 => Card[at],//bank0
                    >= 0x4000 and < 0x8000 => Card[at],//bank1
                    >= 0x8000 and < 0xa000 => VRAM.Locked ? (byte)0xff : VRAM[at],
                    >= 0xa000 and < 0xc000 => Card[at],//ext_ram
                    >= 0xc000 and < 0xe000 => WRAM[at],//wram
                    >= 0xe000 and < 0xFE00 => WRAM[at],//wram mirror
                    >= 0xfe00 and < 0xfea0 => OAM.Locked ? (byte)0xff : OAM[at],
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
                    {
                        VRAM[at] = value;
                    }

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
                    {
                        OAM[at] = value;
                    }

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
        private readonly byte[]? bootROM;

        private readonly Func<byte> ReadInput;

        private readonly byte[] BitConverterBuffer = new byte[2];
        private ushort ReadInputWide()
        {
            BitConverterBuffer[0] = ReadInput();
            BitConverterBuffer[1] = ReadInput();
            return BitConverter.ToUInt16(BitConverterBuffer);
        }
        public MMU(Func<byte> readInput,
            byte[]? boot,
            Func<bool> bootROMActive,
            MBC card,
            VRAM vram,
            OAM oam,
            ControlRegister ioRegisters,
            ControlRegister interruptEnable)
        {
            ReadInput = readInput;

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

        internal ushort FetchD16() => ReadInputWide();

        internal byte FetchD8() => ReadInput();

        internal ushort FetchA16() => ReadInputWide();

        internal byte FetchA8() => this[0xFF00 + ReadInput()];

        internal sbyte FetchR8() => (sbyte)ReadInput();

        public byte Read(ushort at) => this[at];

        //Same problem as readinputwide
        private readonly byte[] BitConverterBuffer2 = new byte[2];
        public ushort ReadWide(ushort at)
        {
            BitConverterBuffer2[0] = this[at];
            BitConverterBuffer2[1] = this[at + 1];
            return BitConverter.ToUInt16(BitConverterBuffer2);
        }

        public void Write(ushort at, byte arg) => this[at] = arg;

        public void Write(DMGInteger at, byte arg)
        {
            if (at == DMGInteger.a8)
            {
                Write((ushort)(0xff00 + FetchD8()), arg);
            }
            else if (at == DMGInteger.a16)
            {
                Write(FetchD16(), arg);
            }
            else
            {
                throw new Exception("Not an adress");
            }
        }

        public void Write(ushort at, ushort arg)
        {
            //var bytes = BitConverter.GetBytes(arg);
            //for (int i = 0; i < bytes.Length; i++)
            //    this[at + i] = bytes[i];
            this[at] = (byte)(arg & 0xff);
            this[at + 1] = (byte)((arg & 0xff00) >> 8);
        }
    }
}