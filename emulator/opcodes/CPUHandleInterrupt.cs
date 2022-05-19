namespace emulator;

public partial class CPU
{
    public bool DoInterrupt()
    {
        var coincidence = ISR.Fired();

        if (coincidence.Any() && Halted == HaltState.NormalIME1)
        {
            Halted = HaltState.off;
        }
        else if (coincidence.Any() && Halted == HaltState.normalIME0)
        {
            Halted = HaltState.off;
            return true;
        }

        if (!ISR.IME || !coincidence.Any())
        {
            return false; //Interrupts have to be globally enabled to use them
        }

        //This section follows https://gbdev.io/pandocs/Interrupts.html#interrupt-handling
        var interrupt = coincidence.First();

        ISR.IME = false;
        ISR.ClearInterrupt(interrupt);

        var addr = InterruptRegisters.Address(interrupt);
        CycleElapsed();
        CycleElapsed();
        Call(addr);

        return true;
    }
}
