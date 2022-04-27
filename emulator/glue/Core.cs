using emulator.sound;

namespace emulator;

public class Core : IDisposable
{
    //Global clock used by RTC carts
    public long masterclock;

    public readonly CPU CPU;
    private readonly APU APU;
    private readonly PPU PPU;
    public readonly MMU Memory;
    private readonly Timers Timers;
    private readonly InterruptRegisters InterruptRegisters;

    //TODO: move serial in to it's own class when we implement it
    private byte serialControl = 0x7e;

    public Core(byte[] gameROM, byte[]? bootROM, string fileName, Keypad Keypad, IFrameSink frameSink)
    {
        if (gameROM.Length < 0x8000)
        {
            throw new CartridgeTooSmallException("Cartridge file has to be at least 8kb in size");
        }

        InterruptRegisters = new InterruptRegisters();
        Keypad.Input.KeyWentDown += InterruptRegisters.TriggerEvent;

        APU = new APU();
        PPU = new PPU(frameSink);

        PPU.VBlankInterrupt += InterruptRegisters.EnableVBlankInterrupt;
        PPU.STATInterrupt += InterruptRegisters.EnableLCDSTATInterrupt;

        Timers = new Timers();
        Timers.Interrupt += InterruptRegisters.EnableTimerInterrupt;
        Timers.APUTick512Hz += APU.FrameSequencerClock;

        var ioRegisters = SetupControlRegisters(Keypad);

        var Header = new CartHeader(gameROM);

        MBC Card;
        if (Header.HasBattery())
        {
            var mmf = Header.MakeMemoryMappedFile(fileName);
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
APU,
ioRegisters,
(x => InterruptRegisters.InterruptControlRegister = x,
() => InterruptRegisters.InterruptControlRegister)
);

        CPU = new CPU(Memory, InterruptRegisters);

        CPU.OAMCorruption += PPU.OAM.Corrupt;

        ioRegisters[0xff0f] = InterruptRegisters.HookUp();

        ioRegisters[0xff50] = Memory.HookUpMemory();

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

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA;
    }

    private void DMA(object? o, EventArgs e)
    {
        if (DMATicksLeft != 0)
        {
            //DMA values greater than or equal to A000 always go to the external RAM
            var r = baseAddr < 0xa000
            ? Memory[(ushort)(baseAddr + (160 - DMATicksLeft))]
            : Memory.ExternalBusRAM((ushort)(baseAddr + (160 - DMATicksLeft)));

            PPU.OAM[OAM.Start + (160 - DMATicksLeft)] = r;
            DMATicksLeft--;
        }
    }

    public int DMATicksLeft;
    public ushort baseAddr;
    private bool disposedValue;

    private Dictionary<ushort, (Action<byte> Write, Func<byte> Read)> SetupControlRegisters(Keypad Keypad)
    {
        var controlRegisters = new Dictionary<ushort, (Action<byte> Write, Func<byte> Read)>
        {
            //Keypad
            { 0xff00, Keypad.HookUpKeypad() }
        };

        //Serial
        var SerialRegisters = new (Action<byte> Write, Func<byte> Read)[] {
            ((x) => { },
            () => 0),

            ((x) => serialControl = (byte)((x & 0x81) | 0x7e),
            () => serialControl),
            };

        controlRegisters.Add(0xff01, SerialRegisters[0]);
        controlRegisters.Add(0xff02, SerialRegisters[1]);

        //Timers
        var TimerRegisters = Timers.HookUpTimers();
        controlRegisters.Add(0xff04, TimerRegisters[0]);
        controlRegisters.Add(0xff05, TimerRegisters[1]);
        controlRegisters.Add(0xff06, TimerRegisters[2]);
        controlRegisters.Add(0xff07, TimerRegisters[3]);


        var GraphicsRegisters = PPU.HookUpGraphics();
        controlRegisters.Add(0xff40, GraphicsRegisters[0]);
        controlRegisters.Add(0xff41, GraphicsRegisters[1]);
        controlRegisters.Add(0xff42, GraphicsRegisters[2]);
        controlRegisters.Add(0xff43, GraphicsRegisters[3]);
        controlRegisters.Add(0xff44, GraphicsRegisters[4]);
        controlRegisters.Add(0xff45, GraphicsRegisters[5]);
        controlRegisters.Add(0xff47, GraphicsRegisters[6]);
        controlRegisters.Add(0xff48, GraphicsRegisters[7]);
        controlRegisters.Add(0xff49, GraphicsRegisters[8]);
        controlRegisters.Add(0xff4a, GraphicsRegisters[9]);
        controlRegisters.Add(0xff4b, GraphicsRegisters[10]);

        //DMA
        (Action<byte> Write, Func<byte> Read) dmaController =
        ((x) =>
        {
            DMATicksLeft = 160;
            baseAddr = (ushort)(x << 8);
        },
        () => (byte)(baseAddr >> 8));

        controlRegisters[0xff46] = dmaController;

        //Set all unused IO registers to just read back 0xff
        for (int i = 0; i < 0x80; i++)
        {
            var addr = (ushort)(0xff00 + i);
            if (!controlRegisters.ContainsKey(addr))
                controlRegisters.Add(addr, (x => { }, () => 0xff));
        }

        return controlRegisters;
    }

    public void Step()
    {
        masterclock += 4;
        CPU.Step();
    }

    public (short left, short right) Sample() => APU.Sample();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Memory.Dispose();
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Core()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
