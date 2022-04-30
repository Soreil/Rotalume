namespace emulator.sound;

public class Envelope
{
    //https://nightshade256.github.io/2021/03/27/gb-sound-emulation.html
    public void Tick()
    {
        if (EnvelopeStepPeriod == 0) return;

        if (envelopeSweepTimer != 0) envelopeSweepTimer--;

        if (envelopeSweepTimer == 0)
        {
            //Reload the envelope timer
            //The case where this is 0 but has to be treated as 8 can't ever possibly work, why is it so then?
            envelopeSweepTimer = EnvelopeStepPeriod == 0 ? 8 : EnvelopeStepPeriod;

            if (CurrentEnvelopeVolume < 0xf && EnvelopeIncreasing) CurrentEnvelopeVolume++;
            if (CurrentEnvelopeVolume > 0x0 && !EnvelopeIncreasing) CurrentEnvelopeVolume--;
        }
    }

    public int Volume => CurrentEnvelopeVolume;

    public int CurrentEnvelopeVolume;

    private int InitialEnvelopeVolume;
    private bool EnvelopeIncreasing;
    private int EnvelopeStepPeriod;

    private int envelopeSweepTimer;
    public void Trigger()
    {
        envelopeSweepTimer = EnvelopeStepPeriod == 0 ? 8 : EnvelopeStepPeriod;
        CurrentEnvelopeVolume = InitialEnvelopeVolume;
    }

    public byte Register
    {
        get => (byte)((InitialEnvelopeVolume << 4) | (Convert.ToByte(EnvelopeIncreasing) << 3) | (EnvelopeStepPeriod & 0x07));

        set
        {
            InitialEnvelopeVolume = value >> 4;
            EnvelopeIncreasing = value.GetBit(3);
            EnvelopeStepPeriod = value & 0x7;
        }
    }
}
