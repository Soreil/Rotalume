using System.Collections;

namespace emulator.sound;

internal class ToneSweepChannel : Channel
{
    private bool sweepEnabled;
    private ushort frequencyShadowRegister;
    private int sweepTimer;

    public void TickSweep()
    {
        if (sweepEnabled && sweepTimer != 0)
        {
            var newFreq = CalculateSweepFrequency();

            //If the new frequency is 2047 or less and the sweep shift is not zero,
            //this new frequency is written back to the shadow frequency and square 1's frequency in NR13 and NR14
            if (newFreq <= 2047 && SweepShift != 0)
            {
                frequencyShadowRegister = newFreq;
                Frequency = newFreq;
                //frequency calculation and overflow check are run AGAIN immediately using this new value,
                //but this second new frequency is not written back.
                _ = CalculateSweepFrequency();
            }
        }
    }

    public void TriggerSweep()
    {
        frequencyShadowRegister = Frequency;
        sweepTimer = SweepPeriod;
        sweepEnabled = SweepPeriod != 0 || SweepShift != 0;

        //If the sweep shift is non - zero, frequency calculation and the overflow check are performed immediately.
        if (SweepShift != 0)
        {
            frequencyShadowRegister = CalculateSweepFrequency();
        }
    }

    private ushort CalculateSweepFrequency()
    {
        var tmp = frequencyShadowRegister >> SweepShift;
        if (!SweepIncreasing) tmp = -tmp;
        var newFrequency = (ushort)(frequencyShadowRegister + tmp);

        //Overflow check
        if (newFrequency > 2047) ChannelEnabled = false;

        return newFrequency;
    }

    private int SweepPeriod;
    private bool SweepIncreasing;
    private int SweepShift;

    public byte NR10
    {
        get => (byte)((SweepPeriod << 4) | (Convert.ToByte(SweepIncreasing) << 3) | (SweepShift & 0x07));

        set
        {
            SweepPeriod = value >> 4;
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
            SoundLength = value & 0x3f;
        }
    }

    public void TickVolEnv()
    {
        if (envelopeVolume is 0 or 15) return;
        envelopeVolume += EnvelopeIncreasing ? +1 : -1;
    }

    int envelopeVolume;

    private int InitialEnvelopeVolume;
    private bool EnvelopeIncreasing;
    private int EnvelopeSweepNumber;

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

    ushort Frequency;

    public byte NR13 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFFF0) | value); }

    private bool CounterSelection;

    public byte NR14
    {
        get => (byte)(Convert.ToByte(CounterSelection) | 0xbf);
        set
        {
            CounterSelection = value.GetBit(6);
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));

            if (value.GetBit(7)) base.Trigger();
        }
    }

    protected override void Trigger()
    {
        base.Trigger();
        //Square 1's sweep does several things (see frequency sweep).
        TriggerSweep();
    }

    protected override int SoundLengthMAX => 64;

    protected override int SoundLength { get; set; }

    private int WaveFormIndex;

    private static BitArray GetWaveForm(WavePatternDuty pattern) => pattern switch
    {
        WavePatternDuty.Eigth => new(new bool[] { false, false, false, false, false, false, false, true }),
        WavePatternDuty.Quarter => new(new bool[] { true, false, false, false, false, false, false, true }),
        WavePatternDuty.Half => new(new bool[] { false, false, false, false, false, true, true, true }),
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
