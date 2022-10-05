namespace emulator;

public partial class CPU
{
    public bool DoInterrupt()
    {
        var coincidence = ISR.Fired();

        if (coincidence is not null && Halted == HaltState.NormalIME1)
        {
            Halted = HaltState.off;
        }
        else if (coincidence is not null && Halted == HaltState.normalIME0)
        {
            Halted = HaltState.off;
            return true;
        }

        if (!ISR.IME || coincidence is null)
        {
            return false; //Interrupts have to be globally enabled to use them
        }

        //This section follows https://gbdev.io/pandocs/Interrupts.html#interrupt-handling

        Interrupt interrupt = (Interrupt)coincidence;

        ISR.IME = false;
        ISR.ClearInterrupt(interrupt);

        var addr = InterruptRegisters.Address(interrupt);
        CycleElapsed();
        CycleElapsed();
        Call(addr);

        return true;
    }
}
