using System;

namespace generator
{
    //Timers require to be updated as soon as the clock reaches the next timestep.
    //Maybe an update function which gets called on Clock write?
    public class Timers
    {
        int Clock; //We should probably have a pair of clocks so we can reset them individually.
                   //Currently we have to keep track of an offset when the divider and timer get reset.

        readonly Action EnableTimerInterrupt;
        public Timers(Action enableTimerInterrupt)
        {
            EnableTimerInterrupt = enableTimerInterrupt;

            initialDiv = 0;
            initialClock = 0;
        }

        public void Tick()
        {
            Clock++;

            _divider = Clock; // updates the clock
            _Timer = Clock; // updates the clock
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
            get
            {
                return _divider % 256;
            }
            set
            {
                _divider = (value - initialDiv) / dividerMod;
            }
        }

        public byte Divider
        {
            get => (byte)_divider;
            set
            {
                initialDiv = Clock;
                _divider = initialDiv; //This is really quite stupid but it gets it to 0
            }
        }

        public byte TimerDefault { get; set; }

        public byte TimerControl { get; set; }

        bool TimerEnabled => (TimerControl & 0x04) != 0;
        int TimerClock => (TimerControl & 0x03) switch
        {
            0 => 1024,
            1 => 16,
            2 => 64,
            3 => 256,
            _ => throw new NotImplementedException(),
        };

        private int _Timer
        {
            get
            {
                return _Timer;
            }
            set
            {
                var current = ((value - initialClock) / TimerClock);
                if (current > 0xff)
                {
                    //This is really ugly but it should work
                    initialClock = value - (TimerDefault * TimerClock);
                    EnableTimerInterrupt();
                    _Timer = 0;
                }
                else
                {
                    _Timer = (byte)current;
                }
            }
        }

        public byte Timer => (byte)_Timer;
    }
}
