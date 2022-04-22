namespace emulator.sound;

internal class ToneChannel : Channel
{
    private WavePatternDuty wavePatternDuty;
    private int SoundLength;
    public byte NR21
    {
        get => (byte)(((byte)wavePatternDuty << 6) | 0x3f);

        set
        {
            wavePatternDuty = (WavePatternDuty)(value >> 6);
            SoundLength = value & 0x3f;
        }
    }

    private int InitialEnvelopeVolume;
    private bool EnvelopeIncreasing;
    private int EnvelopeSweepNumber;

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

    ushort Frequency;

    public byte NR23 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFFF0) | value); }

    private bool CounterSelection;
    private bool Restarted;
    public byte NR24
    {
        get => (byte)(Convert.ToByte(CounterSelection) | 0xbf);
        set
        {
            Restarted = value.GetBit(7);
            CounterSelection = value.GetBit(6);
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));
        }
    }

    public override bool IsOn() => throw new NotImplementedException();
    public override void Clock() => throw new NotImplementedException();
}
