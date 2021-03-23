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

            if (TACEnable)
            {

                //Overflowing internalcounter shouldn't be an issue here.
                var overflow = ((InternalCounter + 1) & TACFrequency) == 0;
                if (overflow && (InternalCounter & TACFrequency) != 0)
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
                var overflow = (InternalCounter & TACFrequency) != 0;
                if (overflow)
                {
                    IncrementTIMA();
                }

                InternalCounter = 0;
            }
        }

        public byte TAC
        {
            get => (byte)(0xf8 | ((TACEnable ? 1 : 0) << 2) | PositionToBits(TACFrequency));
            set
            {
                bool glitch;
                if (!TACEnable)
                {
                    glitch = false;
                }
                else
                {
                    glitch = !value.GetBit(2)
                        ? (InternalCounter & TACFrequency) != 0
                        : ((InternalCounter & TACFrequency) != 0) &&
                                 ((InternalCounter & (BitPosition(value))) == 0);
                }
                if (glitch)
                {
                    IncrementTIMA();
                }

                TACEnable = value.GetBit(2);
                TACFrequency = BitPosition(value);
            }
        }

        private bool TACEnable;
        private int TACFrequency;

        private static int BitPosition(byte b) => (b & 0x03) switch
        {
            0 => 1 << 9,
            1 => 1 << 3,
            2 => 1 << 5,
            3 => 1 << 7,
            _ => throw new NotImplementedException(),
        };
        private static byte PositionToBits(int p) => p switch
        {
            1 << 9 => 0,
            1 << 3 => 1,
            1 << 5 => 2,
            1 << 7 => 3,
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
