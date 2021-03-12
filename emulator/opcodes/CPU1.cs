namespace emulator
{
    public partial class CPU
    {
        public bool IME;
        private HaltState Halted = HaltState.off;

        private bool InterruptEnableScheduled;
        private byte _IE = 0xe0;
        public byte InterruptFireRegister
        {
            get => _IE;
            set => _IE = (byte)((value & 0x1f) | 0xe0);
        }
        public byte InterruptControlRegister { get; set; }
        public void DoInterrupt()
        {
            byte coincidence = (byte)(InterruptControlRegister & InterruptFireRegister & 0x1f); //Coincidence has all the bits which have both fired AND are enabled

            if (Halted != HaltState.off)
            {
                if (coincidence != 0 && Halted == HaltState.normal)
                {
                    Halted = HaltState.off;
                    //This 4 extra clock cycles is from TCAGBD.
                    AddTicks(4);
                }
                else if (coincidence != 0 && Halted == HaltState.normalIME0)
                {
                    Halted = HaltState.off;
                    AddTicks(4);
                    return;
                }
            }

            if (!IME || coincidence == 0)
            {
                return; //Interrupts have to be globally enabled to use them
            }

            for (int bit = 0; bit < 5; bit++) //Bit 0 has highest priority, we only handle one interrupt at a time
            {
                if (coincidence.GetBit(bit))
                {
                    IME = false;
                    var IFR = InterruptFireRegister;
                    IFR.SetBit(bit, false);
                    InterruptFireRegister = IFR;

                    var addr = (ushort)(0x40 + (0x8 * bit));
                    Call(20, addr); //We need a cleaner way to call functions without fetching

                    return;
                }
            }
        }
    }
}