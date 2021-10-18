namespace emulator;

public class Core
{
    //Global clock used by RTC carts
    public long masterclock;

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
        PPU = new PPU(InterruptRegisters.EnableVBlankInterrupt, InterruptRegisters.EnableLCDSTATInterrupt, frameSink);

        Timers = new Timers(InterruptRegisters.EnableTimerInterrupt);

        var ioRegisters = SetupControlRegisters(Keypad);

        CartHeader Header = new CartHeader(gameROM);

        MBC Card;
        if (Header.HasBattery())
        {
            var mmf = Header.MakeMemoryMappedFile();
            Card = Header.HasClock() ? Header.MakeMBC(gameROM, mmf, () => masterclock) : Header.MakeMBC(gameROM, mmf);
        }
        else
        {
            Card = Header.MakeMBC(gameROM);
        }

        //Writing out the RTC too often would be very heavy. This writes it out once per frame.
        //
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
            CPU.SetStateWithoutBootrom();

            //timers
            Timers.SetStateWithoutBootrom();


            //sound
            APU.SetStateWithoutBootrom();

            //graphics TODO: we can't really set up the graphics environment correctly
            //because we will have to also initialize the internal renderer state correctly
            PPU.SetStateWithoutBootrom();


            InterruptRegisters.SetStateWithoutBootrom();
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

    int DMATicksLeft;
    ushort baseAddr;

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
