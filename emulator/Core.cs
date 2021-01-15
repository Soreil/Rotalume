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
        private byte bootROMField = 0;

        readonly ControlRegister.Write BootROMFlagController;
        readonly ControlRegister.Read ReadBootROMFlag;
        readonly ControlRegister.Write LCDControlController;
        readonly ControlRegister.Read ReadLCDControl;
        readonly ControlRegister.Write LCDStatController;
        readonly ControlRegister.Read ReadLCDStat;
        readonly ControlRegister.Write ScrollYController;
        readonly ControlRegister.Read ReadScrollY;
        readonly ControlRegister.Write ScrollXController;
        readonly ControlRegister.Read ReadScrollX;
        readonly ControlRegister.Write LCDLineController;
        readonly ControlRegister.Read ReadLine;
        readonly ControlRegister.Write LYCController;
        readonly ControlRegister.Read ReadLYC;
        readonly ControlRegister.Write PaletteController;
        readonly ControlRegister.Read ReadPalette;
        readonly ControlRegister.Write OBP0Controller;
        readonly ControlRegister.Read ReadOBP0;
        readonly ControlRegister.Write OBP1Controller;
        readonly ControlRegister.Read ReadOBP1;
        readonly ControlRegister.Write WYController;
        readonly ControlRegister.Read ReadWY;
        readonly ControlRegister.Write WXController;
        readonly ControlRegister.Read ReadWX;

        //Global clock from which all timing derives
        public long Clock;

        //Opcode fetcher
        public Func<byte> Read;

        public CPU CPU;

        public PPU PPU;
        public Timers Timers;
        public HRAM HRAM;
        public WRAM WRAM;
        public UnusableMEM UnusableMEM;

        byte keypadFlags = 0x30;

        Func<bool> GetKeyboardInterrupt = () => false;

        public byte InterruptFireRegister { get; set; }

        public byte InterruptControlRegister { get; set; }

        readonly ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);
        readonly ControlRegister interruptRegisters = new ControlRegister(0xffff, 0x1); //This is only being used for two registers.

        //Constructor just for tests which don't care about a functioning bootrom
        public Core(List<byte> l, List<byte> bootrom = null) : this(l, bootrom, (x) => 0x01f, () => false)
        {
            if (bootrom == null)
            {
                bootROMActive = false;
            }
        }

        public Core(List<byte> gameROM, List<byte> bootROM, Func<byte, byte> GetJoyPad, Func<bool> getKeyboardInterrupt)
        {
            Func<ushort> GetProgramCounter = () => PC;
            Action<ushort> SetProgramCounter = (x) => { PC = x; };
            //Maybe adding to the timers should be handled by a clock object rather than just this one lambda
            Action<long> IncrementClock = (x) => { Clock += x; Timers.Add(x); };
            Read = () => CPU.Memory[PC++];

            PPU = new PPU(() => Clock, () => InterruptFireRegister = InterruptFireRegister.SetBit(0),
                                       () => InterruptFireRegister = InterruptFireRegister.SetBit(1));

            Timers = new Timers(() => InterruptFireRegister = InterruptFireRegister.SetBit(2));

            HRAM = new HRAM();
            WRAM = new WRAM();
            UnusableMEM = new UnusableMEM();

            BootROMFlagController = (byte b) =>
            {
                bootROMField = b;
                if (b == 1)
                {
                    controlRegisters.Writer[0x50] -= BootROMFlagController;
                    bootROMActive = false;
                }
            };

            ReadBootROMFlag = () => bootROMField;

            LCDControlController = (byte b) => PPU.SetLCDC(b);
            ReadLCDControl = () => PPU.LCDC;

            LCDStatController = (byte b) => PPU.STAT = b;
            ReadLCDStat = () => PPU.STAT;

            ScrollYController = (byte b) => PPU.SCY = b;
            ReadScrollY = () => PPU.SCY;
            ScrollXController = (byte b) => PPU.SCX = b;
            ReadScrollX = () => PPU.SCX;
            LCDLineController = (byte b) => PPU.LY = b;
            ReadLine = () => PPU.LY;

            PaletteController = (byte b) => PPU.BGP = b;
            ReadPalette = () => PPU.BGP;

            OBP0Controller = (byte b) => PPU.OBP0 = b;
            ReadOBP0 = () => PPU.OBP0;

            OBP1Controller = (byte b) => PPU.OBP1 = b;
            ReadOBP1 = () => PPU.OBP1;

            WYController = (byte b) => PPU.WY = b;
            ReadWY = () => PPU.WY;

            WXController = (byte b) => PPU.WX = b;
            ReadWX = () => PPU.WX;

            PaletteController = (byte b) => PPU.BGP = b;
            ReadPalette = () => PPU.BGP;

            LYCController = (byte b) => PPU.LYC = b;
            ReadLYC = () => PPU.LYC;

            GetKeyboardInterrupt = getKeyboardInterrupt;

            controlRegisters.Writer[0] += x => keypadFlags = x;
            controlRegisters.Reader[0] += () => GetJoyPad(keypadFlags);

            controlRegisters.Writer[0xF] += x => InterruptFireRegister = x;
            controlRegisters.Reader[0xF] += () => InterruptFireRegister;

            controlRegisters.Writer[0x04] += x => Timers.Divider = x;
            controlRegisters.Reader[0x04] += () => Timers.Divider;

            controlRegisters.Writer[0x05] += x => Timers.Timer = x;
            controlRegisters.Reader[0x05] += () => Timers.Timer;

            controlRegisters.Writer[0x06] += x => Timers.TimerDefault = x;
            controlRegisters.Reader[0x06] += () => Timers.TimerDefault;

            controlRegisters.Writer[0x07] += x => Timers.TimerControl = x;
            controlRegisters.Reader[0x07] += () => Timers.TimerControl;

            controlRegisters.Writer[0x50] += BootROMFlagController;
            controlRegisters.Reader[0x50] += ReadBootROMFlag;

            //PPU registers
            controlRegisters.Writer[0x40] += LCDControlController;
            controlRegisters.Reader[0x40] += ReadLCDControl;
            controlRegisters.Writer[0x41] += LCDStatController;
            controlRegisters.Reader[0x41] += ReadLCDStat;
            controlRegisters.Writer[0x42] += ScrollYController;
            controlRegisters.Reader[0x42] += ReadScrollY;
            controlRegisters.Writer[0x43] += ScrollXController;
            controlRegisters.Reader[0x43] += ReadScrollX;
            controlRegisters.Writer[0x44] += LCDLineController;
            controlRegisters.Reader[0x44] += ReadLine;
            controlRegisters.Writer[0x45] += LYCController;
            controlRegisters.Reader[0x45] += ReadLYC;

            //DMA
            controlRegisters.Writer[0x46] += (x) =>
            {
                if (x > 0xf1) throw new Exception("Illegal DMA start adress");

                ushort baseAddr = (ushort)(x << 8);
                for (int i = 0; i < OAM.Size; i++)
                {
                    var r = CPU.Memory.Read((ushort)(baseAddr + i));
                    PPU.OAM[OAM.Start + i] = r;
                }
            };

            controlRegisters.Reader[0x46] += () => throw new Exception("DMAREAD");

            controlRegisters.Writer[0x47] += PaletteController;
            controlRegisters.Reader[0x47] += ReadPalette;
            controlRegisters.Writer[0x48] += OBP0Controller;
            controlRegisters.Reader[0x48] += ReadOBP0;
            controlRegisters.Writer[0x49] += OBP1Controller;
            controlRegisters.Reader[0x49] += ReadOBP1;

            controlRegisters.Writer[0x4A] += WYController;
            controlRegisters.Reader[0x4A] += ReadWY;
            controlRegisters.Writer[0x4B] += WXController;
            controlRegisters.Reader[0x4B] += ReadWX;

            interruptRegisters.Writer[0x00] += x => InterruptControlRegister = x;
            interruptRegisters.Reader[0x00] += () => InterruptControlRegister;

            for (ushort SoundRegister = 0xff10; SoundRegister <= 0xff26; SoundRegister++)
            {
                controlRegisters.Writer[SoundRegister & 0xff] += (x) => { };
                controlRegisters.Reader[SoundRegister & 0xff] += () => 0xff;
            }

            for (ushort SoundWave = 0xff30; SoundWave <= 0xff3f; SoundWave++)
            {
                controlRegisters.Writer[SoundWave & 0xff] += (x) => { };
                controlRegisters.Reader[SoundWave & 0xff] += () => 0xff;
            }

            for (ushort Serial = 0xff01; Serial <= 0xff02; Serial++)
            {
                controlRegisters.Writer[Serial & 0xff] += (x) => { };
                controlRegisters.Reader[Serial & 0xff] += () => 0xff;
            }
            for (ushort Unused = 0xff4c; Unused < 0xff80; Unused++)
            {
                if (Unused == 0xff50) continue;
                controlRegisters.Writer[Unused & 0xff] += (x) => { };
                controlRegisters.Reader[Unused & 0xff] += () => 0xff;
            }

            List<MMU.SetRange> setRanges = new();
            List<MMU.GetRange> getRanges = new();

            setRanges.Add(new(
                interruptRegisters.Start,
                interruptRegisters.Start + interruptRegisters.Size,
                (x, v) => interruptRegisters[x] = v));
            getRanges.Add(new(
                interruptRegisters.Start,
                interruptRegisters.Start + interruptRegisters.Size,
                x => interruptRegisters[x]));

            setRanges.Add(new MMU.SetRange(
                controlRegisters.Start,
                controlRegisters.Start + controlRegisters.Size,
                (x, v) => controlRegisters[x] = v)
                );
            getRanges.Add(new MMU.GetRange(
                controlRegisters.Start,
                controlRegisters.Start + controlRegisters.Size,
                (x) => controlRegisters[x])
                );

            //HRAM memory range
            setRanges.Add(new MMU.SetRange(
                HRAM.Start,
                HRAM.Start + HRAM.Size,
                (at, v) => HRAM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                HRAM.Start,
                HRAM.Start + HRAM.Size,
                (at) => HRAM[at]
                ));

            //Work RAM memory range
            setRanges.Add(new MMU.SetRange(
                WRAM.Start,
                WRAM.Start + WRAM.Size,
                (at, v) => WRAM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                WRAM.Start,
                WRAM.Start + WRAM.Size,
                (at) => WRAM[at]
                ));
            //Mirror of Work RAM
            setRanges.Add(new MMU.SetRange(
                WRAM.MirrorStart,
                WRAM.MirrorStart + WRAM.Size,
                (at, v) => WRAM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                WRAM.MirrorStart,
                WRAM.MirrorStart + WRAM.Size,
                (at) => WRAM[at]
                ));

            //Illegal memory range (used by tetris though?)
            setRanges.Add(new MMU.SetRange(
                UnusableMEM.Start,
                UnusableMEM.Start + UnusableMEM.Size,
                (at, v) => UnusableMEM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                UnusableMEM.Start,
                UnusableMEM.Start + UnusableMEM.Size,
                (at) => UnusableMEM[at]
                ));


            //Graphics memory ranges
            setRanges.Add(new MMU.SetRange(
                VRAM.Start,
                VRAM.Start + VRAM.Size,
                (at, v) => PPU.VRAM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                VRAM.Start,
                VRAM.Start + VRAM.Size,
                (at) => PPU.VRAM[at]
                ));
            setRanges.Add(new MMU.SetRange(
                OAM.Start,
                OAM.Start + OAM.Size,
                (at, v) => PPU.OAM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                OAM.Start,
                OAM.Start + OAM.Size,
                (at) => PPU.OAM[at]
                ));

            CartHeader Header = gameROM.Count < 0x8000 ? null : new CartHeader(gameROM);
            MBC Card;
            if (Header is not null) Card = MakeMBC(Header, gameROM);
            else Card = MakeFakeMBC(gameROM);

            setRanges.Add(new MMU.SetRange(
                MBC.ROMStart,
                MBC.ROMStart + MBC.ROMSize,
                (at, v) => Card[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                MBC.ROMStart,
                MBC.ROMStart + MBC.ROMSize,
                (at) => Card[at]
                ));

            setRanges.Add(new MMU.SetRange(
                MBC.RAMStart,
                MBC.RAMStart + MBC.RAMSize,
                (at, v) => Card[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                MBC.RAMStart,
                MBC.RAMStart + MBC.RAMSize,
                (at) => Card[at]
                ));

            var memory = new MMU(Read,
    bootROM,
    () => bootROMActive,
    getRanges,
    setRanges);

            CPU = new CPU(GetProgramCounter, SetProgramCounter, IncrementClock, memory);
        }

        private MBC MakeFakeMBC(List<byte> gameROM) => new ROMONLY(gameROM);

        private static MBC MakeMBC(CartHeader header, List<byte> gameROM) => header.Type switch
        {
            CartType.ROM_ONLY => new ROMONLY(gameROM),
            CartType.MBC1 => new MBC1(header, gameROM),
            CartType.MBC1_RAM_BATTERY => new MBC1(header, gameROM),
            _ => throw new NotImplementedException(),
        };

        public void DoNextOP()
        {
            if (CPU.Halted)
            {
                Clock += 4; //Just take some time so the interrupt handling and gpu keep going otherwise
                //We can never get to a situation where the halt state stops.
                Timers.Add(4);
                return;
            }

            var op = Read();
            if (op != 0xcb)
            {
                //if ((Unprefixed)op != Unprefixed.CPL && (Unprefixed)op != Unprefixed.NOP && (Unprefixed)op != Unprefixed.RST_38H)
                Unprefixeds.Push((PC - 1, (Unprefixed)op));
                CPU.Op((Unprefixed)op)();
            }
            else
            {
                var CBop = Read(); //Because of the CB prefix we encountered in the previous case we already skipped the extra byte of a cb instruction here
                CPU.Op((Cbprefixed)CBop)();
            }
        }

        public void DoInterrupt()
        {
            byte coincidence = (byte)((InterruptControlRegister & InterruptFireRegister) & 0x1f); //Coincidence has all the bits which have both fired AND are enabled
            if (coincidence != 0)
                CPU.Halted = false;

            if (!CPU.IME || coincidence == 0) return; //Interrupts have to be globally enabled to use them
            for (int bit = 0; bit < 5; bit++) //Bit 0 has highest priority, we only handle one interrupt at a time
            {
                if (coincidence.GetBit(bit))
                {
                    CPU.IME = false;
                    InterruptFireRegister = InterruptFireRegister.SetBit(bit, false);

                    var addr = (ushort)(0x40 + (0x8 * bit));
                    CPU.Call(24, addr); //We need a cleaner way to call functions without fetching

                    return;
                }
            }
        }

        public static List<byte> LoadBootROM()
        {
            return System.IO.File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin").ToList();
        }

        public void Step()
        {
            DoNextOP();
            //We really should have the GUI thread somehow do this logic but polling like this should work
            if (!InterruptFireRegister.GetBit(4) && GetKeyboardInterrupt())
                InterruptFireRegister = InterruptFireRegister.SetBit(4);

            DoInterrupt();
            PPU.Do();
        }

    }
}