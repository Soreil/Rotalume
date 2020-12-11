using System;

namespace emulator
{
    //Timers require to be updated as soon as the clock reaches the next timestep.
    //Maybe an update function which gets called on Clock write?
    public class Timers
    {
        int DividerClock;
        int TimerClock;

        readonly Action EnableTimerInterrupt;
        public Timers(Action enableTimerInterrupt)
        {
            EnableTimerInterrupt = enableTimerInterrupt;

            initialDiv = 0;
            initialClock = 0;
        }

        public void Tick()
        {
            DividerClock++;

            _divider = _divider = (DividerClock - initialDiv) / dividerMod;

            if (TimerEnabled)
            {
                TimerClock++;

                var current = ((TimerClock - initialClock) / TimerScale);
                if (current > 0xff)
                {
                    //This is really ugly but it should work
                    initialClock = TimerClock - (TimerDefault * TimerScale);
                    EnableTimerInterrupt();
                    _Timer = 0;
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

        private int initialDiv;
        private int initialClock;

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
                initialDiv = DividerClock;
                _divider = initialDiv; //This is really quite stupid but it gets it to 0
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
