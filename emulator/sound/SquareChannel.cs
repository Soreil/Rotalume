namespace emulator.sound;

internal class SquareChannel : Channel
{
    private WavePatternDuty wavePatternDuty;
    public byte NRs1
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

    public byte NRs2
    {
        get => (byte)((InitialEnvelopeVolume << 4) | (Convert.ToByte(EnvelopeIncreasing) << 3) | (EnvelopeSweepNumber & 0x07));

        set
        {
            InitialEnvelopeVolume = value >> 4;
            EnvelopeIncreasing = value.GetBit(3);
            EnvelopeSweepNumber = value & 0x7;
        }
    }

    public ushort Frequency { get; protected set; }

    public byte NRs3 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFF00) | value); }

    public byte NRs4
    {
        get => (byte)((Convert.ToByte(UseLength) << 6) | 0xbf);
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

    private static readonly byte[,] waveTable = new byte[4, 8]
    {
        {0,0,0,0,0,0,0,1 },
        {1,0,0,0,0,0,0,1 },
        {0,0,0,0,1,1,1,1 },
        {0,1,1,1,1,1,1,0 },
    };

    private byte CurrentSample;

    public override void Clock()
    {
        CurrentSample = waveTable[(int)wavePatternDuty, WaveFormIndex];

        WaveFormIndex++;
        WaveFormIndex &= 0x7;
    }
    public override byte Sample() => (byte)(CurrentSample * envelopeVolume);
    public override bool DACOn() => (NRs2 >> 3) != 0;
}

