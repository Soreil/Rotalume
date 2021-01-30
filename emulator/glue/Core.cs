using System;
using System.Collections.Generic;
using System.Linq;

namespace emulator
{
    public class Core
    {
        public ushort PC;

        public Stack<(int, Unprefixed)> Unprefixeds = new();

        //Not sure where this special bit should go but it's not in memory and suposed to be hard to access
        bool bootROMActive = true;

        //Global clock from which all timing derives
        public long Clock;

        //Opcode fetcher
        public Func<byte> Read;
        public Func<byte> ReadHaltBug;

        public CPU CPU;
        public APU APU;
        public PPU PPU;
        public Timers Timers;

        byte keypadFlags = 0x30;

        Func<bool> GetKeyboardInterrupt = () => false;

        private byte _dma = 0xff;
        private byte serialControl = 0x7e;


        readonly ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);
        readonly ControlRegister interruptRegisters = new ControlRegister(0xffff, 0x1); //This is only being used for two registers.

        public Core(List<byte> bwah) : this(bwah.ToArray())
        { }
        //Constructor just for tests which don't care about a functioning bootrom
        public Core(byte[] l, byte[] bootrom = null) : this(l.Length < 0x8000 ? PadAndMoveTo0x100(l) : l, bootrom, (x) => 0x01f, () => false)
        { }

        private static byte[] PadAndMoveTo0x100(byte[] l)
        {
            byte[] buffer = new byte[0x8000];

            l.CopyTo(buffer, 0x100);
            return buffer;
        }

        public Core(byte[] gameROM, byte[] bootROM, Func<byte, byte> GetJoyPad, Func<bool> getKeyboardInterrupt)
        {
            Func<ushort> GetProgramCounter = () => PC;
            Action<ushort> SetProgramCounter = (x) => { PC = x; };
            //Maybe adding to the timers should be handled by a clock object rather than just this one lambda
            Action<long> IncrementClock = (x) => { Clock += x; Timers.Add(x); };
            Read = () => CPU.Memory[PC++];
            ReadHaltBug = () =>
            {
                CPU.Halted = HaltState.off;
                return CPU.Memory[PC];
            };

            APU = new APU();
            PPU = new PPU(() => Clock, () => CPU.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(0),
                                       () => CPU.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(1));

            Timers = new Timers(() => CPU.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(2));

            Func<byte> ReadBootROMFlag = () => 0xff;
            GetKeyboardInterrupt = getKeyboardInterrupt;

            interruptRegisters.Writer[0x00] = x => CPU.InterruptControlRegister = x;
            interruptRegisters.Reader[0x00] = () => CPU.InterruptControlRegister;

            var ioRegisters = SetupControlRegisters(GetJoyPad, ReadBootROMFlag);

            CartHeader Header = gameROM.Length < 0x8000 ? null : new CartHeader(gameROM);
            MBC Card;
            if (Header is not null) Card = MakeMBC(Header, gameROM);
            else Card = MakeFakeMBC(gameROM);

            var memory = new MMU(Read,
    bootROM,
    () => bootROMActive,
    Card,
    PPU.VRAM,
    PPU.OAM,
    ioRegisters,
    interruptRegisters
    );

            CPU = new CPU(GetProgramCounter, SetProgramCounter, IncrementClock, memory);

            if (bootROM == null)
            {

                bootROMActive = false;

                //registers
                PC = 0x100;
                CPU.Registers.AF = 0x0100;
                CPU.Registers.BC = 0xff13;
                CPU.Registers.DE = 0x00c1;
                CPU.Registers.HL = 0x8403;
                CPU.Registers.SP = 0xfffe;

                //timers
                Timers.TIMA = 0;
                Timers.TAC = 0;
                Timers.TMA = 0;
                //Timers.InternalCounter = 0xabcc;
                Timers.InternalCounter = 0x1800;

                CPU.InterruptFireRegister = 0xe1;

                //sound
                APU.NR10 = 0x80;
                APU.NR12 = 0xf3;
                APU.NR14 = 0xbf;
                APU.NR21 = 0x3f;
                APU.NR22 = 0x00;
                APU.NR24 = 0xbf;
                APU.NR30 = 0x7f;
                APU.NR32 = 0x9f;
                APU.NR34 = 0xbf;
                APU.NR42 = 0x00;
                APU.NR43 = 0x00;
                APU.NR44 = 0xbf;
                APU.NR50 = 0x77;
                APU.NR51 = 0xf3;
                APU.NR52 = 0xf1;

                //graphics TODO: we can't really set up the graphics environment correctly
                //because we will have to also initialize the internal renderer state correctly
                PPU.LCDC = 0x91;
                PPU.STAT = 0x83;

                PPU.SCY = 0;
                PPU.SCX = 0;
                PPU.WY = 0;
                PPU.WX = 0;
                PPU.LYC = 0;
                PPU.LY = 1;

                PPU.BGP = 0xfc;
                PPU.OBP0 = 0xff;
                PPU.OBP1 = 0xff;

                CPU.IME = true;
            }
        }

