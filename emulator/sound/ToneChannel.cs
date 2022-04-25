using System.Collections;

namespace emulator.sound;

internal class ToneChannel : Channel
{
    private WavePatternDuty wavePatternDuty;
    public byte NR21
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
    protected override void Trigger()
    {
        base.Trigger();
        //This channel has an envelope
        envelopeSweepTimer = EnvelopeSweepNumber;
        envelopeVolume = InitialEnvelopeVolume;
    }

    public byte NR22
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

    public byte NR23 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFFF0) | value); }

    protected override bool UseLength { get; set; }
    public byte NR24
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
