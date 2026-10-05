using emulator.extensions;

namespace emulator.sound;

public class EnvelopeState
{
    public required int CurrentEnvelopeVolume { get; init; }
    public required int InitialEnvelopeVolume { get; init; }
    public required bool EnvelopeIncreasing { get; init; }
    public required int EnvelopeStepPeriod { get; init; }
    public required int EnvelopeSweepTimer { get; init; }
}

public class Envelope
{
    public EnvelopeState GetState() => new()
    {
        CurrentEnvelopeVolume = Volume,
        InitialEnvelopeVolume = InitialEnvelopeVolume,
        EnvelopeIncreasing = EnvelopeIncreasing,
        EnvelopeStepPeriod = EnvelopeStepPeriod,
        EnvelopeSweepTimer = envelopeSweepTimer
    };

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

            if (Volume < 0xf && EnvelopeIncreasing) Volume++;
            if (Volume > 0x0 && !EnvelopeIncreasing) Volume--;
        }
    }

    public int Volume { get; private set; }

    private int InitialEnvelopeVolume;
    private bool EnvelopeIncreasing;
    private int EnvelopeStepPeriod;

    private int envelopeSweepTimer;
    public void Trigger()
    {
        envelopeSweepTimer = EnvelopeStepPeriod == 0 ? 8 : EnvelopeStepPeriod;
        Volume = InitialEnvelopeVolume;
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
