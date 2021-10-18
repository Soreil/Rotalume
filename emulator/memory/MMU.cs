namespace emulator;

public class MMU
{
    private readonly MBC Card;
    private readonly VRAM VRAM;
    private readonly WRAM WRAM;
    private readonly OAM OAM;
    private readonly (Action<byte> Write, Func<byte> Read)[] IORegisters;
    private readonly HRAM HRAM;
    private readonly (Action<byte> Write, Func<byte> Read) InterruptEnable;
    private readonly UnusableMEM UnusableMEM;
    private readonly ProgramCounter PC;
    public byte this[ushort at]
    {
        get => BootROMActive && at < 0x100
                ? bootROM![at]
                : at switch
                {
                    >= 0 and < 0x4000 => Card[at],//bank0
                        >= 0x4000 and < 0x8000 => Card[at],//bank1
                        >= 0x8000 and < 0xa000 => VRAM.Locked ? (byte)0xff : VRAM[at],
                    >= 0xa000 and < 0xc000 => Card[at],//ext_ram
                        >= 0xc000 and < 0xe000 => WRAM[at],//wram
                        >= 0xe000 and < 0xFE00 => WRAM[at],//wram mirror
                        >= 0xfe00 and < 0xfea0 => OAM.Locked ? (byte)0xff : OAM[at],
                    >= 0xfea0 and < 0xff00 => UnusableMEM[at],//This should be illegal?
                        >= 0xff00 and < 0xff80 => IORegisters[at - 0xff00].Read(),
                    >= 0xff80 and < 0xffff => HRAM[at],
                    0xffff => InterruptEnable.Read(),
                };

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
                IORegisters[at - 0xff00].Write(value);
                break;
                case >= 0xff80 and < 0xffff:
                HRAM[at] = value;
                break;
                case 0xffff:
                InterruptEnable.Write(value);
                break;
            }
        }
    }

    private bool BootROMActive;
    internal (Action<byte> Write, Func<byte> Read) HookUpMemory()
    {
        void BootROMFlagController(byte b)
        {
            if (b == 1)
            {
                BootROMActive = false;
            }
        }
        return
            (
        BootROMFlagController,
         () => 0xff
         );
    }
    private readonly byte[]? bootROM;

    public byte ReadInput() => this[PC.Value++];
    private ushort ReadInputWide()
    {
        Span<byte> buf = stackalloc byte[2];
        buf[0] = ReadInput();
        buf[1] = ReadInput();
        return BitConverter.ToUInt16(buf);
    }
    public MMU(
        byte[]? boot,
        MBC card,
        VRAM vram,
        OAM oam,
        (Action<byte> Write, Func<byte> Read)[] ioRegisters,
        (Action<byte> Write, Func<byte> Read) interruptEnable,
        ProgramCounter ProgramCounter)
    {

        bootROM = boot; //Bootrom should be 256 bytes
        BootROMActive = bootROM is not null;

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
        PC = ProgramCounter;
    }

    internal ushort FetchD16() => ReadInputWide();

    internal byte FetchD8() => ReadInput();

    internal ushort FetchA16() => ReadInputWide();

    internal sbyte FetchR8() => (sbyte)ReadInput();

    public byte Read(ushort at) => this[at];

    public ushort ReadWide(ushort at)
    {
        Span<byte> buf = stackalloc byte[2];
        buf[0] = this[at];
        buf[1] = this[(ushort)(at + 1)];
        return BitConverter.ToUInt16(buf);
    }

    public void Write(ushort at, byte arg) => this[at] = arg;

    public void Write(ushort at, ushort arg)
    {
        this[at] = (byte)arg;
        this[(ushort)(at + 1)] = (byte)(arg >> 8);
    }
}
