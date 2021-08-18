namespace emulator
{
    public class InterruptRegisters
    {
        public bool IME;
        public bool InterruptEnableScheduled;
        private byte _IE = 0xe0;

        public byte InterruptFireRegister
        {
            get => _IE;
            set => _IE = (byte)((value & 0x1f) | 0xe0);
        }

        public byte InterruptControlRegister;

        public void TriggerEvent(object? sender, EventArgs e) => _IE.SetBit(4);

        internal (Action<byte> Write, Func<byte> Read) HookUp() => (x => InterruptFireRegister = x,
    () => InterruptFireRegister);

        //These functions are only ever called by one user and could just be lambdas inplace.
        internal void EnableVBlankInterrupt() => _IE.SetBit(0);
        internal void EnableLCDSTATInterrupt() => _IE.SetBit(1);
        internal void EnableTimerInterrupt() => _IE.SetBit(2);
        internal void SetStateWithoutBootrom()
        {
            InterruptFireRegister = 0xe1;
            IME = true;
        }
    }
}