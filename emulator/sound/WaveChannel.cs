namespace emulator.sound;

internal class WaveChannel : Channel
{
    private readonly byte[] table;

    private bool ChannelOff;
    public byte NR30
    {
        get => (byte)((Convert.ToByte(ChannelOff) << 7) | 0x7f);
        set => ChannelOff = value.GetBit(7);
    }

    public byte NR31
    {
        get => NRx1;
        set => NRx1 = value;
    }

    private WaveOutputLevel OutputLevel;
    public byte NR32
    {
        get => (byte)(((byte)OutputLevel) << 5 | 0x9f);
        set => OutputLevel = (WaveOutputLevel)((value >> 5) & 0x3);
    }

    public ushort Frequency { get; private set; }

    public byte NR33 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFF00) | value); }

    public byte NR34
    {
        get => (byte)((Convert.ToByte(UseLength) << 6) | 0xbf);
        set
        {
            UseLength = value.GetBit(6);
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));

            if (value.GetBit(7)) base.Trigger();
            else ChannelEnabled = false;
        }
    }

    protected override int SoundLengthMAX => 256;


    private int PositionCounter;

    public override void Clock()
    {
        PositionCounter++;
        PositionCounter %= 32;

        ReadSampleFromTable();
    }

    public WaveChannel() =>
        //Initial values on the dmg
        table = new byte[32] {
            0x8, 0x4, 0x4, 0x0,
            0x4, 0x3, 0xA, 0xA,
            0x2, 0xD, 0x7, 0x8,
            0x9, 0x2, 0x3, 0xC,
            0x6, 0x0, 0x5, 0x9,
            0x5, 0x9, 0xB, 0x0,
            0x3, 0x4, 0xB, 0x8,
            0x2, 0xE, 0xD, 0xA};


    private byte sample;
    private void ReadSampleFromTable() => sample = table[PositionCounter];

    protected override void Trigger()
    {
        base.Trigger();
        PositionCounter = 0;
    }

    public override byte Sample() => OutputLevel switch
    {
        WaveOutputLevel.Mute => 0,
        WaveOutputLevel.half => (byte)(sample >> 1),
        WaveOutputLevel.quarter => (byte)(sample >> 2),
        WaveOutputLevel.full => sample,
        _ => throw new NotSupportedException()
    };
    public override bool DACOn() => NR30.GetBit(7);

    public byte this[int n]
    {
        get
        {
            if (ChannelEnabled)
            {
                var isOdd = PositionCounter % 2 == 1;
                var topHalf = isOdd ? table[(PositionCounter / 2) + 1] : table[PositionCounter / 2];
                var bottomHalf = isOdd ? table[PositionCounter / 2] : table[(PositionCounter / 2) + 1];

                return (byte)((topHalf << 4) | bottomHalf);
            }
            //if (ChannelEnabled) return 0xff;
            else return (byte)(table[n * 2] << 4 | table[n * 2 + 1]);
        }
        set
        {
            table[n * 2] = (byte)(value >> 4);
            table[n * 2 + 1] = (byte)(value & 0x0f);
        }
    }
}
