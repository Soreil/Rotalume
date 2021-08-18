namespace emulator
{
    public partial class CPU
    {
        public bool DoInterrupt()
        {
            byte coincidence = (byte)(ISR.InterruptControlRegister & ISR.InterruptFireRegister & 0x1f); //Coincidence has all the bits which have both fired AND are enabled

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
                return true;
            }

            if (!ISR.IME || coincidence == 0)
            {
                return false; //Interrupts have to be globally enabled to use them
            }

            for (int bit = 0; bit < 5; bit++) //Bit 0 has highest priority, we only handle one interrupt at a time
            {
                if (coincidence.GetBit(bit))
                {
                    ISR.IME = false;
                    var IFR = ISR.InterruptFireRegister;
                    IFR.SetBit(bit, false); //Clear the interrupt bit
                    ISR.InterruptFireRegister = IFR;

                    var addr = (ushort)(0x40 + (0x8 * bit));
                    Call(20, addr);

                    return true;
                }
            }
            throw new Exception("This should be unreachable");
        }
    }
}