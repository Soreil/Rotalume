namespace emulator;

public sealed class MMU
{
    private readonly MBC Card;
    private readonly VRAM VRAM;
    private readonly WRAM WRAM;
    private readonly OAM OAM;
    private readonly sound.APU APU;

    private readonly HRAM HRAM;
    private readonly UnusableMEM UnusableMEM;
    public byte this[ushort at]
    {
        get => BootRom.Active && at < 0x100
                ? BootRom[at]
                : at switch
                {
                    >= 0 and < 0x4000 => Card[at],//bank0
                    >= 0x4000 and < 0x8000 => Card[at],//bank1
                    >= 0x8000 and < 0xa000 => VRAM.Locked ? (byte)0xff : VRAM[at],
                    >= 0xa000 and < 0xc000 => Card[at],//ext_ram
                    >= 0xc000 and < 0xe000 => WRAM[at],//wram
                    >= 0xe000 and < 0xFE00 => WRAM[at],//wram mirror
                    >= 0xfe00 and < 0xfea0 => OAM.Locked ? (byte)0xff : OAM[at],
                    >= 0xfea0 and < 0xff00 => UnusableMEM[at],

                    0xff00 => Keypad.Register,
                    0xff01 => Serial.Data,
                    0xff02 => Serial.Control,

                    >= 0xff04 and < 0xff08 => Timers[at],

                    0xff0f => InterruptRegisters.InterruptFireRegister,

                    >= 0xff10 and < 0xff27 => APU[(sound.Address)at],
                    >= 0xff30 and < 0xff40 => APU[(sound.Address)at],

                    not (ushort)graphics.Address.DMA and >= 0xff40 and < 0xff50 => PPU[(graphics.Address)at],

                    (ushort)graphics.Address.DMA => DMA.Register,

                    0xff50 => BootRom.Register,
                    >= 0xff80 and < 0xffff => HRAM[at],

                    0xffff => InterruptRegisters.InterruptControlRegister,

                    _ => (byte)0xff
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
                UnusableMEM[at] = value;
                break;

                case 0xff00:
                Keypad.Register = value;
                break;

                case 0xff01:
                Serial.Data = value;
                break;

                case 0xff02:
                Serial.Control = value;
                break;

                case >= 0xff04 and < 0xff08:
                Timers[at] = value;
                break;

                case 0xff0f:
                InterruptRegisters.InterruptFireRegister = value;
                break;

                case >= 0xff10 and < 0xff27:
                APU[(sound.Address)at] = value;
                break;

                case >= 0xff30 and < 0xff40:
                APU[(sound.Address)at] = value;
                break;

                case not (ushort)graphics.Address.DMA and >= 0xff40 and < 0xff50:
                PPU[(graphics.Address)at] = value;
                break;

                case (ushort)graphics.Address.DMA:
                DMA.Register = value;
                break;

                case 0xff50:
                BootRom.Register = value;
                break;

                case >= 0xff80 and < 0xffff:
                HRAM[at] = value;
                break;
                case 0xffff:
                InterruptRegisters.InterruptControlRegister = value;
                break;
            }
        }
    }

    private readonly BootRom BootRom;
    private readonly InterruptRegisters InterruptRegisters;
    private readonly Keypad Keypad;
    private readonly Serial Serial;
    private readonly Timers Timers;
    private readonly PPU PPU;
    private readonly DMARegister DMA;

    public MMU(
        BootRom boot,
        MBC card,
        VRAM vram,
        OAM oam,

        Keypad keypad,
        Serial serial,
        Timers timers,
        sound.APU apu,
        InterruptRegisters interruptRegisters,
        PPU ppu,
        DMARegister dma
        )
    {


        var wram = new WRAM();
        var hram = new HRAM();
        UnusableMEM = new UnusableMEM();

        APU = apu;
        Card = card;
        VRAM = vram;
        WRAM = wram;
        OAM = oam;
        HRAM = hram;

        BootRom = boot;
        InterruptRegisters = interruptRegisters;
        Keypad = keypad;
        Serial = serial;
        Timers = timers;
        PPU = ppu;
        DMA = dma;
    }

    public void Write(ushort at, byte arg) => this[at] = arg;
    public byte Read(ushort at) => this[at];

    public byte ExternalBusRAM(ushort at) => at < 0xfe00 ? this[at] : WRAM[at];
}
