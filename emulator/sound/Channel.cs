namespace emulator.sound;

public abstract class Channel
{
    public void TickLength()
    {
        if (!UseLength || LengthTimer == 0) return;

        LengthTimer--;
        if (LengthTimer == 0) ChannelEnabled = false;
    }

    internal bool NextStepClocksLength { get; set; } = true;

    internal void PowerOff()
    {
        ChannelEnabled = false;
        UseLength = false;
    }

    protected void SetLengthControl(byte value)
    {
        bool wasEnabled = UseLength;
        UseLength = (value & 0x40) != 0;
        if (!wasEnabled && UseLength && !NextStepClocksLength) TickLength();
        if ((value & 0x80) == 0) return;

        bool reloadLength = LengthTimer == 0;
        Trigger();
        if (reloadLength && UseLength && !NextStepClocksLength) TickLength();
    }

    protected int LengthTimer { get; set; }
    protected byte NRx1 { get => 0xff; set => LengthTimer = SoundLengthMAX - (value & (SoundLengthMAX - 1)); }

    protected abstract int SoundLengthMAX { get; }

    public bool IsOn() => ChannelEnabled;
    public abstract void Clock();

    protected bool ChannelEnabled;
    protected bool UseLength { get; set; }

    public abstract byte Sample();

    public abstract bool DACOn();
    protected virtual void Trigger()
    {
        ChannelEnabled = DACOn();
        if (LengthTimer == 0) LengthTimer = SoundLengthMAX;
        //Frequency timer is reloaded with period.
        //Volume envelope timer is reloaded with period.
        //Channel volume is reloaded from NRx2.
    }
}