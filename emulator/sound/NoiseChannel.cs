using System.Collections;

namespace emulator.sound;

public class NoiseChannel : Channel
{
    private int Length;
    public byte NR41
    {
        get => 0xff;
        set => Length = value & 0x3f;
    }


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
    bool ShiftRegisterWidth;
    int FrequencyDividerRatio;
    public byte NR43
    {
        get => (byte)((shiftClockFrequency << 4) | (Convert.ToByte(ShiftRegisterWidth) << 3) | (FrequencyDividerRatio & 0x07)); set
        {
            shiftClockFrequency = value >> 4;
            ShiftRegisterWidth = value.GetBit(3);
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

    private readonly LFSR ShiftRegister;

    int clocks;

    //Clock should be called every 8th tick since this is the minimum divisor of LFSR updates
    public override void Clock()
    {
        clocks++;

        var divisor = GetDiv(FrequencyDividerRatio);

        if (clocks % (divisor << shiftClockFrequency) == 0)
        {
            ShiftRegister.Step(ShiftRegisterWidth);
        }
    }

    public override bool IsOn() => throw new NotImplementedException();

    private static int GetDiv(int frequencyDividerRatio) => frequencyDividerRatio switch
    {
        0 => 1,
        1 => 2,
        2 => 4,
        3 => 6,
        4 => 8,
        5 => 10,
        6 => 12,
        7 => 14,
        _ => throw new Exception("Impossible")
    };

    public NoiseChannel()
    {
        ShiftRegister = new();
    }
}

public class LFSR
{
    private const int LFSRbitCount = 15;
    private BitArray bits;

    //Waveform output is bit 0 of the LFSR flipped
    public bool Output() => !bits[0];

    public void Step(bool WidthMode)
    {
        var newBit = bits[0] ^ bits[1];

        bits = bits.RightShift(1);
        bits.Set(LFSRbitCount - 1, newBit);

        if (WidthMode) bits.Set(6, newBit);
    }

    public LFSR()
    {
        bits = new(LFSRbitCount);
        var rand = new Random();
        var randomValue = rand.Next();

        for (int i = 0; i < LFSRbitCount; i++)
        {
            bits.Set(i, (randomValue & (1 << i)) != 0);
        }
    }
}