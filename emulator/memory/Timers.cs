namespace emulator;

//Timer system handles all Gekkio timer tests except for tima_write_reloading and tma_write_reloading
public class Timers
{
    public ushort InternalCounter;

    //The previous value has a NOT gate in front of it so we want to set it to true initally
    private bool fallingEdgePrevious = true;

    public Timers(sound.APU apu, InterruptRegisters interruptRegisters)
    {
        APUTick512Hz += apu.FrameSequencerClock;
        Interrupt += interruptRegisters.EnableTimerInterrupt;
    }

    public void Tick(object? o, EventArgs e)
    {
        var oldInternalCounter = InternalCounter;
        InternalCounter++;


        var overflow = IsSet(InternalCounter, TACSelectedBit);

        var valueForFallingEdgeDetector = overflow && TACEnable;

        //falling edge detector
        if ((!valueForFallingEdgeDetector) && fallingEdgePrevious)
        {
            IncrementTIMA();
        }

        fallingEdgePrevious = valueForFallingEdgeDetector;

        if (IgnoreTIMAWriteTicks > 0)
        {
            IgnoreTIMAWriteTicks--;
        }

        if (DelayTicks > 0)
        {
            DelayTicks--;
            if (DelayTicks == 0)
            {
                //We are in the [B] M-cycle according to TCAGBD here
                TIMA = TMA;
                IgnoreTIMAWriteTicks = 4;
                Interrupt?.Invoke(this, EventArgs.Empty);
            }
        }

        //We want to tick when bit 5 of the top part turns to 1
        //TODO: check how to do this correctly
        var topHalfNew = (byte)(InternalCounter >> 8);
        var topHalfOld = (byte)(oldInternalCounter >> 8);
        if (topHalfNew.GetBit(4) && !topHalfOld.GetBit(4))
        {
            OnAPUTick512z();
        }

    }

    private static bool IsSet(ushort internalCounter, int tACSelectedBit) => (internalCounter & (1 << tACSelectedBit)) != 0;
    private void OnAPUTick512z() => APUTick512Hz?.Invoke(this, EventArgs.Empty);

    private event EventHandler? APUTick512Hz;

    private event EventHandler? Interrupt;
    private byte DIV
    {
        get => (byte)(InternalCounter >> 8);
        set
        {
            InternalCounter = 0;
        }
    }

    private byte TAC
    {
        get => (byte)(0xf8 | (Convert.ToInt32(TACEnable) << 2) | PositionToBits(TACSelectedBit));
        set
        {
            TACEnable = value.GetBit(2);
            TACSelectedBit = BitPosition(value);
        }
    }

    internal void SetStateWithoutBootrom()
    {
        TIMA = 0;
        TAC = 0;
        TMA = 0;
        InternalCounter = 0x1800;
    }

    private bool TACEnable;
    private int TACSelectedBit;

    private static int BitPosition(byte b) => (b & 0x03) switch
    {
        0 => 9,
        1 => 3,
        2 => 5,
        3 => 7,
        _ => throw new NotImplementedException(),
    };
    private static byte PositionToBits(int p) => p switch
    {
        9 => 0,
        3 => 1,
        5 => 2,
        7 => 3,
        _ => throw new NotImplementedException(),
    };

    private byte tma;
    private byte TMA
    {
        get => tma;
        set
        {
            if (IgnoreTIMAWriteTicks != 0)
            {
                tima = value;
            }
            tma = value;
        }
    }

    private byte tima;
    private byte TIMA
    {
        get => tima;
        set
        {
            if (IgnoreTIMAWriteTicks != 0) return;

            tima = value;
            //When writing a value to TIMA in case [A] it prevents the interrupt handling and reload from TMA
            DelayTicks = 0;
        }
    }

    //When TIMA overflows it should delay writing the value for 4 cycles
    private const int DelayDuration = 4;
    private uint DelayTicks;
    private uint IgnoreTIMAWriteTicks;
    private void IncrementTIMA()
    {
        if (TIMA == 0xff)
        {
            TIMA = 0;
            DelayTicks = DelayDuration;
            return;
        }
        TIMA++;
    }

    public byte this[int n]
    {
        get => n switch
        {
            0xff04 => DIV,
            0xff05 => TIMA,
            0xff06 => TMA,
            0xff07 => TAC,
            _ => throw new Exception("Not a timer register buddy")
        };


        set
        {
            switch (n)
            {
                case 0xff04:
                DIV = value;
                break;
                case 0xff05:
                TIMA = value;
                break;
                case 0xff06:
                TMA = value;
                break;
                case 0xff07:
                TAC = value;
                break;
                default:
                throw new Exception("Not a timer m8");
            }
        }
    }
}
