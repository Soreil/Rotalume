using System;

namespace emulator
{
    public class Core
    {
        //Global clock used by RTC carts
        public long masterclock = 0;

        public readonly CPU CPU;
        private readonly APU APU;
        private readonly PPU PPU;
        private readonly Timers Timers;

        //TODO: move DMA somewhere else, probably the PPU
        private byte _dma = 0xff;
        //TODO: move serial in to it's own class when we implement it
        private byte serialControl = 0x7e;

        public Core(byte[] gameROM, byte[]? bootROM, Keypad Keypad, Func<bool> getKeyboardInterrupt, FrameSink frameSink)
        {
            if (gameROM.Length < 0x8000)
            {
                throw new Exception("Cartridge file has to be at least 8kb in size");
            }

            APU = new APU(32768);
            PPU = new PPU(() => CPU!.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(0),
                                       () => CPU!.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(1),
                                       frameSink);

            Timers = new Timers(() => CPU!.InterruptFireRegister = CPU.InterruptFireRegister.SetBit(2));

            ControlRegister interruptRegisters = new(0xffff, 0x1);
            interruptRegisters.Writer[0x00] = x => CPU!.InterruptControlRegister = x;
            interruptRegisters.Reader[0x00] = () => CPU!.InterruptControlRegister;

            var ioRegisters = SetupControlRegisters(Keypad);

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

            if (Card is MBC5WithRumble rumble)
            {
                rumble.RumbleStateChange += Keypad.ToggleRumble;
            }

            var memory = new MMU(
    bootROM,
    Card,
    PPU.VRAM,
    PPU.OAM,
    ioRegisters,
    interruptRegisters
    );

            CPU = new CPU(getKeyboardInterrupt, memory);
            CPU.HookUpCPU(ioRegisters);

            memory.ReadInput = CPU.ReadOp;
            memory.HookUpMemory(ioRegisters);

            //We have to replicate the state of the system post boot without running the bootrom
            if (bootROM == null)
            {
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

        private ControlRegister SetupControlRegisters(Keypad Keypad)
        {
            ControlRegister controlRegisters = new ControlRegister(0xff00, 0x80);

            //Serial
            controlRegisters.Writer[1] = (x) => { };
            controlRegisters.Reader[1] = () => 0;

            controlRegisters.Writer[2] = (x) => serialControl = (byte)((x & 0x81) | 0x7e);
            controlRegisters.Reader[2] = () => serialControl;

            //DMA
            controlRegisters.Reader[0x46] = () => _dma;
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
    }
}