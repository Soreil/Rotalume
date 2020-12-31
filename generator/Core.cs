using System;
using System.Collections.Generic;

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
        readonly ControlRegister.Write ScrollYController;
        readonly ControlRegister.Read ReadScrollY;
        readonly ControlRegister.Write ScrollXController;
        readonly ControlRegister.Read ReadScrollX;
        readonly ControlRegister.Write LCDLineController;
        readonly ControlRegister.Read ReadLine;
        readonly ControlRegister.Write PaletteController;
        readonly ControlRegister.Read ReadPalette;

        //Global clock from which all timing derives
        public int Clock;

        //Opcode fetcher
        public Func<byte> Read;

        public CPU CPU;

        public PPU PPU;
        public Timers Timers;

        public byte InterruptFireRegister { get; set; }
        public byte InterruptControlRegister { get; set; }

        readonly ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);
        readonly ControlRegister interruptRegisters = new ControlRegister(0xffff, 0x1); //This is only being used for two registers.

        //Constructor just for tests which don't care about a functioning bootrom
        public Core(List<byte> l) : this(new List<byte>(), l)
        {
            bootROMActive = false;
        }
        public Core() : this(LoadBootROM(), new List<byte>())
        {
        }

        public Core(List<byte> bootROM, List<byte> gameROM)
        {
            Func<ushort> GetProgramCounter = () => PC;
            Action<ushort> SetProgramCounter = (x) => { PC = x; };
            //Maybe adding to the timers should be handled by a clock object rather than just this one lambda
            Action<int> IncrementClock = (x) => { Clock += x; Timers.Add(x); };
            Read = () => CPU.Memory[PC++];

            CPU = new CPU(Read, bootROM, gameROM, GetProgramCounter, SetProgramCounter, IncrementClock, () => bootROMActive);

            PPU = new PPU(() => Clock, () => InterruptFireRegister = InterruptFireRegister.SetBit(0));

            Timers = new Timers(() => InterruptFireRegister = InterruptFireRegister.SetBit(2, true));

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

            ScrollYController = (byte b) => PPU.SCY = b;
            ReadScrollY = () => PPU.SCY;
            ScrollXController = (byte b) => PPU.SCX = b;
            ReadScrollX = () => PPU.SCX;
            LCDLineController = (byte b) => PPU.LY = b;
            ReadLine = () => PPU.LY;

            PaletteController = (byte b) => PPU.BGP = b;
            ReadPalette = () => PPU.BGP;

            controlRegisters.Writer[0xF] += x => InterruptFireRegister = x;
            controlRegisters.Reader[0xF] += () => InterruptFireRegister;

            controlRegisters.Writer[0x50] += BootROMFlagController;
            controlRegisters.Reader[0x50] += ReadBootROMFlag;

            //PPU registers
            controlRegisters.Writer[0x40] += LCDControlController;
            controlRegisters.Reader[0x40] += ReadLCDControl;
            controlRegisters.Writer[0x42] += ScrollYController;
            controlRegisters.Reader[0x42] += ReadScrollY;
            controlRegisters.Writer[0x43] += ScrollXController;
            controlRegisters.Reader[0x43] += ReadScrollX;
            controlRegisters.Writer[0x44] += LCDLineController;
            controlRegisters.Reader[0x44] += ReadLine;
            controlRegisters.Writer[0x47] += PaletteController;
            controlRegisters.Reader[0x47] += ReadPalette;

            //DMA
            controlRegisters.Writer[0x46] += (x) =>
            {
                if (x > 0xf1) throw new Exception("Illegal DMA start adress");

                ushort baseAddr = (ushort)(x << 8);
                ushort destinationBaseAddr = 0xFE00;
                for (int i = 0; i < 0xa0; i++)
                {
                    var r = CPU.Memory.Read((ushort)(baseAddr + i));
                    CPU.Memory.Write((ushort)(destinationBaseAddr + i), r);
                }

            };

            controlRegisters.Reader[0x46] += () => throw new Exception("DMAREAD");

            interruptRegisters.Writer[0x00] += x => InterruptControlRegister = x;
            interruptRegisters.Reader[0x00] += () => InterruptControlRegister;

            CPU.Memory.setRanges.Add(new(
                interruptRegisters.Start,
                interruptRegisters.Start + interruptRegisters.Size,
                interruptRegisters.ContainsWriter,
                (x, v) => interruptRegisters[x] = v));
            CPU.Memory.getRanges.Add(new(
                interruptRegisters.Start,
                interruptRegisters.Start + interruptRegisters.Size,
                interruptRegisters.ContainsReader,
                x => interruptRegisters[x]));

            CPU.Memory.setRanges.Add(new MMU.SetRange(
                controlRegisters.Start,
                controlRegisters.Start + controlRegisters.Size,
                controlRegisters.ContainsWriter, (x, v) => controlRegisters[x] = v)
                );
            CPU.Memory.getRanges.Add(new MMU.GetRange(
                controlRegisters.Start,
                controlRegisters.Start + controlRegisters.Size,
                controlRegisters.ContainsReader, (x) => controlRegisters[x])
                );


            //Graphics memory ranges
            CPU.Memory.setRanges.Add(new MMU.SetRange(
                VRAM.Start,
                VRAM.Start + VRAM.Size,
                x => true,
                (at, v) => PPU.VRAM[at] = v
                ));
            CPU.Memory.getRanges.Add(new MMU.GetRange(
                VRAM.Start,
                VRAM.Start + VRAM.Size,
                x => true,
                (at) => PPU.VRAM[at]
                ));
            CPU.Memory.setRanges.Add(new MMU.SetRange(
                OAM.Start,
                OAM.Start + OAM.Size,
                x => true,
                (at, v) => PPU.OAM[at] = v
                ));
            CPU.Memory.getRanges.Add(new MMU.GetRange(
                OAM.Start,
                OAM.Start + OAM.Size,
                x => true,
                (at) => PPU.OAM[at]
                ));
        }

        public void DoNextOP()
        {
            if (CPU.Halted) return;

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

            byte coincidence = (byte)(InterruptControlRegister & InterruptFireRegister); //Coincidence has all the bits which have both fired AND are enabled
            if (coincidence != 0)
                CPU.Halted = false;

            if (!CPU.IME) return; //Interrupts have to be globally enabled to use them
            for (int bit = 0; bit < 5; bit++) //Bit 0 has highest priority, we only handle one interrupt at a time
            {
                if (coincidence.GetBit(bit))
                {
                    CPU.IME = false;
                    coincidence.SetBit(bit, false);

                    var addr = (ushort)(0x40 + (0x8 * bit));
                    CPU.Call(24, addr); //We need a cleaner way to call functions without fetching

                    return;
                }
            }
        }

        public static List<byte> LoadBootROM()
        {
            byte[] bootROM = {
    0x31, 0xFE, 0xFF, 0xAF, 0x21, 0xFF, 0x9F, 0x32, 0xCB, 0x7C, 0x20, 0xFB,
    0x21, 0x26, 0xFF, 0x0E, 0x11, 0x3E, 0x80, 0x32, 0xE2, 0x0C, 0x3E, 0xF3,
    0xE2, 0x32, 0x3E, 0x77, 0x77, 0x3E, 0xFC, 0xE0, 0x47, 0x11, 0x04, 0x01,
    0x21, 0x10, 0x80, 0x1A, 0xCD, 0x95, 0x00, 0xCD, 0x96, 0x00, 0x13, 0x7B,
    0xFE, 0x34, 0x20, 0xF3, 0x11, 0xD8, 0x00, 0x06, 0x08, 0x1A, 0x13, 0x22,
    0x23, 0x05, 0x20, 0xF9, 0x3E, 0x19, 0xEA, 0x10, 0x99, 0x21, 0x2F, 0x99,
    0x0E, 0x0C, 0x3D, 0x28, 0x08, 0x32, 0x0D, 0x20, 0xF9, 0x2E, 0x0F, 0x18,
    0xF3, 0x67, 0x3E, 0x64, 0x57, 0xE0, 0x42, 0x3E, 0x91, 0xE0, 0x40, 0x04,
    0x1E, 0x02, 0x0E, 0x0C, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x0D, 0x20,
    0xF7, 0x1D, 0x20, 0xF2, 0x0E, 0x13, 0x24, 0x7C, 0x1E, 0x83, 0xFE, 0x62,
    0x28, 0x06, 0x1E, 0xC1, 0xFE, 0x64, 0x20, 0x06, 0x7B, 0xE2, 0x0C, 0x3E,
    0x87, 0xE2, 0xF0, 0x42, 0x90, 0xE0, 0x42, 0x15, 0x20, 0xD2, 0x05, 0x20,
    0x4F, 0x16, 0x20, 0x18, 0xCB, 0x4F, 0x06, 0x04, 0xC5, 0xCB, 0x11, 0x17,
    0xC1, 0xCB, 0x11, 0x17, 0x05, 0x20, 0xF5, 0x22, 0x23, 0x22, 0x23, 0xC9,
    0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83,
    0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E,
    0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63,
    0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E,
    0x3C, 0x42, 0xB9, 0xA5, 0xB9, 0xA5, 0x42, 0x3C, 0x21, 0x04, 0x01, 0x11,
    0xA8, 0x00, 0x1A, 0x13, 0xBE, 0x20, 0xFE, 0x23, 0x7D, 0xFE, 0x34, 0x20,
    0xF5, 0x06, 0x19, 0x78, 0x86, 0x23, 0x05, 0x20, 0xFB, 0x86, 0x20, 0xFE,
    0x3E, 0x01, 0xE0, 0x50
};
            return new List<byte>(bootROM);
        }
        public void Step()
        {
            DoNextOP();
            DoInterrupt();
            PPU.Do();
        }

    }
}