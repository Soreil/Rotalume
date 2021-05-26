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
            InternalCounter++;
            if (TACEnable)
            {

                //Overflowing internalcounter shouldn't be an issue here.
                var overflow = ((InternalCounter) & TACFrequency) == 0;
                if (overflow && ((InternalCounter - 1) & TACFrequency) != 0)
                {
                    IncrementTIMA();
                }
            }
            if (DelayTicks > 0)
            {
                if (DelayTicks-- == 0)
                {
                    TIMA = TMA;
                }
            }

            //There are a number of different timers in the APU which derive from
            //bits in the DIV register rising/falling.
            //https://github.com/LIJI32/SameBoy/Core/timing.c handles these cases for reference
            //in GB_set_internal_div_counter.
            //A rising edge at bit 14,13 is handled here.
            if ((InternalCounter & 0x7FFF) == 0x4000)
            {
                OnAPUTick128Hz();
            }

        }

        private void OnAPUTick128Hz() => APUTick128Hz?.Invoke(this, EventArgs.Empty);

        public event EventHandler? APUTick128Hz;

        private byte DIV
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

        private byte TAC
        {
            get => (byte)(0xf8 | ((TACEnable ? 1 : 0) << 2) | PositionToBits(TACFrequency));
            set
            {
                var glitch = TACEnable
&& (!value.GetBit(2)
                        ? (InternalCounter & TACFrequency) != 0
                        : ((InternalCounter & TACFrequency) != 0) &&
                                 ((InternalCounter & (BitPosition(value))) == 0));
                if (glitch)
                {
                    IncrementTIMA();
                }

                TACEnable = value.GetBit(2);
                TACFrequency = BitPosition(value);
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

        private byte TMA;

        private byte TIMA;

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
