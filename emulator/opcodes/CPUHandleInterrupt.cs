namespace emulator;

public partial class CPU
{
    public bool DoInterrupt()
    {
        byte coincidence = (byte)(ISR.InterruptControlRegister & ISR.InterruptFireRegister & 0x1f); //Coincidence has all the bits which have both fired AND are enabled

        if (coincidence != 0 && Halted == HaltState.normal)
        {
            Halted = HaltState.off;
            //This 4 extra clock cycles is from TCAGBD.
            CycleElapsed();
        }
        else if (coincidence != 0 && Halted == HaltState.normalIME0)
        {
            Halted = HaltState.off;
            CycleElapsed();
            return true;
        }

        if (!ISR.IME || coincidence == 0)
        {
            return false; //Interrupts have to be globally enabled to use them
        }

        for (var bit = 0; bit < 5; bit++) //Bit 0 has highest priority, we only handle one interrupt at a time
        {
            if (coincidence.GetBit(bit))
            {
                ISR.IME = false;
                var IFR = ISR.InterruptFireRegister;
                IFR.SetBit(bit, false); //Clear the interrupt bit
                ISR.InterruptFireRegister = IFR;

                var addr = (ushort)(0x40 + (0x8 * bit));
                CycleElapsed();
                CycleElapsed();
                Call( addr);

                return true;
            }
        }
        throw new IllegalInterruptState("This should be unreachable");
    }
}
