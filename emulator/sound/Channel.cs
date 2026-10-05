namespace emulator.sound;

public class ChannelState
{
    public required bool ChannelEnabled { get; init; }
    public required bool UseLength { get; init; }
    public required int LengthTimer { get; init; }
    public required bool NextStepClocksLength { get; init; }
    public required byte NRx1 { get; init; }
}

internal abstract class Channel
{
    protected virtual ChannelState GetChannelState() => new()
    {
        ChannelEnabled = ChannelEnabled,
        UseLength = UseLength,
        LengthTimer = LengthTimer,
        NextStepClocksLength = NextStepClocksLength,
        NRx1 = _NRx1
    };

    internal void TickLength()
    {
        if (!UseLength || LengthTimer == 0) return;

        LengthTimer--;
        if (LengthTimer == 0) ChannelEnabled = false;
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

    internal bool NextStepClocksLength { get; set; } = true;
    private int LengthTimer { get; set; }
    protected bool UseLength { get; set; }
    protected abstract int SoundLengthMAX { get; }

    //This field exists for serialization, we can't just read back 0xff since then we couldn't see internal state.
    private byte _NRx1;
    protected byte NRx1 { get => 0xff; set { _NRx1 = value; LengthTimer = SoundLengthMAX - (value & (SoundLengthMAX - 1)); } }
    protected bool ChannelEnabled;

    internal void PowerOff()
    {
        ChannelEnabled = false;
        UseLength = false;
    }

    internal bool IsOn() => ChannelEnabled;
    internal abstract void Clock();


    public abstract byte Sample();

    internal abstract bool DACOn();

    protected virtual void Trigger()
    {
        ChannelEnabled = DACOn();
        if (LengthTimer == 0) LengthTimer = SoundLengthMAX;
        //Frequency timer is reloaded with period.
        //Volume envelope timer is reloaded with period.
        //Channel volume is reloaded from NRx2.
    }
}