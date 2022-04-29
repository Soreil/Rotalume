namespace emulator.sound;

public abstract class Channel
{
    public void TickLength()
    {
        if (!UseLength || !ChannelEnabled) return;

        LengthTimer--;
        if (LengthTimer == 0) ChannelEnabled = false;
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
        ChannelEnabled = true;
        if (LengthTimer == 0) LengthTimer = SoundLengthMAX;
        //Frequency timer is reloaded with period.
        //Volume envelope timer is reloaded with period.
        //Channel volume is reloaded from NRx2.
    }
}