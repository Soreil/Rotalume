namespace emulator.sound;

public class NoiseChannel
{
    public byte NR41
    {
        get => 0xff;
        set => Length = value & 0x3f;
    }

    private int Length;

    int InitialEnvelopeVolume;
    bool EnvelopeIncreasing;
    int EnvelopeSweepNumber;

    public byte NR42
    {
        get => (byte)((InitialEnvelopeVolume << 4) | (Convert.ToByte(EnvelopeIncreasing) << 3) | (EnvelopeSweepNumber & 0x07));

        set
        {
            InitialEnvelopeVolume = value >> 4;
            EnvelopeIncreasing = value.GetBit(3);
            EnvelopeSweepNumber = value & 0x7;
        }
    }

    int shiftClockFrequency;
    bool CounterStepOrWidth;
    int FrequencyDividerRatio;
    public byte NR43
    {
        get => (byte)((shiftClockFrequency << 4) | (Convert.ToByte(CounterStepOrWidth) << 3) | (FrequencyDividerRatio & 0x07)); set
        {
            shiftClockFrequency = value >> 4;
            CounterStepOrWidth = value.GetBit(3);
            FrequencyDividerRatio = value & 0x7;
        }
    }

    bool RestartSound;
    bool CounterSelection;
    public byte NR44
    {
        get => (byte)((Convert.ToByte(CounterSelection) << 6) | 0xbf);

        set
        {
            RestartSound = value.GetBit(7);
            CounterSelection = value.GetBit(6);
        }
    }
}
