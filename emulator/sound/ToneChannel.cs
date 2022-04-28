namespace emulator.sound;

internal class ToneChannel : SquareChannel
{
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
