using Hardware;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace emulator
{
    public class Core : ISampleProvider
    {
        //Not sure where this special bit should go but it's not in memory and suposed to be hard to access
        private bool bootROMActive = true;

        //Global clock used by RTC carts
        private long masterclock = 0;

        public CPU CPU;
        public APU APU;
        public PPU PPU;
        public Timers Timers;

        //0x3 sets the top selection bits for buttons and dpad
        private byte keypadFlags = 0x30;

        private byte _dma = 0xff;
        private byte serialControl = 0x7e;
        private readonly ControlRegister interruptRegisters = new(0xffff, 0x1); //This is only being used for two registers.

        public Core(List<byte> l, byte[]? bootrom = null) : this(l.Count < 0x8000 ? PadAndMoveTo0x100(l.ToArray()) : l.ToArray(), bootrom, new(), () => false, new())
        { }

        private static byte[] PadAndMoveTo0x100(byte[] l)
        {
            byte[] buffer = new byte[0x8000];

            l.CopyTo(buffer, 0x100);
            return buffer;
        }

        private readonly ConcurrentDictionary<JoypadKey, bool> Pressed;
        private byte UpdateJoypadPresses(byte Flags)
        {
            var selectButtons = !Flags.GetBit(5);
            var selectArrows = !Flags.GetBit(4);

            byte joypad = 0xf;
            if (!selectButtons && !selectArrows)
            {
                return (byte)((joypad & 0xf) | 0xc0);
            }

            if (selectArrows)
            {
                if (Pressed[JoypadKey.Right])
                {
                    joypad = joypad.SetBit(0, false);
                }

                if (Pressed[JoypadKey.Left])
                {
                    joypad = joypad.SetBit(1, false);
                }

                if (Pressed[JoypadKey.Up])
                {
                    joypad = joypad.SetBit(2, false);
                }

                if (Pressed[JoypadKey.Down])
                {
                    joypad = joypad.SetBit(3, false);
                }
            }
            if (selectButtons)
            {

                if (Pressed[JoypadKey.B])
                {
                    joypad = joypad.SetBit(0, false);
                }

                if (Pressed[JoypadKey.A])
                {
                    joypad = joypad.SetBit(1, false);
                }

                if (Pressed[JoypadKey.Select])
                {
                    joypad = joypad.SetBit(2, false);
                }

                if (Pressed[JoypadKey.Start])
                {
                    joypad = joypad.SetBit(3, false);
                }
            }

            return (byte)((joypad & 0xf) | 0xc0);
        }


        public Core(byte[] gameROM, byte[]? bootROM, ConcurrentDictionary<JoypadKey, bool> Pressed, Func<bool> getKeyboardInterrupt, FrameSink frameSink)
        {
            this.Pressed = Pressed;

            APU = new APU(WaveFormat.SampleRate * 2);
            PPU = new PPU(() => CPU!.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(0),
                                       () => CPU!.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(1),
                                       frameSink);

            Timers = new Timers(() => CPU!.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(2));

            interruptRegisters.Writer[0x00] = x => CPU!.InterruptControlRegister = x;
            interruptRegisters.Reader[0x00] = () => CPU!.InterruptControlRegister;

            var ioRegisters = SetupControlRegisters();

            if (gameROM.Length < 0x8000)
            {
                throw new Exception();
            }

            CartHeader Header = new CartHeader(gameROM);

            MBC Card;
            if (Header is null)
            {
                Card = MakeFakeMBC(gameROM);
            }
            else
            {
                Card = MakeMBC(Header, gameROM, MakeMemoryMappedFile(Header));
            }

            var memory = new MMU(
    bootROM,
    () => bootROMActive,
    Card,
    PPU.VRAM,
    PPU.OAM,
    ioRegisters,
    interruptRegisters
    );

            CPU = new CPU(getKeyboardInterrupt, memory);
            memory.ReadInput = CPU.ReadOp;

            //We have to replicate the state of the system post boot without running the bootrom
            if (bootROM == null)
            {
                bootROMActive = false;

                //registers
                CPU.PC = 0x100;
                CPU.Registers.AF = 0x0100;
                CPU.Registers.BC = 0xff13;
                CPU.Registers.DE = 0x00c1;
                CPU.Registers.HL = 0x8403;
                CPU.Registers.SP = 0xfffe;

                //timers
                Timers.TIMA = 0;
                Timers.TAC = 0;
                Timers.TMA = 0;
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

        private static System.IO.MemoryMappedFiles.MemoryMappedFile? MakeMemoryMappedFile(CartHeader Header)
        {
            //A cartridge requires a battery in order to be able to keep state while the system is off
            if (!Header.HasBattery())
            {
                return null;
            }

            //This retrieves %appdata% path
            var root = Environment.GetEnvironmentVariable("AppData") + "\\rotalume";
            if (!System.IO.Directory.Exists(root))
            {
                System.IO.Directory.CreateDirectory(root);
            }

            //Filenames might be somewhat illegal depending on what characters are in the title?
            var path = string.Format(@"{0}\{1}.sav", root, Header.Title);
            if (!System.IO.File.Exists(path))
            {
                int size = 0;
                if (Header.RAM_Size != 0)
                {
                    size += Header.RAM_Size;
                }
                //MBC2 does not report a size in the header but instead has a fixed 2k internal RAM
                else if (Header.Type == CartType.MBC2_BATTERY)
                {
                    size += 0x2000;
                }
                //16 bytes to store clock should be plenty
                if (Header.HasClock())
                {
                    size += 16;
                }

                var buffer = new byte[size];
                for (int i = 0; i < size; i++)
                {
                    buffer[i] = 0xff;
                }

                System.IO.File.WriteAllBytes(path, buffer);
            }
            return System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(path);
        }

        private ControlRegister SetupControlRegisters()
        {
            void BootROMFlagController(byte b)
            {
                if (b == 1)
                {
                    bootROMActive = false;
                }
            }

            ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);

            controlRegisters.Writer[0] = x => keypadFlags = (byte)(x & 0xf0);
            controlRegisters.Reader[0] = () => UpdateJoypadPresses(keypadFlags);

            controlRegisters.Writer[0xF] = x => CPU.InterruptFireRegister = x;
            controlRegisters.Reader[0xF] = () => CPU.InterruptFireRegister;

            HookUpTimers(controlRegisters);

            controlRegisters.Writer[0x50] = BootROMFlagController;
            controlRegisters.Reader[0x50] = () => 0xff;

            void LCDControlController(byte b) => PPU.LCDC = b;

            byte ReadLCDControl() => PPU.LCDC;

            void LCDStatController(byte b) => PPU.STAT = (byte)((b & 0xf8) | (PPU.STAT & 0x7));

            byte ReadLCDStat() => PPU.STAT;

            void ScrollYController(byte b) => PPU.SCY = b;

            byte ReadScrollY() => PPU.SCY;

            void ScrollXController(byte b) => PPU.SCX = b;

            byte ReadScrollX() => PPU.SCX;

            void LCDLineController(byte b) => PPU.LY = b;

            byte ReadLine() => PPU.LY;

            void PaletteController(byte b) => PPU.BGP = b;

            byte ReadPalette() => PPU.BGP;

            void OBP0Controller(byte b) => PPU.OBP0 = b;

            byte ReadOBP0() => PPU.OBP0;

            void OBP1Controller(byte b) => PPU.OBP1 = b;

            byte ReadOBP1() => PPU.OBP1;

            void WYController(byte b) => PPU.WY = b;

            byte ReadWY() => PPU.WY;

            void WXController(byte b) => PPU.WX = b;

            byte ReadWX() => PPU.WX;

            void LYCController(byte b) => PPU.LYC = b;

            byte ReadLYC() => PPU.LYC;

            HookUpGraphics(controlRegisters, LCDControlController, ReadLCDControl, LCDStatController, ReadLCDStat, ScrollYController, ReadScrollY, ScrollXController, ReadScrollX, LCDLineController, ReadLine, PaletteController, ReadPalette, OBP0Controller, ReadOBP0, OBP1Controller, ReadOBP1, WYController, ReadWY, WXController, ReadWX, LYCController, ReadLYC);

            HookUpSound(controlRegisters);

            //Serial
            controlRegisters.Writer[1] = (x) => { };
            controlRegisters.Reader[1] = () => 0;

            controlRegisters.Writer[2] = (x) => serialControl = (byte)((x & 0x81) | 0x7e);
            controlRegisters.Reader[2] = () => serialControl;

            //Not used on the DMG
            for (ushort Unused = 0xff4c; Unused < 0xff80; Unused++)
            {
                if (Unused == 0xff50)
                {
                    continue;
                }

                controlRegisters.Writer[Unused & 0xff] = (x) => { };
                controlRegisters.Reader[Unused & 0xff] = () => 0xff;
            }
            return controlRegisters;
        }

        private void HookUpTimers(ControlRegister controlRegisters)
        {
            controlRegisters.Writer[0x04] = x => Timers.DIV = x;
            controlRegisters.Reader[0x04] = () => Timers.DIV;

            controlRegisters.Writer[0x05] = x => Timers.TIMA = x;
            controlRegisters.Reader[0x05] = () => Timers.TIMA;

            controlRegisters.Writer[0x06] = x => Timers.TMA = x;
            controlRegisters.Reader[0x06] = () => Timers.TMA;

            controlRegisters.Writer[0x07] = x => Timers.TAC = x;
            controlRegisters.Reader[0x07] = () => Timers.TAC;
        }

        private void HookUpGraphics(ControlRegister controlRegisters, Action<byte> LCDControlController, Func<byte> ReadLCDControl, Action<byte> LCDStatController, Func<byte> ReadLCDStat, Action<byte> ScrollYController, Func<byte> ReadScrollY, Action<byte> ScrollXController, Func<byte> ReadScrollX, Action<byte> LCDLineController, Func<byte> ReadLine, Action<byte> PaletteController, Func<byte> ReadPalette, Action<byte> OBP0Controller, Func<byte> ReadOBP0, Action<byte> OBP1Controller, Func<byte> ReadOBP1, Action<byte> WYController, Func<byte> ReadWY, Action<byte> WXController, Func<byte> ReadWX, Action<byte> LYCController, Func<byte> ReadLYC)
        {
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
                if (x > 0xf1)
                {
                    throw new Exception("Illegal DMA start adress"); //TODO: investigate how to handle these
                }

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
        }

        private void HookUpSound(ControlRegister controlRegisters)
        {
            //Sound
            controlRegisters.Writer[0x10] = (x) => APU.NR10 = x;
            controlRegisters.Reader[0x10] = () => APU.NR10;
            controlRegisters.Writer[0x11] = (x) => APU.NR11 = x;
            controlRegisters.Reader[0x11] = () => APU.NR11;
            controlRegisters.Writer[0x12] = (x) => APU.NR12 = x;
            controlRegisters.Reader[0x12] = () => APU.NR12;
            controlRegisters.Writer[0x13] = (x) => APU.NR13 = x;
            controlRegisters.Reader[0x13] = () => APU.NR13;
            controlRegisters.Writer[0x14] = (x) => APU.NR14 = x;
            controlRegisters.Reader[0x14] = () => APU.NR14;
            controlRegisters.Writer[0x16] = (x) => APU.NR21 = x;
            controlRegisters.Reader[0x16] = () => APU.NR21;
            controlRegisters.Writer[0x17] = (x) => APU.NR22 = x;
            controlRegisters.Reader[0x17] = () => APU.NR22;
            controlRegisters.Writer[0x18] = (x) => APU.NR23 = x;
            controlRegisters.Reader[0x18] = () => APU.NR23;
            controlRegisters.Writer[0x19] = (x) => APU.NR24 = x;
            controlRegisters.Reader[0x19] = () => APU.NR24;
            controlRegisters.Writer[0x1a] = (x) => APU.NR30 = x;
            controlRegisters.Reader[0x1a] = () => APU.NR30;
            controlRegisters.Writer[0x1b] = (x) => APU.NR31 = x;
            controlRegisters.Reader[0x1b] = () => APU.NR31;
            controlRegisters.Writer[0x1c] = (x) => APU.NR32 = x;
            controlRegisters.Reader[0x1c] = () => APU.NR32;
            controlRegisters.Writer[0x1d] = (x) => APU.NR33 = x;
            controlRegisters.Reader[0x1d] = () => APU.NR33;
            controlRegisters.Writer[0x1e] = (x) => APU.NR34 = x;
            controlRegisters.Reader[0x1e] = () => APU.NR34;
            controlRegisters.Writer[0x20] = (x) => APU.NR41 = x;
            controlRegisters.Reader[0x20] = () => APU.NR41;
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

            controlRegisters.Writer[0x30] = (x) => APU.Wave[0] = x;
            controlRegisters.Reader[0x30] = () => APU.Wave[0];
            controlRegisters.Writer[0x31] = (x) => APU.Wave[1] = x;
            controlRegisters.Reader[0x31] = () => APU.Wave[1];
            controlRegisters.Writer[0x32] = (x) => APU.Wave[2] = x;
            controlRegisters.Reader[0x32] = () => APU.Wave[2];
            controlRegisters.Writer[0x33] = (x) => APU.Wave[3] = x;
            controlRegisters.Reader[0x33] = () => APU.Wave[3];
            controlRegisters.Writer[0x34] = (x) => APU.Wave[4] = x;
            controlRegisters.Reader[0x34] = () => APU.Wave[4];
            controlRegisters.Writer[0x35] = (x) => APU.Wave[5] = x;
            controlRegisters.Reader[0x35] = () => APU.Wave[5];
            controlRegisters.Writer[0x36] = (x) => APU.Wave[6] = x;
            controlRegisters.Reader[0x36] = () => APU.Wave[6];
            controlRegisters.Writer[0x37] = (x) => APU.Wave[7] = x;
            controlRegisters.Reader[0x37] = () => APU.Wave[7];
            controlRegisters.Writer[0x38] = (x) => APU.Wave[8] = x;
            controlRegisters.Reader[0x38] = () => APU.Wave[8];
            controlRegisters.Writer[0x39] = (x) => APU.Wave[9] = x;
            controlRegisters.Reader[0x39] = () => APU.Wave[9];
            controlRegisters.Writer[0x3a] = (x) => APU.Wave[10] = x;
            controlRegisters.Reader[0x3a] = () => APU.Wave[10];
            controlRegisters.Writer[0x3b] = (x) => APU.Wave[11] = x;
            controlRegisters.Reader[0x3b] = () => APU.Wave[11];
            controlRegisters.Writer[0x3c] = (x) => APU.Wave[12] = x;
            controlRegisters.Reader[0x3c] = () => APU.Wave[12];
            controlRegisters.Writer[0x3d] = (x) => APU.Wave[13] = x;
            controlRegisters.Reader[0x3d] = () => APU.Wave[13];
            controlRegisters.Writer[0x3e] = (x) => APU.Wave[14] = x;
            controlRegisters.Reader[0x3e] = () => APU.Wave[14];
            controlRegisters.Writer[0x3f] = (x) => APU.Wave[15] = x;
            controlRegisters.Reader[0x3f] = () => APU.Wave[15];
        }

        private static MBC MakeFakeMBC(byte[] gameROM) => new ROMONLY(gameROM);

        private MBC MakeMBC(CartHeader header, byte[] gameROM, System.IO.MemoryMappedFiles.MemoryMappedFile? file) => header.Type switch
        {
            CartType.ROM_ONLY => new ROMONLY(gameROM),
            CartType.MBC1 => new MBC1(header, gameROM),
            CartType.MBC1_RAM => new MBC1(header, gameROM),
            CartType.MBC1_RAM_BATTERY => new MBC1(header, gameROM, file),
            CartType.MBC2 => new MBC2(gameROM, file!),
            CartType.MBC2_BATTERY => new MBC2(gameROM, file!),
            CartType.MBC3 => new MBC3(header, gameROM, file!),
            CartType.MBC3_RAM => new MBC3(header, gameROM, file!),
            CartType.MBC3_RAM_BATTERY => new MBC3(header, gameROM, file!),
            CartType.MBC3_TIMER_BATTERY => new MBC3(header, gameROM, file!, () => masterclock),
            CartType.MBC3_TIMER_RAM_BATTERY => new MBC3(header, gameROM, file!, () => masterclock),
            CartType.MBC5 => new MBC5(header, gameROM, file!),
            CartType.MBC5_RAM => new MBC5(header, gameROM, file!),
            CartType.MBC5_RAM_BATTERY => new MBC5(header, gameROM, file!),
            _ => throw new NotImplementedException(),
        };


        public static byte[] LoadBootROM() => System.IO.File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin");

        //We have to make Step take one tick per subsystem
        public void Step()
        {
            masterclock++;
            Timers.Tick();
            CPU.Tick();
            APU.Tick();
            PPU.Tick();
        }

        //Gusboy uses this frequency because it aligns well with the gameboy clock
        public WaveFormat WaveFormat { get; init; } = WaveFormat.CreateIeeeFloatWaveFormat(32768, 2);

        private readonly SignalGenerator wave = new()
        {
            Gain = 0.1,
            Frequency = 500,
            Type = SignalGeneratorType.Sin
        };

        public int Read(float[] buffer, int offset, int count)
        {
            while (APU.SampleCount < count)
            {
                Step();
            }

            APU.SampleCount -= count;

            return wave.Read(buffer, offset, count);
        }
    }
}