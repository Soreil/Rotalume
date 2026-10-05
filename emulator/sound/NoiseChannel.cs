
using emulator.extensions;

namespace emulator.sound;

public class NoiseChannelState
{
    public required ChannelState Channel { get; init; }
    public required EnvelopeState Envelope { get; init; }
    public required LFSRState LFSR { get; init; }

    public required int DivisorShiftAmount { get; init; }
    public required bool ShiftRegisterWidth { get; init; }
    public required int BaseDivisorCode { get; init; }
    public required int Divisor { get; init; }
    public required int FrequencyTimer { get; init; }

}

internal class NoiseChannel : Channel
{
    public NoiseChannelState GetState() => new()
    {
        Channel = GetChannelState(),
        Envelope = envelope.GetState(),
        LFSR = ShiftRegister.GetState(),

        DivisorShiftAmount = DivisorShiftAmount,
        ShiftRegisterWidth = ShiftRegisterWidth,
        BaseDivisorCode = BaseDivisorCode,
        Divisor = Divisor,
        FrequencyTimer = FrequencyTimer
    };

    public byte NR41
    {
        get => NRx1;
        set => NRx1 = value;
    }

    public readonly Envelope envelope;
    public byte NR42
    {
        get => envelope.Register;
        set
        {
            envelope.Register = value;
            if (!DACOn()) ChannelEnabled = false;
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

    public byte NR44
    {
        get => (byte)((Convert.ToByte(UseLength) << 6) | 0xbf);

        set => SetLengthControl(value);
    }

    protected override int SoundLengthMAX => 64;


    private readonly LFSR ShiftRegister;

    internal override void Clock()
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

    internal override bool DACOn() => (NR42 >> 3) != 0;

    internal NoiseChannel()
    {
        ShiftRegister = new();
        envelope = new();
    }
}
