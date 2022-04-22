namespace emulator.sound;

internal class WaveChannel : Channel
{
    private readonly byte[] table = new byte[32];

    private bool ChannelOff;
    public byte NR30
    {
        get => (byte)((Convert.ToByte(ChannelOff) << 7) | 0x7f);
        set => ChannelOff = value.GetBit(7);
    }

    public byte NR31 { get => 0xff; set => length = value; }

    private WaveOutputLevel OutputLevel;
    public byte NR32
    {
        get => (byte)(((byte)OutputLevel) << 5 | 0x9f);
        set => OutputLevel = (WaveOutputLevel)((value >> 5) & 0x3);
    }

    ushort Frequency;

    public byte NR33 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFFF0) | value); }

    private bool CounterSelection;
    private bool Restarted;
    public byte NR34
    {
        get => (byte)(Convert.ToByte(CounterSelection) | 0xbf);
        set
        {
            Restarted = value.GetBit(7);
            CounterSelection = value.GetBit(6);
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));
        }
    }

    private int length;
    private int PositionCounter;

    public override void Clock()
    {
        PositionCounter++;
        PositionCounter %= 32;

        ReadSampleFromTable();
    }

    private void ReadSampleFromTable()
    {
        Samples[0] = table[PositionCounter];
    }


    private readonly byte[] Samples = new byte[1];

    public override bool IsOn() => throw new NotImplementedException();
    public byte this[int n]
    {
        get => (byte)(table[n * 2] << 4 | table[n * 2 + 1]);
        set
        {
            table[n * 2] = (byte)(value >> 4);
            table[n * 2 + 1] = (byte)(value & 0x0f);
        }
    }
}