        private ControlRegister SetupControlRegisters(Func<byte, byte> GetJoyPad, Func<byte> ReadBootROMFlag)
        {
            ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);

            Action<byte> BootROMFlagController = (byte b) =>
            {
                if (b == 1)
                {
                    bootROMActive = false;
                }
            };

            Action<byte> LCDControlController = (byte b) => PPU.LCDC = b;
            Func<byte> ReadLCDControl = () => PPU.LCDC;

            Action<byte> LCDStatController = (byte b) => PPU.STAT = (byte)((b & 0xf8) | (PPU.STAT & 0x7));
            Func<byte> ReadLCDStat = () => PPU.STAT;

            Action<byte> ScrollYController = (byte b) => PPU.SCY = b;
            Func<byte> ReadScrollY = () => PPU.SCY;
            Action<byte> ScrollXController = (byte b) => PPU.SCX = b;
            Func<byte> ReadScrollX = () => PPU.SCX;
            Action<byte> LCDLineController = (byte b) => PPU.LY = b;
            Func<byte> ReadLine = () => PPU.LY;

            Action<byte> PaletteController = (byte b) => PPU.BGP = b;
            Func<byte> ReadPalette = () => PPU.BGP;

            Action<byte> OBP0Controller = (byte b) => PPU.OBP0 = b;
            Func<byte> ReadOBP0 = () => PPU.OBP0;

            Action<byte> OBP1Controller = (byte b) => PPU.OBP1 = b;
            Func<byte> ReadOBP1 = () => PPU.OBP1;

            Action<byte> WYController = (byte b) => PPU.WY = b;
            Func<byte> ReadWY = () => PPU.WY;

            Action<byte> WXController = (byte b) => PPU.WX = b;
            Func<byte> ReadWX = () => PPU.WX;

            Action<byte> LYCController = (byte b) => PPU.LYC = b;
            Func<byte> ReadLYC = () => PPU.LYC;


            controlRegisters.Writer[0] = x => keypadFlags = (byte)(x & 0xf0);
            controlRegisters.Reader[0] = () => GetJoyPad(keypadFlags);

            controlRegisters.Writer[0xF] = x => CPU.InterruptFireRegister = x;
            controlRegisters.Reader[0xF] = () => CPU.InterruptFireRegister;

            controlRegisters.Writer[0x04] = x => Timers.DIV = x;
            controlRegisters.Reader[0x04] = () => Timers.DIV;

            controlRegisters.Writer[0x05] = x => Timers.TIMA = x;
            controlRegisters.Reader[0x05] = () => Timers.TIMA;

            controlRegisters.Writer[0x06] = x => Timers.TMA = x;
            controlRegisters.Reader[0x06] = () => Timers.TMA;

            controlRegisters.Writer[0x07] = x => Timers.TAC = x;
            controlRegisters.Reader[0x07] = () => Timers.TAC;

            controlRegisters.Writer[0x50] = BootROMFlagController;
            controlRegisters.Reader[0x50] = ReadBootROMFlag;

