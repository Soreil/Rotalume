using System;

namespace emulator
{
    //Timer system handles all Gekkio timer tests except for tima_write_reloading and tma_write_reloading
    public class Timers
    {
        public ushort InternalCounter;
        private readonly Action EnableTimerInterrupt;
        public Timers(Action enableTimerInterrupt) => EnableTimerInterrupt = enableTimerInterrupt;

        public (Action<byte> Write, Func<byte> Read)[] HookUpTimers() => new (Action<byte> Write, Func<byte> Read)[] {
            ( x => DIV = x,
             () => DIV),

            ( x => TIMA = x,
             () => TIMA),

            ( x => TMA = x,
             () => TMA),

            ( x => TAC = x,
             () => TAC)
        };

        public void Tick()
        {
            if (DelayTicks > 0)
            {
                if (DelayTicks-- == 0)
                {
                    TIMA = TMA;
                }
            }

            if (TimerEnabled)
            {

                //Overflowing internalcounter shouldn't be an issue here.
                var overflow = ((InternalCounter + 1) & (1 << TACBitSelected)) == 0;
                if (overflow && (InternalCounter & (1 << TACBitSelected)) == 0 == false)
                {
                    IncrementTIMA();
                }
            }
            InternalCounter++;
        }

        public byte DIV
        {
            get => (byte)(InternalCounter >> 8);
            set
            {
                var overflow = (InternalCounter & (1 << TACBitSelected)) != 0;
                if (overflow)
                {
                    IncrementTIMA();
                }

                InternalCounter = 0;
            }
        }

        public byte TAC
        {
            get => (byte)(0xf8 | ((TimerEnabled ? 1 : 0) << 2) | PositionToBits(TACBitSelected));
            set
            {
                bool glitch;
                if (!TimerEnabled)
                {
                    glitch = false;
                }
                else
                {
                    glitch = !value.GetBit(2)
                        ? (InternalCounter & (1 << (TACBitSelected))) != 0
                        : ((InternalCounter & (1 << (TACBitSelected))) != 0) &&
                                 ((InternalCounter & (1 << (BitPosition(value)))) == 0);
                }
                if (glitch)
                {
                    IncrementTIMA();
                }

                TimerEnabled = value.GetBit(2);
                TACBitSelected = BitPosition(value);
            }
        }

        private bool TimerEnabled;
        private int TACBitSelected;

        private static byte BitPosition(byte b) => (b & 0x03) switch
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

        public byte TMA;

        public byte TIMA;

        //When TIMA overflows it should delay writing the value for 4 cycles
        private const int DelayDuration = 4;
        private uint DelayTicks = 0;
        private void IncrementTIMA()
        {
            if (TIMA == 0xff)
            {
                DelayTicks = DelayDuration;
                EnableTimerInterrupt();
            }
            TIMA++;
        }
    }
}
