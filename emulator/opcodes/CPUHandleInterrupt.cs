namespace emulator.opcodes;
public partial class CPU
{
    public bool DoInterrupt()
    {
        var coincidence = ISR.Fired();

        if (coincidence != Interrupt.None && Halted == HaltState.NormalIME1)
        {
            Halted = HaltState.off;
        }
        else if (coincidence != Interrupt.None && Halted == HaltState.normalIME0)
        {
            Halted = HaltState.off;
            return true;
        }

        if (!ISR.IME || coincidence == Interrupt.None)
        {
            return false; //Interrupts have to be globally enabled to use them
        }

        //This section follows https://gbdev.io/pandocs/Interrupts.html#interrupt-handling

        ISR.IME = false;
        ISR.ClearInterrupt(coincidence);

        var addr = InterruptRegisters.Address(coincidence);
        CycleElapsed();
        CycleElapsed();
        Call(addr);

        return true;
    }
}
