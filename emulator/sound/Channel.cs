namespace emulator.sound;

public abstract class Channel
{
    public void TickLength()
    {
        if (ChannelEnabled == false || !UseLength) return;


        if (SoundLength <= 0) throw new Exception("Unexpected");

        SoundLength--;
        if (SoundLength == 0) ChannelEnabled = false;
    }

    protected int SoundLength { get; set; }
    protected byte NRx1 { get => 0xff; set => SoundLength = SoundLengthMAX - (value & (SoundLengthMAX - 1)); }

    protected abstract int SoundLengthMAX { get; }

    public bool IsOn() => ChannelEnabled;
    public abstract void Clock();

    protected bool ChannelEnabled;
    protected abstract bool UseLength { get; set; }

    public abstract byte Sample();

    protected virtual void Trigger()
    {
        ChannelEnabled = true;
        if (SoundLength == 0) SoundLength = SoundLengthMAX;
        //Frequency timer is reloaded with period.
        //Volume envelope timer is reloaded with period.
        //Channel volume is reloaded from NRx2.
    }
}