            //PPU registers
            controlRegisters.Writer[0x40] = LCDControlController;
            controlRegisters.Reader[0x40] = ReadLCDControl;
            controlRegisters.Writer[0x41] = LCDStatController;
            controlRegisters.Reader[0x41] = ReadLCDStat;
            controlRegisters.Writer[0x42] = ScrollYController;
            controlRegisters.Reader[0x42] = ReadScrollY;
            controlRegisters.Writer[0x43] = ScrollXController;
            controlRegisters.Reader[0x43] = ReadScrollX;
            controlRegisters.Writer[0x44] = LCDLineController;
            controlRegisters.Reader[0x44] = ReadLine;
            controlRegisters.Writer[0x45] = LYCController;
            controlRegisters.Reader[0x45] = ReadLYC;

            //DMA
            controlRegisters.Writer[0x46] = (x) =>
            {
                if (x > 0xf1) throw new Exception("Illegal DMA start adress");
                _dma = x;

                ushort baseAddr = (ushort)(x << 8);
                for (int i = 0; i < OAM.Size; i++)
                {
                    var r = CPU.Memory.Read((ushort)(baseAddr + i));
                    PPU.OAM[OAM.Start + i] = r;
                }
            };

            controlRegisters.Reader[0x46] = () => _dma;

            //PPU registers
            controlRegisters.Writer[0x47] = PaletteController;
            controlRegisters.Reader[0x47] = ReadPalette;
            controlRegisters.Writer[0x48] = OBP0Controller;
            controlRegisters.Reader[0x48] = ReadOBP0;
            controlRegisters.Writer[0x49] = OBP1Controller;
            controlRegisters.Reader[0x49] = ReadOBP1;

            controlRegisters.Writer[0x4A] = WYController;
            controlRegisters.Reader[0x4A] = ReadWY;
            controlRegisters.Writer[0x4B] = WXController;
            controlRegisters.Reader[0x4B] = ReadWX;

            //Sound
            controlRegisters.Writer[0x10] = (x) => APU.NR10 = x;
            controlRegisters.Reader[0x10] = () => APU.NR10;
            controlRegisters.Writer[0x11] = (x) => APU.NR11 = x;
            controlRegisters.Reader[0x11] = () => APU.NR11;
            controlRegisters.Writer[0x12] = (x) => APU.NR12 = x;
            controlRegisters.Reader[0x12] = () => APU.NR12;
            controlRegisters.Writer[0x14] = (x) => APU.NR14 = x;
            controlRegisters.Reader[0x14] = () => APU.NR14;
            controlRegisters.Writer[0x16] = (x) => APU.NR21 = x;
            controlRegisters.Reader[0x16] = () => APU.NR21;
            controlRegisters.Writer[0x17] = (x) => APU.NR22 = x;
            controlRegisters.Reader[0x17] = () => APU.NR22;
            controlRegisters.Writer[0x19] = (x) => APU.NR24 = x;
            controlRegisters.Reader[0x19] = () => APU.NR24;
            controlRegisters.Writer[0x1a] = (x) => APU.NR30 = x;
            controlRegisters.Reader[0x1a] = () => APU.NR30;
            controlRegisters.Writer[0x1c] = (x) => APU.NR32 = x;
            controlRegisters.Reader[0x1c] = () => APU.NR32;
            controlRegisters.Writer[0x1e] = (x) => APU.NR34 = x;
            controlRegisters.Reader[0x1e] = () => APU.NR34;
            controlRegisters.Writer[0x21] = (x) => APU.NR42 = x;
            controlRegisters.Reader[0x21] = () => APU.NR42;
            controlRegisters.Writer[0x22] = (x) => APU.NR43 = x;
            controlRegisters.Reader[0x22] = () => APU.NR43;
            controlRegisters.Writer[0x23] = (x) => APU.NR44 = x;
            controlRegisters.Reader[0x23] = () => APU.NR44;
            controlRegisters.Writer[0x24] = (x) => APU.NR50 = x;
            controlRegisters.Reader[0x24] = () => APU.NR50;
            controlRegisters.Writer[0x25] = (x) => APU.NR51 = x;
            controlRegisters.Reader[0x25] = () => APU.NR51;
            controlRegisters.Writer[0x26] = (x) => APU.NR52 = x;
            controlRegisters.Reader[0x26] = () => APU.NR52;

