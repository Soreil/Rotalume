
using System.Collections;

namespace emulator.sound;

public class NoiseChannel : Channel
{
    public byte NR41
    {
        get => NRx1;
        set => NRx1 = value;
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


    private int DivisorShiftAmount;
    private bool ShiftRegisterWidth;
    private int BaseDivisorCode;

    private int Divisor;
    private int FrequencyTimer;
    public byte NR43
    {
        get => (byte)((DivisorShiftAmount << 4) | (Convert.ToByte(ShiftRegisterWidth) << 3) | (BaseDivisorCode & 0x07));

        set
        {
            DivisorShiftAmount = value >> 4;
            ShiftRegisterWidth = value.GetBit(3);
            BaseDivisorCode = value & 0x7;
            Divisor = GetDiv(BaseDivisorCode);
            FrequencyTimer = Divisor << DivisorShiftAmount;
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

    public override void Clock()
    {
        if (FrequencyTimer == 0)
        {
            FrequencyTimer = GetDiv(BaseDivisorCode) << DivisorShiftAmount;
            ShiftRegister.Step(ShiftRegisterWidth);
        }
        FrequencyTimer--;
    }

    private static int GetDiv(int frequencyDividerRatio) => frequencyDividerRatio switch
    {
        0 => 8,
        1 => 16,
        2 => 32,
        3 => 48,
        4 => 64,
        5 => 80,
        6 => 96,
        7 => 112,
        _ => throw new Exception("Impossible")
    };

    protected override void Trigger()
    {
        base.Trigger();
        ShiftRegister.ResetBits();
        //This channel has an envelope
        envelopeSweepTimer = EnvelopeSweepNumber;
        envelopeVolume = InitialEnvelopeVolume;
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