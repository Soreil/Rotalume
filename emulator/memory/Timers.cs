using System;

namespace emulator
{
    public class Timers
    {
        public ushort InternalCounter;

        readonly Action EnableTimerInterrupt;
        public Timers(Action enableTimerInterrupt)
        {
            EnableTimerInterrupt = enableTimerInterrupt;
        }

        private void Tick()
        {
            var before = (InternalCounter & (1 << TACBitSelected)) == 0;
            InternalCounter++;
            var overflow = (InternalCounter & (1 << TACBitSelected)) == 0;
            if (before == false && overflow == true && TimerEnabled)
            {
                IncrementTIMA();
            }
        }

        public void Add(long n)
        {
            for (long i = 0; i < n; i++) Tick();
        }
        public byte DIV
        {
            get => (byte)((InternalCounter & 0xff00) >> 8);
            set
            {
                InternalCounter = 0;
            }
        }

        public byte TMA { get; set; }

        public byte TAC { get; set; }

        bool TimerEnabled => TAC.GetBit(2);
        int TACBitSelected => (TAC & 0x03) switch
        {
            0 => 9,
            1 => 3,
            2 => 5,
            3 => 7,
            _ => throw new NotImplementedException(),
        };

        private byte _tima;
        public byte TIMA
        {
            get => _tima;
            set
            {
                _tima = value;
            }
        }
        private void IncrementTIMA()
        {
            if (_tima == 0xff)
            {
                _tima = TMA;
                EnableTimerInterrupt();
            }
            else _tima++;
        }
    }
}
