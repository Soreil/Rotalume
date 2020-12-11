using System;

namespace emulator
{
    public class Timers
    {
        int DividerClock;
        int TimerClock;

        readonly Action EnableTimerInterrupt;
        public Timers(Action enableTimerInterrupt)
        {
            EnableTimerInterrupt = enableTimerInterrupt;
        }

        public void Tick()
        {
            DividerClock++;

            _divider = DividerClock / dividerMod;

            if (TimerEnabled)
            {
                TimerClock++;

                var current = TimerClock / TimerScale;
                if (current > 0xff)
                {
                    //This is really ugly but it should work
                    EnableTimerInterrupt();
                    TimerClock = TimerDefault * TimerScale;
                    _Timer = TimerDefault;
                }
                else
                {
                    _Timer = (byte)current;
                }
            }
        }

        public void Add(int n)
        {
            for (int i = 0; i < n; i++) Tick();
        }

        const int dividerMod = 16384;

        int _divider
        {
            get;
            set;
        }

        public byte Divider
        {
            get => (byte)(_divider % 256);
            set
            {
                DividerClock = 0;
            }
        }

        public byte TimerDefault { get; set; }

        public byte TimerControl { get; set; }

        bool TimerEnabled => (TimerControl & 0x04) != 0;
        int TimerScale => (TimerControl & 0x03) switch
        {
            0 => 1024,
            1 => 16,
            2 => 64,
            3 => 256,
            _ => throw new NotImplementedException(),
        };

        private int _Timer
        {
            get;
            set;
        }

        public byte Timer => (byte)_Timer;
    }
}
