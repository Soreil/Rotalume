using System;
using System.Collections.Generic;

using generator;

namespace Tests
{
    public class BootBase
    {
        public ushort PC;

        public Func<ushort> GetProgramCounter;
        public Action<ushort> SetProgramCounter;

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

        public int Clock;
        public Action<int> IncrementClock;

        public Func<byte> Read;

        public Decoder dec;

        public PPU PPU;
        readonly ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);

        public BootBase(List<byte> l) : this(new List<byte>(), l)
        {
            bootROMActive = false;
        }
        public BootBase() : this(LoadBootROM(), new List<byte>())
        {
        }
        public BootBase(List<byte> bootROM, List<byte> gameROM)
        {
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

            controlRegisters.Writer[0x50] += BootROMFlagController;
            controlRegisters.Reader[0x50] += ReadBootROMFlag;

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

            GetProgramCounter = () => PC;
            SetProgramCounter = (x) => { PC = x; };
            IncrementClock = (x) => { Clock += x; };
            Read = () => dec.Storage[PC++];
            var decoder = new Decoder(Read, bootROM, gameROM, GetProgramCounter, SetProgramCounter, IncrementClock, () => bootROMActive);

            dec = decoder;
            PPU = new PPU(() => Clock);

            dec.Storage.setRanges.Add(new MMU.SetRange(controlRegisters.Start, controlRegisters.Start + controlRegisters.Size, controlRegisters.ContainsWriter, (x, v) => controlRegisters[x] = v));
            dec.Storage.getRanges.Add(new MMU.GetRange(controlRegisters.Start, controlRegisters.Start + controlRegisters.Size, controlRegisters.ContainsReader, (x) => controlRegisters[x]));
        }

        internal void DoPPU() => PPU.Do();

        public void DoNextOP()
        {
            var op = Read();
            if (op != 0xcb)
            {
                dec.Op((Unprefixed)op)();
            }
            else
            {
                var CBop = Read();
                dec.Op((Cbprefixed)CBop)();
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
    }
}