            for (ushort SoundRegister = 0xff13; SoundRegister <= 0xff26; SoundRegister++)
            {
                if (SoundRegister == 0xff14 ||
                    SoundRegister == 0xff16 ||
                    SoundRegister == 0xff17 ||
                    SoundRegister == 0xff19 ||
                    SoundRegister == 0xff1a ||
                    SoundRegister == 0xff1c ||
                    SoundRegister == 0xff1e ||
                    SoundRegister == 0xff21 ||
                    SoundRegister == 0xff22 ||
                    SoundRegister == 0xff23 ||
                    SoundRegister == 0xff24 ||
                    SoundRegister == 0xff25 ||
                    SoundRegister == 0xff26
                    ) continue;

                controlRegisters.Writer[SoundRegister & 0xff] = (x) => { };
                controlRegisters.Reader[SoundRegister & 0xff] = () => 0xff;
            }

            for (ushort SoundWave = 0xff30; SoundWave <= 0xff3f; SoundWave++)
            {
                controlRegisters.Writer[SoundWave & 0xff] = (x) => { };
                controlRegisters.Reader[SoundWave & 0xff] = () => 0xff;
            }

            //Serial
            controlRegisters.Writer[1] = (x) => { };
            controlRegisters.Reader[1] = () => 0;

            controlRegisters.Writer[2] = (x) => serialControl = (byte)((x & 0x81) | 0x7e);
            controlRegisters.Reader[2] = () => serialControl;

            //Not used on the DMG
            for (ushort Unused = 0xff4c; Unused < 0xff80; Unused++)
            {
                if (Unused == 0xff50) continue;
                controlRegisters.Writer[Unused & 0xff] = (x) => { };
                controlRegisters.Reader[Unused & 0xff] = () => 0xff;
            }
            return controlRegisters;
        }

        private static MBC MakeFakeMBC(byte[] gameROM) => new ROMONLY(gameROM);

        private MBC MakeMBC(CartHeader header, byte[] gameROM) => header.Type switch
        {
            CartType.ROM_ONLY => new ROMONLY(gameROM),
            CartType.MBC1 => new MBC1(header, gameROM),
            CartType.MBC1_RAM_BATTERY => new MBC1(header, gameROM),
            CartType.MBC1_RAM => new MBC1(header, gameROM),
            CartType.MBC2_BATTERY => new MBC2(header, gameROM),
            CartType.MBC2 => new MBC2(header, gameROM),
            CartType.MBC3 => new MBC3(header, gameROM, () => Clock),
            CartType.MBC3_TIMER_BATTERY => new MBC3(header, gameROM, () => Clock),
            CartType.MBC3_RAM_BATTERY => new MBC3(header, gameROM, () => Clock),
            CartType.MBC3_TIMER_RAM_BATTERY => new MBC3(header, gameROM, () => Clock),
            CartType.MBC3_RAM => new MBC3(header, gameROM, () => Clock),
            CartType.MBC5 => new MBC5(header, gameROM),
            CartType.MBC5_RAM => new MBC5(header, gameROM),
            CartType.MBC5_RAM_BATTERY => new MBC5(header, gameROM),
            _ => throw new NotImplementedException(),
        };

        public void DoNextOP()
        {
            if (CPU.Halted != HaltState.off)
            {
                Clock += 4; //Just take some time so the interrupt handling and gpu keep going otherwise
                //We can never get to a situation where the halt state stops.
                Timers.Add(4);

                if (CPU.Halted != HaltState.haltbug)
                    return;
            }

            var op = CPU.Halted == HaltState.haltbug ? ReadHaltBug() : Read();
            if (op != 0xcb)
                CPU.Op((Unprefixed)op)();
            else
            {
                var CBop = Read(); //Because of the CB prefix we encountered in the previous case we already skipped the extra byte of a cb instruction here
                CPU.Op((Cbprefixed)CBop)();
            }
        }

        public static byte[] LoadBootROM()
        {
            return System.IO.File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin");
        }

        public void Step()
        {
            DoNextOP();
            //We really should have the GUI thread somehow do this logic but polling like this should work
            if (!CPU.InterruptFireRegister.GetBit(4) && GetKeyboardInterrupt())
                CPU.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(4);

            CPU.DoInterrupt();
            if (CPU.InterruptEnableSceduled)
            {
                CPU.IME = true;
                CPU.InterruptEnableSceduled = false;
            }
            PPU.Do();
        }

    }
}