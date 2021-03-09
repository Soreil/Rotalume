using Hardware;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using System;
using System.Collections.Concurrent;

namespace emulator
{
    public class Core : ISampleProvider
    {
        //Not sure where this special bit should go but it's not in memory and suposed to be hard to access
        private bool bootROMActive = true;

        //Global clock used by RTC carts
        private long masterclock = 0;

        private readonly CPU CPU;
        private readonly APU APU;
        private readonly PPU PPU;
        private readonly Timers Timers;
        private readonly Keypad Keypad;

        private byte _dma = 0xff;
        private byte serialControl = 0x7e;
        private readonly ControlRegister interruptRegisters = new(0xffff, 0x1); //This is only being used for two registers.

        public Core(byte[] gameROM, byte[]? bootROM, ConcurrentDictionary<JoypadKey, bool> Pressed, Func<bool> getKeyboardInterrupt, FrameSink frameSink)
        {
            Keypad = new(Pressed);
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
                throw new Exception("Cartridge file has to be at least 8kb in size");
            }

            CartHeader Header = new CartHeader(gameROM);

            MBC Card;
            var mmf = Header.MakeMemoryMappedFile();
            Card = Header.MakeMBC(gameROM, mmf, () => masterclock);

            if (Header.Type == CartType.MBC3_TIMER_RAM_BATTERY)
            {
                var SaveRTC = ((MBC3)Card).SaveRTC();

                void h(object? x, EventArgs y) => SaveRTC();
                frameSink.FramePushed += h;
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
            CPU.HookUpCPU(ioRegisters);

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

            //Serial
            controlRegisters.Writer[1] = (x) => { };
            controlRegisters.Reader[1] = () => 0;

            controlRegisters.Writer[2] = (x) => serialControl = (byte)((x & 0x81) | 0x7e);
            controlRegisters.Reader[2] = () => serialControl;

            controlRegisters.Writer[0x50] = BootROMFlagController;
            controlRegisters.Reader[0x50] = () => 0xff;

            controlRegisters.Reader[0x46] = () => _dma;

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

            Keypad.HookUpKeypad(controlRegisters);

            Timers.HookUpTimers(controlRegisters);

            PPU.HookUpGraphics(controlRegisters);

            APU.HookUpSound(controlRegisters);

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