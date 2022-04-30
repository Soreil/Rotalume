namespace emulator.sound;

internal class SquareChannel : Channel
{
    public SquareChannel()
    {
        envelope = new();
    }


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


    public readonly Envelope envelope;
    public byte NRs2
    {
        get => envelope.Register;
        set => envelope.Register = value;
    }

    protected override void Trigger()
    {
        base.Trigger();
        envelope.Trigger();
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
    public override byte Sample() => (byte)(CurrentSample * envelope.Volume);
    public override bool DACOn() => (NRs2 >> 3) != 0;
}

