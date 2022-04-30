
using System.Collections;

namespace emulator.sound;
public class NoiseChannel : Channel
{
    public byte NR41
    {
        get => NRx1;
        set => NRx1 = value;
    }

    public readonly Envelope envelope;
    public byte NR42
    {
        get => envelope.Register;
        set => envelope.Register = value;
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
        envelope.Trigger();
    }

    public override byte Sample() => MakeSample();

    public byte MakeSample()
    {
        var start = Convert.ToByte(ShiftRegister.Output());
        return (byte)(start * envelope.Volume);
    }

    public override bool DACOn() => (NR42 >> 3) != 0;

    public NoiseChannel()
    {
        ShiftRegister = new();
        envelope = new();
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

    public void ResetBits() => bits.SetAll(true);

    public LFSR()
    {
        bits = new(LFSRbitCount);
        ResetBits();
    }
}