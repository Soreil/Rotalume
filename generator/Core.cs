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

        //Global clock from which all timing derives
        public long Clock;

        //Opcode fetcher
        public Func<byte> Read;

        public CPU CPU;

        public PPU PPU;
        public Timers Timers;

        byte keypadFlags = 0x30;

        Func<bool> GetKeyboardInterrupt = () => false;

        public byte InterruptFireRegister { get; set; }

        public byte InterruptControlRegister { get; set; }

        readonly ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);
        readonly ControlRegister interruptRegisters = new ControlRegister(0xffff, 0x1); //This is only being used for two registers.

        //Constructor just for tests which don't care about a functioning bootrom
        public Core(List<byte> l) : this(new List<byte>(), l)
        {
            bootROMActive = false;
        }

        public Core(List<byte> bootROM, List<byte> gameROM, Func<byte, byte> GetJoyPad, Func<bool> getKeyboardInterrupt)
        {
            Func<ushort> GetProgramCounter = () => PC;
            Action<ushort> SetProgramCounter = (x) => { PC = x; };
            //Maybe adding to the timers should be handled by a clock object rather than just this one lambda
            Action<long> IncrementClock = (x) => { Clock += x; Timers.Add(x); };
            Read = () => CPU.Memory[PC++];

            PPU = new PPU(() => Clock, () => InterruptFireRegister = InterruptFireRegister.SetBit(0),
                                       () => InterruptFireRegister = InterruptFireRegister.SetBit(1));

            Timers = new Timers(() => InterruptFireRegister = InterruptFireRegister.SetBit(2));

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

            PaletteController = (byte b) => PPU.BGP = b;
            ReadPalette = () => PPU.BGP;

            LYCController = (byte b) => PPU.LYC = b;
            ReadLYC = () => PPU.LYC;

            GetKeyboardInterrupt = getKeyboardInterrupt;

            controlRegisters.Writer[0] += x => keypadFlags = x;
            controlRegisters.Reader[0] += () => GetJoyPad(keypadFlags);

            controlRegisters.Writer[0xF] += x => InterruptFireRegister = x;
            controlRegisters.Reader[0xF] += () => InterruptFireRegister;

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

            interruptRegisters.Writer[0x00] += x => InterruptControlRegister = x;
            interruptRegisters.Reader[0x00] += () => InterruptControlRegister;

            List<MMU.SetRange> setRanges = new();
            List<MMU.GetRange> getRanges = new();

            setRanges.Add(new(
                interruptRegisters.Start,
                interruptRegisters.Start + interruptRegisters.Size,
                interruptRegisters.ContainsWriter,
                (x, v) => interruptRegisters[x] = v));
            getRanges.Add(new(
                interruptRegisters.Start,
                interruptRegisters.Start + interruptRegisters.Size,
                interruptRegisters.ContainsReader,
                x => interruptRegisters[x]));

            setRanges.Add(new MMU.SetRange(
                controlRegisters.Start,
                controlRegisters.Start + controlRegisters.Size,
                controlRegisters.ContainsWriter, (x, v) => controlRegisters[x] = v)
                );
            getRanges.Add(new MMU.GetRange(
                controlRegisters.Start,
                controlRegisters.Start + controlRegisters.Size,
                controlRegisters.ContainsReader, (x) => controlRegisters[x])
                );


            //Graphics memory ranges
            setRanges.Add(new MMU.SetRange(
                VRAM.Start,
                VRAM.Start + VRAM.Size,
                x => true,
                (at, v) => PPU.VRAM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                VRAM.Start,
                VRAM.Start + VRAM.Size,
                x => true,
                (at) => PPU.VRAM[at]
                ));
            setRanges.Add(new MMU.SetRange(
                OAM.Start,
                OAM.Start + OAM.Size,
                x => true,
                (at, v) => PPU.OAM[at] = v
                ));
            getRanges.Add(new MMU.GetRange(
                OAM.Start,
                OAM.Start + OAM.Size,
                x => true,
                (at) => PPU.OAM[at]
                ));

            var memory = new MMU(Read,
    bootROM,
    gameROM,
    () => bootROMActive,
    getRanges,
    setRanges);

            CPU = new CPU(GetProgramCounter, SetProgramCounter, IncrementClock, memory);


        }

        public Core(List<byte> bootROM, List<byte> gameROM) : this(bootROM, gameROM, (x) => 0x0f, () => false)
        {
        }

        public void DoNextOP()
        {
            if (CPU.Halted)
            {
                Clock += 4; //Just take some time so the interrupt handling and gpu keep going otherwise
                //We can never get to a situation where the halt state stops.
                return;
            }

            var op = Read();
            if (op != 0xcb)
            {
                //if ((Unprefixed)op != Unprefixed.CPL && (Unprefixed)op != Unprefixed.NOP && (Unprefixed)op != Unprefixed.RST_38H)
                //Unprefixeds.Push((PC - 1, (Unprefixed)op));
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
            return System.IO.File.ReadAllBytes(@"..\..\..\..\generator\bootrom\DMG_ROM_BOOT.bin").ToList();
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