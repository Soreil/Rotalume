﻿using System;
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

            PPU = new PPU(() => Clock, () => InterruptFireRegister = InterruptFireRegister.SetBit(0),
                                       () => InterruptFireRegister = InterruptFireRegister.SetBit(1));

            Timers = new Timers(() => InterruptFireRegister = InterruptFireRegister.SetBit(2));

            Func<byte> ReadBootROMFlag = () => bootROMField;
            GetKeyboardInterrupt = getKeyboardInterrupt;

            interruptRegisters.Writer[0x00] = x => InterruptControlRegister = x;
            interruptRegisters.Reader[0x00] = () => InterruptControlRegister;

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
                Timers.DividerClock = 0xabcc;

                bootROMActive = false;
                PC = 0x100;
                CPU.Registers.AF = 0x01b0;
                CPU.Registers.BC = 0x0013;
                CPU.Registers.DE = 0x00d8;
                CPU.Registers.HL = 0x014d;
                CPU.Registers.SP = 0xfffe;

                Timers.Timer = 0;
                Timers.TimerControl = 0;
                Timers.TimerDefault = 0;

                PPU.LCDC = 0x91;

                PPU.SCY = 0;
                PPU.SCX = 0;
                PPU.LYC = 0;

                PPU.BGP = 0xfc;
                PPU.OBP0 = 0xff;
                PPU.OBP1 = 0xff;

                PPU.SCY = 0;
                PPU.SCX = 0;

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


            controlRegisters.Writer[0] = x => keypadFlags = x;
            controlRegisters.Reader[0] = () => GetJoyPad(keypadFlags);

            controlRegisters.Writer[0xF] = x => InterruptFireRegister = x;
            controlRegisters.Reader[0xF] = () => InterruptFireRegister;

            controlRegisters.Writer[0x04] = x => Timers.Divider = x;
            controlRegisters.Reader[0x04] = () => Timers.Divider;

            controlRegisters.Writer[0x05] = x => Timers.Timer = x;
            controlRegisters.Reader[0x05] = () => Timers.Timer;

            controlRegisters.Writer[0x06] = x => Timers.TimerDefault = x;
            controlRegisters.Reader[0x06] = () => Timers.TimerDefault;

            controlRegisters.Writer[0x07] = x => Timers.TimerControl = x;
            controlRegisters.Reader[0x07] = () => Timers.TimerControl;

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

                ushort baseAddr = (ushort)(x << 8);
                for (int i = 0; i < OAM.Size; i++)
                {
                    var r = CPU.Memory.Read((ushort)(baseAddr + i));
                    PPU.OAM[OAM.Start + i] = r;
                }
            };

            controlRegisters.Reader[0x46] = () => throw new Exception("DMAREAD");

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

            for (ushort SoundRegister = 0xff10; SoundRegister <= 0xff26; SoundRegister++)
            {
                controlRegisters.Writer[SoundRegister & 0xff] = (x) => { };
                controlRegisters.Reader[SoundRegister & 0xff] = () => 0xff;
            }

            for (ushort SoundWave = 0xff30; SoundWave <= 0xff3f; SoundWave++)
            {
                controlRegisters.Writer[SoundWave & 0xff] = (x) => { };
                controlRegisters.Reader[SoundWave & 0xff] = () => 0xff;
            }

            for (ushort Serial = 0xff01; Serial <= 0xff02; Serial++)
            {
                controlRegisters.Writer[Serial & 0xff] = (x) => { };
                controlRegisters.Reader[Serial & 0xff] = () => 0xff;
            }
            for (ushort Unused = 0xff4c; Unused < 0xff80; Unused++)
            {
                if (Unused == 0xff50) continue;
                controlRegisters.Writer[Unused & 0xff] = (x) => { };
                controlRegisters.Reader[Unused & 0xff] = () => 0xff;
            }
            return controlRegisters;
        }

        private static MBC MakeFakeMBC(byte[] gameROM) => new ROMONLY(gameROM);

        private static MBC MakeMBC(CartHeader header, byte[] gameROM) => header.Type switch
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

            //if (PC == 0x904) System.Diagnostics.Debugger.Break();

            var op = Read();
            if (op != 0xcb)
            {
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

        public static byte[] LoadBootROM()
        {
            return System.IO.File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin");
        }

        public void Step()
        {
            DoNextOP();
            //We really should have the GUI thread somehow do this logic but polling like this should work
            if (!InterruptFireRegister.GetBit(4) && GetKeyboardInterrupt())
                InterruptFireRegister = InterruptFireRegister.SetBit(4);

            DoInterrupt();
            if (CPU.InterruptEnableSceduled)
            {
                CPU.IME = true;
                CPU.InterruptEnableSceduled = false;
            }
            PPU.Do();
        }

    }
}