
using System.Collections;

namespace emulator.sound;

public class NoiseChannel : Channel
{
    protected override int SoundLength { get; set; }
    public byte NR41
    {
        get => 0xff;
        set => SoundLength = value & 0x3f;
    }

    private int envelopeVolume;
    public void TickVolEnv()
    {
        if (envelopeVolume is 0 or 15) return;
        envelopeVolume += EnvelopeIncreasing ? +1 : -1;
    }

    private int InitialEnvelopeVolume;
    private bool EnvelopeIncreasing;
    private int EnvelopeSweepNumber;
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

    private int shiftClockFrequency;
    private bool ShiftRegisterWidth;
    private int FrequencyDividerRatio;
    public byte NR43
    {
        get => (byte)((shiftClockFrequency << 4) | (Convert.ToByte(ShiftRegisterWidth) << 3) | (FrequencyDividerRatio & 0x07)); set
        {
            shiftClockFrequency = value >> 4;
            ShiftRegisterWidth = value.GetBit(3);
            FrequencyDividerRatio = value & 0x7;
        }
    }

    protected override bool UseLength { get; set; }

    public byte NR44
    {
        get => (byte)((Convert.ToByte(UseLength) << 6) | 0xbf);

        set
        {
            UseLength = value.GetBit(6);

            if (value.GetBit(7)) base.Trigger();
            else ChannelEnabled = false;
        }
    }

    protected override int SoundLengthMAX => 64;


    private readonly LFSR ShiftRegister;
    private int clocks;

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

    protected override void Trigger()
    {
        base.Trigger();
        ShiftRegister.ResetBits();
    }

    public override byte Sample() => MakeSample();

    public byte MakeSample()
    {
        var start = Convert.ToByte(ShiftRegister.Output());
        return (byte)(start * envelopeVolume);
    }

    public NoiseChannel() => ShiftRegister = new();
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

    public void ResetBits() => bits.SetAll(true);

    public LFSR()
    {
        bits = new(LFSRbitCount);
        ResetBits();
    }
}