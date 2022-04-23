﻿namespace emulator.sound;

public abstract class Channel
{
    public void TickLength()
    {
        if (ChannelEnabled == false) return;

        if (SoundLength <= 0) throw new Exception("Unexpected");
        SoundLength--;
        if (SoundLength == 0) ChannelEnabled = false;
    }

    protected abstract int SoundLength { get; set; }
    protected abstract int SoundLengthMAX { get; }

    public bool IsOn() => ChannelEnabled;
    public abstract void Clock();

    protected bool ChannelEnabled;

    protected virtual void Trigger()
    {
        ChannelEnabled = true;
        if (SoundLength == 0) SoundLength = SoundLengthMAX;
        //Frequency timer is reloaded with period.
        //Volume envelope timer is reloaded with period.
        //Channel volume is reloaded from NRx2.
    }
}