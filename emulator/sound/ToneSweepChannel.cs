using System.Collections;

namespace emulator.sound;

internal class ToneSweepChannel : Channel
{
    private bool sweepEnabled;
    private ushort shadowFrequency;
    private int sweepTimer;

    //https://nightshade256.github.io/2021/03/27/gb-sound-emulation.html
    public void TickSweep()
    {
        if (sweepTimer > 0) sweepTimer--;


        if (sweepTimer == 0)
        {
            sweepTimer = SweepPeriod == 0 ? 8 : SweepPeriod;

            if (sweepEnabled && SweepPeriod != 0)
            {

                var newFreq = CalculateSweepFrequency();

                //If the new frequency is 2047 or less and the sweep shift is not zero,
                //this new frequency is written back to the shadow frequency and square 1's frequency in NR13 and NR14
                if (newFreq < 2048 && SweepShift != 0)
                {
                    Frequency = newFreq;
                    shadowFrequency = newFreq;
                    //frequency calculation and overflow check are run AGAIN immediately using this new value,
                    //but this second new frequency is not written back.
                    _ = CalculateSweepFrequency();
                }
            }
        }
    }

    public void TriggerSweep()
    {
        shadowFrequency = Frequency;
        sweepTimer = SweepPeriod == 0 ? 8 : SweepPeriod;
        sweepEnabled = SweepPeriod != 0 || SweepShift != 0;

        //If the sweep shift is non - zero, frequency calculation and the overflow check are performed immediately.
        if (SweepShift != 0)
        {
            _ = CalculateSweepFrequency();
        }
    }

    private ushort CalculateSweepFrequency()
    {
        var newFreq = shadowFrequency >> SweepShift;
        newFreq = !SweepIncreasing ? shadowFrequency - newFreq : shadowFrequency + newFreq;

        //Overflow check
        if (newFreq > 2047) ChannelEnabled = false;

        return (ushort)newFreq;
    }

    private int SweepPeriod;
    private bool SweepIncreasing;
    private int SweepShift;

    public byte NR10
    {
        get => (byte)(0x80 | (SweepPeriod << 4) | (Convert.ToByte(SweepIncreasing) << 3) | SweepShift);

        set
        {
            SweepPeriod = (value >> 4) & 0x7;
            SweepIncreasing = value.GetBit(3);
            SweepShift = value & 0x7;
        }
    }

    private WavePatternDuty wavePatternDuty;

    public byte NR11
    {
        get => (byte)(((byte)wavePatternDuty << 6) | 0x3f);

        set
        {
            wavePatternDuty = (WavePatternDuty)(value >> 6);
            NRx1 = value;
        }
    }

    //https://nightshade256.github.io/2021/03/27/gb-sound-emulation.html
    public void TickVolEnv()
    {
        if (EnvelopeSweepNumber == 0) return;
        if (envelopeSweepTimer != 0) envelopeSweepTimer--;

        if (envelopeSweepTimer == 0)
        {
            //Reload the envelope timer
            envelopeSweepTimer = EnvelopeSweepNumber;

            if (envelopeVolume < 0xf && EnvelopeIncreasing) envelopeVolume++;
            if (envelopeVolume > 0x0 && !EnvelopeIncreasing) envelopeVolume--;

        }
    }

    int envelopeVolume;

    private int InitialEnvelopeVolume;
    private bool EnvelopeIncreasing;
    private int EnvelopeSweepNumber;

    private int envelopeSweepTimer;

    public byte NR12
    {
        get => (byte)((InitialEnvelopeVolume << 4) | (Convert.ToByte(EnvelopeIncreasing) << 3) | (EnvelopeSweepNumber & 0x07));

        set
        {
            InitialEnvelopeVolume = value >> 4;
            EnvelopeIncreasing = value.GetBit(3);
            EnvelopeSweepNumber = value & 0x7;
        }
    }

    public ushort Frequency { get; private set; }

    public byte NR13 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFF00) | value); }


    protected override bool UseLength { get; set; }
    public byte NR14
    {
        get => (byte)(Convert.ToByte(UseLength) | 0xbf);
        set
        {
            UseLength = value.GetBit(6);
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));

            if (value.GetBit(7)) Trigger();
            else ChannelEnabled = false;
        }
    }

    protected override void Trigger()
    {
        base.Trigger();
        //Square 1's sweep does several things (see frequency sweep).
        TriggerSweep();
        //This channel has an envelope
        envelopeSweepTimer = EnvelopeSweepNumber;
        envelopeVolume = InitialEnvelopeVolume;
    }

    protected override int SoundLengthMAX => 64;


    private int WaveFormIndex;

    private static BitArray GetWaveForm(WavePatternDuty pattern) => pattern switch
    {
        WavePatternDuty.Eigth => new(new bool[] { false, false, false, false, false, false, false, true }),
        WavePatternDuty.Quarter => new(new bool[] { true, false, false, false, false, false, false, true }),
        WavePatternDuty.Half => new(new bool[] { false, false, false, false, true, true, true, true }),
        WavePatternDuty.ThreeFourths => new(new bool[] { false, true, true, true, true, true, true, false }),
        _ => throw new NotSupportedException()
    };

    byte CurrentSample;

    public override void Clock()
    {
        var sample = Convert.ToByte(GetWaveForm(wavePatternDuty).Get(WaveFormIndex));

        CurrentSample = (byte)(sample * envelopeVolume);

        WaveFormIndex++;
        WaveFormIndex &= 0x7;
    }
    public override byte Sample() => CurrentSample;
}
