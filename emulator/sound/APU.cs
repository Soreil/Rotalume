namespace emulator.sound;

public class APU
{
    internal void SetStateWithoutBootrom()
    {
        ToneSweep.NR10 = 0x80;
        ToneSweep.NR11 = 0xB0;
        ToneSweep.NR12 = 0xf3;
        ToneSweep.NR14 = 0xbf;
        Tone.NR21 = 0x3f;
        Tone.NR22 = 0x00;
        Tone.NR23 = 0xff;
        Tone.NR24 = 0xbf;
        Wave.NR30 = 0x7f;
        Wave.NR31 = 0xff;
        Wave.NR32 = 0x9f;
        Wave.NR33 = 0xff;
        Wave.NR34 = 0xbf;
        Noise.NR42 = 0x00;
        Noise.NR43 = 0x00;
        Noise.NR44 = 0xbf;
        NR50 = 0x77;
        NR51 = 0xf3;
        NR52 = 0xf1;
    }

    private byte NR50 = 0xff;

    private byte NR51 = 0xff;

    private byte NR52
    {
        get
        {
            byte channels = (byte)((Convert.ToByte(ToneSweep.IsOn()) << 3) |
                            (Convert.ToByte(Tone.IsOn()) << 2) |
                            (Convert.ToByte(Wave.IsOn()) << 1) |
                            (Convert.ToByte(Noise.IsOn()) << 0));

            return (byte)(0x70 | channels | (Convert.ToByte(MasterSoundDisable) << 7));
        }
        set
        {
            if (value.GetBit(7)) TurnOn();
            else TurnOff();
        }
    }

    private void TurnOff()
    {
        ToneSweep.NR10 = 0;
        ToneSweep.NR11 = 0;
        ToneSweep.NR12 = 0;
        ToneSweep.NR13 = 0;
        ToneSweep.NR14 = 0;

        Tone.NR21 = 0;
        Tone.NR22 = 0;
        Tone.NR23 = 0;
        Tone.NR24 = 0;

        Wave.NR30 = 0;
        Wave.NR31 = 0;
        Wave.NR32 = 0;
        Wave.NR33 = 0;
        Wave.NR34 = 0;

        Noise.NR41 = 0;
        Noise.NR42 = 0;
        Noise.NR43 = 0;
        Noise.NR44 = 0;

        NR50 = 0;
        NR51 = 0;

        MasterSoundDisable = true;
    }
    private void TurnOn()
    {
        if (MasterSoundDisable == false) return;
        MasterSoundDisable = false;
    }

    //We should have this available as a namespace wide thing somehow
    private const int baseClock = cpu.Constants.Frequency;

    //Control register fields
    private bool LeftChannelOn => NR50.GetBit(7);
    private int LeftOutputVolume => (NR50 & 0x70) >> 4;
    private bool RightChannelOn => NR50.GetBit(3);
    private int RightOutputVolume => NR50 & 0x07;

    private bool Sound1OnLeftChannel => NR51.GetBit(4);
    private bool Sound2OnLeftChannel => NR51.GetBit(5);
    private bool Sound3OnLeftChannel => NR51.GetBit(6);
    private bool Sound4OnLeftChannel => NR51.GetBit(7);

    private bool Sound1OnRightChannel => NR51.GetBit(0);
    private bool Sound2OnRightChannel => NR51.GetBit(1);
    private bool Sound3OnRightChannel => NR51.GetBit(2);
    private bool Sound4OnRightChannel => NR51.GetBit(3);

    private bool MasterSoundDisable;

    private ToneSweepChannel ToneSweep { get; set; }
    private ToneChannel Tone { get; set; }
    private WaveChannel Wave { get; set; }
    private NoiseChannel Noise { get; set; }

    public APU(int sampleRate)
    {
        Tone = new();
        ToneSweep = new();
        Wave = new();
        Noise = new();

        this.sampleRate = sampleRate;
    }

