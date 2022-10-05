namespace emulator;

public enum Interrupt
{
    VBlank,
    STAT,
    Timer,
    Serial,
    Joypad
}

public class InterruptRegisters
{
    //Interrupt Master Enable
    public bool IME;
    //We want to enable Interrupts on the next M cycle if this is set
    public bool InterruptEnableScheduled;

    public void EnableInterruptsIfScheduled()
    {
        if (InterruptEnableScheduled)
        {
            IME = true;
            InterruptEnableScheduled = false;
        }
    }

    public Interrupt? Fired()
    {
        if (VBlankEnabled && VBlankRequested) return Interrupt.VBlank;
        if (STATEnabled && STATRequested)     return Interrupt.STAT;
        if (TimerEnabled && TimerRequested)   return Interrupt.Timer;
        if (SerialEnabled && SerialRequested) return Interrupt.Serial;
        if (JoypadEnabled && JoypadRequested) return Interrupt.Joypad;

        return null;
    }

    internal void ClearInterrupt(Interrupt interrupt)
    {
        if (interrupt == Interrupt.VBlank) VBlankRequested = false;
        if (interrupt == Interrupt.STAT)     STATRequested = false;
        if (interrupt == Interrupt.Timer)   TimerRequested = false;
        if (interrupt == Interrupt.Serial) SerialRequested = false;
        if (interrupt == Interrupt.Joypad) JoypadRequested = false;
    }

    internal static ushort Address(Interrupt interrupt) => interrupt switch
    {
        Interrupt.VBlank => 0x40,
        Interrupt.STAT => 0x48,
        Interrupt.Timer => 0x50,
        Interrupt.Serial => 0x58,
        Interrupt.Joypad => 0x60,
        _ => throw new Exception()
    };

    //The request register only uses the bottom 5 bits
    public byte Request
    {
        get => (byte)(Convert.ToByte(VBlankRequested) |
                 Convert.ToByte(STATRequested) << 1 |
                  Convert.ToByte(TimerRequested) << 2 |
                   Convert.ToByte(SerialRequested) << 3 |
                    Convert.ToByte(JoypadRequested) << 4 |
            0xe0);
        set
        {
            VBlankRequested = value.GetBit(0);
            STATRequested = value.GetBit(1);
            TimerRequested = value.GetBit(2);
            SerialRequested = value.GetBit(3);
            JoypadRequested = value.GetBit(4);
        }
    }

    public bool VBlankRequested { get; private set; }
    public bool TimerRequested { get; private set; }
    public bool STATRequested { get; private set; }
    public bool JoypadRequested { get; private set; }
    public bool SerialRequested { get; private set; }

    //The enable register only uses the bottom 5 bits
    public byte Enable
    {
        get => (byte)(Convert.ToByte(VBlankEnabled) |
                 Convert.ToByte(STATEnabled) << 1 |
                  Convert.ToByte(TimerEnabled) << 2 |
                   Convert.ToByte(SerialEnabled) << 3 |
                    Convert.ToByte(JoypadEnabled) << 4 |
            0xe0);
        set
        {
            VBlankEnabled = value.GetBit(0);
            STATEnabled = value.GetBit(1);
            TimerEnabled = value.GetBit(2);
            SerialEnabled = value.GetBit(3);
            JoypadEnabled = value.GetBit(4);
        }
    }

    public bool VBlankEnabled { get; private set; }
    public bool TimerEnabled { get; private set; }
    public bool STATEnabled { get; private set; }
    public bool JoypadEnabled { get; private set; }
    public bool SerialEnabled { get; private set; }

    private void TriggerEvent(object? sender, EventArgs e) => JoypadRequested = true;
    internal void EnableVBlankInterrupt(object? sender, EventArgs e) => VBlankRequested = true;
    internal void EnableLCDSTATInterrupt(object? sender, EventArgs e) => STATRequested = true;
    internal void EnableTimerInterrupt(object? sender, EventArgs e) => TimerRequested = true;
    internal void SetStateWithoutBootrom()
    {
        Request = 0xe1;
        IME = true;
    }

    public InterruptRegisters(Keypad keypad) => keypad.Input.KeyWentDown += TriggerEvent;
}
