namespace emulator;

public class InterruptRegisters
{
    //Interrupt Master Enable
    public bool IME;
    //We want to enable Interrupts on the next M cycle if this is set
    public bool InterruptEnableScheduled;
    private byte _IE = 0xe0;

    //0xFF0F register
    public byte InterruptFireRegister
    {
        get => _IE;
        set => _IE = (byte)((value & 0x1f) | 0xe0);
    }

    //0xFFFF register
    public byte InterruptControlRegister;

    private void TriggerEvent(object? sender, EventArgs e) => _IE.SetBit(4);


    //These functions are only ever called by one user and could just be lambdas inplace.
    internal void EnableVBlankInterrupt(object? sender, EventArgs e) => _IE.SetBit(0);
    internal void EnableLCDSTATInterrupt(object? sender, EventArgs e) => _IE.SetBit(1);
    internal void EnableTimerInterrupt(object? sender, EventArgs e) => _IE.SetBit(2);
    internal void SetStateWithoutBootrom()
    {
        InterruptFireRegister = 0xe1;
        IME = true;
    }

    public InterruptRegisters(Keypad keypad)
    {
        keypad.Input.KeyWentDown += TriggerEvent;
    }
}