    int sampleRate;
    internal void Tick(object? o, EventArgs e)
    {
        if (MasterSoundDisable) return;
    }

    public byte this[int index]
    {
        get
        {
            if (MasterSoundDisable && index != 0xff52) return 0xff;

            return index switch
            {
                0xff10 => ToneSweep.NR10,
                0xff11 => ToneSweep.NR11,
                0xff12 => ToneSweep.NR12,
                0xff13 => ToneSweep.NR13,
                0xff14 => ToneSweep.NR14,

                0xff16 => Tone.NR21,
                0xff17 => Tone.NR22,
                0xff18 => Tone.NR23,
                0xff19 => Tone.NR24,

                0xff1a => Wave.NR30,
                0xff1b => Wave.NR31,
                0xff1c => Wave.NR32,
                0xff1d => Wave.NR33,
                0xff1e => Wave.NR34,

                0xff20 => Noise.NR41,
                0xff21 => Noise.NR42,
                0xff22 => Noise.NR43,
                0xff23 => Noise.NR44,

                0xff24 => NR50,
                0xff25 => NR51,
                0xff26 => NR52,

                0xff30 => Wave[0],
                0xff31 => Wave[1],
                0xff32 => Wave[2],
                0xff33 => Wave[3],
                0xff34 => Wave[4],
                0xff35 => Wave[5],
                0xff36 => Wave[6],
                0xff37 => Wave[7],
                0xff38 => Wave[8],
                0xff39 => Wave[9],
                0xff3a => Wave[10],
                0xff3b => Wave[11],
                0xff3c => Wave[12],
                0xff3d => Wave[13],
                0xff3e => Wave[14],
                0xff3f => Wave[15],

                _ => 0xff
            };
        }

        set
        {
            //Todo, allow setting of length values on DMG0 (system we are targeting)
            if (MasterSoundDisable && index != 0xff52) return;

            Action<byte> f = index switch
            {
                0xff10 => x => ToneSweep.NR10 = x,
                0xff11 => x => ToneSweep.NR11 = x,
                0xff12 => x => ToneSweep.NR12 = x,
                0xff13 => x => ToneSweep.NR13 = x,
                0xff14 => x => ToneSweep.NR14 = x,

                0xff16 => x => Tone.NR21 = x,
                0xff17 => x => Tone.NR22 = x,
                0xff18 => x => Tone.NR23 = x,
                0xff19 => x => Tone.NR24 = x,

                0xff1a => x => Wave.NR30 = x,
                0xff1b => x => Wave.NR31 = x,
                0xff1c => x => Wave.NR32 = x,
                0xff1d => x => Wave.NR33 = x,
                0xff1e => x => Wave.NR34 = x,

                0xff20 => x => Noise.NR41 = x,
                0xff21 => x => Noise.NR42 = x,
                0xff22 => x => Noise.NR43 = x,
                0xff23 => x => Noise.NR44 = x,

                0xff24 => x => NR50 = x,
                0xff25 => x => NR51 = x,
                0xff26 => x => NR52 = x,

                0xff30 => x => Wave[0] = x,
                0xff31 => x => Wave[1] = x,
                0xff32 => x => Wave[2] = x,
                0xff33 => x => Wave[3] = x,
                0xff34 => x => Wave[4] = x,
                0xff35 => x => Wave[5] = x,
                0xff36 => x => Wave[6] = x,
                0xff37 => x => Wave[7] = x,
                0xff38 => x => Wave[8] = x,
                0xff39 => x => Wave[9] = x,
                0xff3a => x => Wave[10] = x,
                0xff3b => x => Wave[11] = x,
                0xff3c => x => Wave[12] = x,
                0xff3d => x => Wave[13] = x,
                0xff3e => x => Wave[14] = x,
                0xff3f => x => Wave[15] = x,

                _ => x => _ = x
            };
            f(value);
        }
    }
}
