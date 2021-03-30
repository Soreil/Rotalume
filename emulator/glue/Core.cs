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
        public readonly MMU Memory;
        private readonly Timers Timers;
        private readonly InterruptRegisters InterruptRegisters;
        private readonly ProgramCounter PC;

        //TODO: move serial in to it's own class when we implement it
        private byte serialControl = 0x7e;

        public Core(byte[] gameROM, byte[]? bootROM, Keypad Keypad, FrameSink frameSink)
        {
            if (gameROM.Length < 0x8000)
            {
                throw new Exception("Cartridge file has to be at least 8kb in size");
            }
            PC = new();

            InterruptRegisters = new InterruptRegisters();
            Keypad.Input.KeyWentDown += InterruptRegisters.TriggerEvent;

            APU = new APU(32768);
            PPU = new PPU(
            () =>
            {
                var IFR = InterruptRegisters.InterruptFireRegister;
                IFR.SetBit(0);
                InterruptRegisters.InterruptFireRegister = IFR;
            },
            () =>
            {
                var IFR = InterruptRegisters.InterruptFireRegister;
                IFR.SetBit(1);
                InterruptRegisters.InterruptFireRegister = IFR;
            },
            frameSink);

            Timers = new Timers(() =>
            {
                var IFR = InterruptRegisters.InterruptFireRegister;
                IFR.SetBit(2);
                InterruptRegisters.InterruptFireRegister = IFR;
            });

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

            Memory = new MMU(
    bootROM,
    Card,
    PPU.VRAM,
    PPU.OAM,
    ioRegisters,
    (x => InterruptRegisters.InterruptControlRegister = x,
    () => InterruptRegisters.InterruptControlRegister),
    PC
    );

            CPU = new CPU(Memory, InterruptRegisters, PC);
            ioRegisters[0x0f] = InterruptRegisters.HookUp();

            ioRegisters[0x50] = Memory.HookUpMemory();

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

                InterruptRegisters.InterruptFireRegister = 0xe1;

                //sound
                APU.NR10 = 0x80;
                APU.NR11 = 0xB0;
                APU.NR12 = 0xf3;
                APU.NR14 = 0xbf;
                APU.NR21 = 0x3f;
                APU.NR22 = 0x00;
                APU.NR23 = 0xff;
                APU.NR24 = 0xbf;
                APU.NR30 = 0x7f;
                APU.NR31 = 0xff;
                APU.NR32 = 0x9f;
                APU.NR33 = 0xff;
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

                PPU.SCY = 0;
                PPU.SCX = 0;
                PPU.WY = 0;
                PPU.WX = 0;
                PPU.LYC = 0;
                PPU.LY = 1;

                PPU.BGP = 0xfc;
                PPU.OBP0 = 0xff;
                PPU.OBP1 = 0xff;

                InterruptRegisters.IME = true;
            }
        }

        private (Action<byte> Write, Func<byte> Read)[] SetupControlRegisters(Keypad Keypad)
        {
            (Action<byte> Write, Func<byte> Read)[] controlRegisters = new (Action<byte> Write, Func<byte> Read)[0x80];
            for (int i = 0; i < controlRegisters.Length; i++)
                controlRegisters[i] = (x => { }, () => 0xff);

            //Keypad
            controlRegisters[0] = Keypad.HookUpKeypad();

            //Serial
            var SerialRegisters = new (Action<byte> Write, Func<byte> Read)[] {
            ((x) => { },
            () => 0),

            ((x) => serialControl = (byte)((x & 0x81) | 0x7e),
            () => serialControl),
            };

            SerialRegisters.CopyTo(controlRegisters, 1);

            //Timers
            var TimerRegisters = Timers.HookUpTimers();
            TimerRegisters.CopyTo(controlRegisters, 4);

            var SoundRegisters = APU.HookUpSound();
            SoundRegisters[0].CopyTo(controlRegisters, 0x10);
            SoundRegisters[1].CopyTo(controlRegisters, 0x16);
            SoundRegisters[2].CopyTo(controlRegisters, 0x20);
            SoundRegisters[3].CopyTo(controlRegisters, 0x30);

            var GraphicsRegisters = PPU.HookUpGraphics();
            GraphicsRegisters[0].CopyTo(controlRegisters, 0x40);
            GraphicsRegisters[1].CopyTo(controlRegisters, 0x47);

            //DMA
            controlRegisters[0x46] =
            ((x) =>
            {
                if (x > 0xf1)
                {
                    throw new Exception("Illegal DMA start adress"); //TODO: investigate how to handle these
                }
                else if (DMATicksLeft != 0)
                {
                    throw new Exception("Nested DMA call");
                }

                DMATicksLeft = 160;
                baseAddr = (ushort)(x << 8);
            },
            () => (byte)(baseAddr >> 8));


            return controlRegisters;
        }

        int DMATicksLeft = 0;
        ushort baseAddr = 0;

        //We have to make Step take one tick per subsystem
        public void Step()
        {
            masterclock++;
            Timers.Tick();
            CPU.Tick();
            APU.Tick();
            PPU.Tick();

            if (DMATicksLeft != 0)
            {
                var r = Memory.Read((ushort)(baseAddr + (160 - DMATicksLeft)));
                PPU.OAM[OAM.Start + (160 - DMATicksLeft)] = r;
                DMATicksLeft--;
            }
        }
    }
}