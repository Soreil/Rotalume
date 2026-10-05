namespace emulator.sound;

public class ToneChannelState
{
    public required SquareChannelState SquareChannel { get; init; }
}

internal class ToneChannel : SquareChannel
{
    public ToneChannelState GetToneChannelState() => new()
    {
        SquareChannel = GetState()
    };

    public byte NR21
    {
        get => NRs1;
        set => NRs1 = value;
    }

    public byte NR22
    {
        get => NRs2;
        set => NRs2 = value;
    }

    public byte NR23
    {
        get => NRs3;
        set => NRs3 = value;
    }

    public byte NR24
    {
        get => NRs4;
        set => NRs4 = value;
    }
}
