using emulator.extensions;

namespace emulator.sound;

public class WaveChannelState
{
    public required ChannelState Channel { get; init; }
    public required bool ChannelOff { get; init; }
    public required WaveOutputLevel OutputLevel { get; init; }
    public required ushort Frequency { get; init; }
    public required int PositionCounter { get; init; }
    public required int FrequencyTimer { get; init; }
    public required int AccessWindow { get; init; }
    public required int FetchedByteIndex { get; init; }
}

internal class WaveChannel : Channel
{
    public WaveChannelState GetState() => new()
    {
        Channel = GetChannelState(),
        ChannelOff = ChannelOff,
        OutputLevel = OutputLevel,
        Frequency = Frequency,
        PositionCounter = PositionCounter,
        FrequencyTimer = frequencyTimer,
        AccessWindow = accessWindow,
        FetchedByteIndex = fetchedByteIndex
    };

    private readonly byte[] table;

    private bool ChannelOff;
    public byte NR30
    {
        get => (byte)((Convert.ToByte(ChannelOff) << 7) | 0x7f);
        set
        {
            ChannelOff = value.GetBit(7);
            if (!DACOn()) ChannelEnabled = false;
        }
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
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));
            SetLengthControl(value);
        }
    }

    protected override int SoundLengthMAX => 256;


    private int PositionCounter;
    private int frequencyTimer;
    private int accessWindow;
    private int fetchedByteIndex;

    internal override void Clock()
    {
        if (accessWindow > 0) accessWindow--;
        if (!ChannelEnabled) return;
        if (--frequencyTimer > 0) return;
        frequencyTimer = (2048 - Frequency) * 2;
        PositionCounter = (PositionCounter + 1) & 31;
        fetchedByteIndex = PositionCounter / 2;
        accessWindow = 2;
        ReadSampleFromTable();
    }

    public WaveChannel() =>
        //Initial values on the dmg
        table = [
            0x8, 0x4, 0x4, 0x0,
            0x4, 0x3, 0xA, 0xA,
            0x2, 0xD, 0x7, 0x8,
            0x9, 0x2, 0x3, 0xC,
            0x6, 0x0, 0x5, 0x9,
            0x5, 0x9, 0xB, 0x0,
            0x3, 0x4, 0xB, 0x8,
            0x2, 0xE, 0xD, 0xA];


    private byte sample;
    private void ReadSampleFromTable() => sample = table[PositionCounter];

    protected override void Trigger()
    {
        //DMG retriggering during the next RAM fetch corrupts the first four bytes.
        if (ChannelEnabled && frequencyTimer == 2)
        {
            int nextByte = ((PositionCounter + 1) & 31) / 2;
            if (nextByte < 4)
                Array.Copy(table, nextByte * 2, table, 0, 2);
            else
                Array.Copy(table, (nextByte & ~3) * 2, table, 0, 8);
        }
        base.Trigger();
        PositionCounter = 0;
        frequencyTimer = (2048 - Frequency) * 2 + 6;
        accessWindow = 0;
    }

    public override byte Sample() => OutputLevel switch
    {
        WaveOutputLevel.Mute => 0,
        WaveOutputLevel.Half => (byte)(sample >> 1),
        WaveOutputLevel.Quarter => (byte)(sample >> 2),
        WaveOutputLevel.Full => sample,
        _ => throw new NotSupportedException()
    };

    //This is suspicious, shouldn't this bit be flipped?
    internal override bool DACOn() => ChannelOff;

    public byte this[int n]
    {
        get
        {
            if (ChannelEnabled && accessWindow == 0) return 0xff;
            if (ChannelEnabled) n = fetchedByteIndex;
            return (byte)(table[n * 2] << 4 | table[n * 2 + 1]);
        }
        set
        {
            if (ChannelEnabled && accessWindow == 0) return;
            if (ChannelEnabled) n = fetchedByteIndex;
            table[n * 2] = (byte)(value >> 4);
            table[n * 2 + 1] = (byte)(value & 0x0f);
        }
    }
}
