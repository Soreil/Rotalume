namespace emulator.sound;

public class SquareChannelState
{
    public required ChannelState Channel { get; init; }
    public required EnvelopeState Envelope { get; init; }
    public required WavePatternDuty WavePatternDuty { get; init; }
    public required ushort Frequency { get; init; }
    public required int WaveFormIndex { get; init; }
    public required byte CurrentSample { get; init; }
}

internal abstract class SquareChannel : Channel
{
    public SquareChannelState GetState() => new()
    {
        Channel = GetChannelState(),
        Envelope = envelope.GetState(),
        WavePatternDuty = wavePatternDuty,
        Frequency = Frequency,
        WaveFormIndex = WaveFormIndex,
        CurrentSample = CurrentSample
    };

    public SquareChannel() => envelope = new();


    private WavePatternDuty wavePatternDuty;
    internal void ResetDuty() => wavePatternDuty = 0;

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
        set
        {
            envelope.Register = value;
            if (!DACOn()) ChannelEnabled = false;
        }
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
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));
            SetLengthControl(value);
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

    internal override void Clock()
    {
        CurrentSample = waveTable[(int)wavePatternDuty, WaveFormIndex];

        WaveFormIndex++;
        WaveFormIndex &= 0x7;
    }
    public override byte Sample() => (byte)(CurrentSample * envelope.Volume);
    internal override bool DACOn() => (NRs2 >> 3) != 0;
}

