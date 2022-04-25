﻿namespace emulator.sound;

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

    private bool OutputToLeftTerminal;
    private bool OutputToRightTerminal;
    private int OutputVolumeLeft;
    private int OutputVolumeRight;

    private byte NR50
    {
        get => (byte)(Convert.ToByte(OutputToLeftTerminal) << 7 |
                OutputVolumeLeft << 4 |
                Convert.ToByte(OutputToRightTerminal) << 3 |
                OutputVolumeRight);

        set
        {
            OutputToLeftTerminal = value.GetBit(7);
            OutputToRightTerminal = value.GetBit(3);
            OutputVolumeLeft = (value >> 4) & 0x7;
            OutputVolumeRight = value & 0x7;
        }
    }
    private bool Sound4LeftOn;
    private bool Sound3LeftOn;
    private bool Sound2LeftOn;
    private bool Sound1LeftOn;
    private bool Sound4RightOn;
    private bool Sound3RightOn;
    private bool Sound2RightOn;
    private bool Sound1RightOn;

    public byte NR51
    {
        get => (byte)((Convert.ToByte(Sound4LeftOn) << 7) |
             (Convert.ToByte(Sound3LeftOn) << 6) |
             (Convert.ToByte(Sound2LeftOn) << 5) |
             (Convert.ToByte(Sound1LeftOn) << 4) |
             (Convert.ToByte(Sound4RightOn) << 3) |
             (Convert.ToByte(Sound3RightOn) << 2) |
             (Convert.ToByte(Sound2RightOn) << 1) |
             (Convert.ToByte(Sound1RightOn) << 0));
        set
        {
            Sound4LeftOn = value.GetBit(7);
            Sound3LeftOn = value.GetBit(6);
            Sound2LeftOn = value.GetBit(5);
            Sound1LeftOn = value.GetBit(4);
            Sound4RightOn = value.GetBit(3);
            Sound3RightOn = value.GetBit(2);
            Sound2RightOn = value.GetBit(1);
            Sound1RightOn = value.GetBit(0);
        }
    }


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

        SoundClock = 0;
        MasterSoundDisable = true;
    }
    private void TurnOn()
    {
        if (MasterSoundDisable == false) return;
        MasterSoundDisable = false;
    }

    //We should have this available as a namespace wide thing somehow
    private const int baseClock = cpu.Constants.Frequency;

    private int SoundClock;
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

    private int FrameSequencerState;
    public void FrameSequencerClock()
    {
        switch (FrameSequencerState)
        {
            case 0:
            ToneSweep.TickLength();
            Tone.TickLength();
            Wave.TickLength();
            Noise.TickLength();
            FrameSequencerState++;
            break;

            case 1:
            FrameSequencerState++;
            break;

            case 2:
            ToneSweep.TickLength();
            Tone.TickLength();
            Wave.TickLength();
            Noise.TickLength();

            ToneSweep.TickSweep();
            FrameSequencerState++;
            break;

            case 3:
            FrameSequencerState++;
            break;

            case 4:
            ToneSweep.TickLength();
            Tone.TickLength();
            Wave.TickLength();
            Noise.TickLength();

            FrameSequencerState++;
            break;

            case 5:
            FrameSequencerState++;
            break;

            case 6:
            ToneSweep.TickLength();
            Tone.TickLength();
            Wave.TickLength();
            Noise.TickLength();

            ToneSweep.TickSweep();
            FrameSequencerState++;
            break;

            case 7:
            ToneSweep.TickVolEnv();
            Tone.TickVolEnv();
            Noise.TickVolEnv();

            FrameSequencerState = 0;
            break;

            default:
            throw new Exception("Illegal Frameclock step");
        }
    }

    private const int FrameSequencerPeriod = baseClock / 512;
    internal void Tick(object? o, EventArgs e)
    {
        if (MasterSoundDisable) return;
        if (SoundClock % FrameSequencerPeriod == 0)
            FrameSequencerClock();

        if (ToneSweep.IsOn())
        {
            if (SoundClock % ((2048 - ToneSweep.Frequency) * 4) == 0)
            {
                ToneSweep.Clock();
            }
        }
        if (Tone.IsOn())
        {
            if (SoundClock % ((2048 - Tone.Frequency) * 4) == 0)
            {
                Tone.Clock();
            }
        }
        if (Wave.IsOn())
        {
            if (SoundClock % ((2048 - Wave.Frequency) * 2) == 0)
            {
                Wave.Clock();
            }
        }
        if (Noise.IsOn())
        {
            if (SoundClock % 8 == 0)
            {
                Noise.Clock();
            }
        }

        SoundClock++;
    }

    public (byte left, byte right) Sample()
    {
        byte volumeLeft = 0;
        byte volumeRight = 0;
        if (ToneSweep.IsOn())
        {
            if (Sound1LeftOn) volumeLeft += ToneSweep.Sample();
            if (Sound1RightOn) volumeRight += ToneSweep.Sample();
        }
        if (Tone.IsOn())
        {
            if (Sound2LeftOn) volumeLeft += Tone.Sample();
            if (Sound2RightOn) volumeRight += Tone.Sample();
        }
        if (Wave.IsOn())
        {
            if (Sound3LeftOn) volumeLeft += Wave.Sample();
            if (Sound3RightOn) volumeRight += Wave.Sample();
        }

        if (Noise.IsOn())
        {
            if (Sound4LeftOn) volumeLeft += Noise.Sample();
            if (Sound4RightOn) volumeRight += Noise.Sample();
        }

        if (OutputToLeftTerminal)
        {
            volumeLeft *= (byte)(OutputVolumeLeft + 1);
        }

        if (OutputToRightTerminal)
        {
            volumeRight *= (byte)(OutputVolumeRight + 1);
        }

        return (volumeLeft, volumeRight);
    }

    public byte this[Address index]
    {
        get
        {
            if (MasterSoundDisable && index != Address.NR52) return 0xff;

            return index switch
            {
                Address.NR10   => ToneSweep.NR10,
                Address.NR11   => ToneSweep.NR11,
                Address.NR12   => ToneSweep.NR12,
                Address.NR13   => ToneSweep.NR13,
                Address.NR14   => ToneSweep.NR14,
                               
                Address.NR21   => Tone.NR21,
                Address.NR22   => Tone.NR22,
                Address.NR23   => Tone.NR23,
                Address.NR24   => Tone.NR24,
                               
                Address.NR30   => Wave.NR30,
                Address.NR31   => Wave.NR31,
                Address.NR32   => Wave.NR32,
                Address.NR33   => Wave.NR33,
                Address.NR34   => Wave.NR34,
                               
                Address.NR41   => Noise.NR41,
                Address.NR42   => Noise.NR42,
                Address.NR43   => Noise.NR43,
                Address.NR44   => Noise.NR44,
                               
                Address.NR50   => NR50,
                Address.NR51   => NR51,
                Address.NR52   => NR52,
                               
                Address.Wave0   => Wave[0],
                Address.Wave1   => Wave[1],
                Address.Wave2   => Wave[2],
                Address.Wave3   => Wave[3],
                Address.Wave4   => Wave[4],
                Address.Wave5   => Wave[5],
                Address.Wave6   => Wave[6],
                Address.Wave7   => Wave[7],
                Address.Wave8   => Wave[8],
                Address.Wave9   => Wave[9],
                Address.Wave10 => Wave[10],
                Address.Wave11 => Wave[11],
                Address.Wave12 => Wave[12],
                Address.Wave13 => Wave[13],
                Address.Wave14 => Wave[14],
                Address.Wave15 => Wave[15],

                _ => 0xff
            };
        }

        set
        {
            //Todo, allow setting of length values on DMG0 (system we are targeting)
            if (MasterSoundDisable && index != Address.NR52) return;

            Action<byte> f = index switch
            {
               Address.NR10    => x => ToneSweep.NR10 = x,
               Address.NR11    => x => ToneSweep.NR11 = x,
               Address.NR12    => x => ToneSweep.NR12 = x,
               Address.NR13    => x => ToneSweep.NR13 = x,
               Address.NR14    => x => ToneSweep.NR14 = x,
                             
               Address.NR21    => x => Tone.NR21 = x,
               Address.NR22    => x => Tone.NR22 = x,
               Address.NR23    => x => Tone.NR23 = x,
               Address.NR24    => x => Tone.NR24 = x,
                             
               Address.NR30    => x => Wave.NR30 = x,
               Address.NR31    => x => Wave.NR31 = x,
               Address.NR32    => x => Wave.NR32 = x,
               Address.NR33    => x => Wave.NR33 = x,
               Address.NR34    => x => Wave.NR34 = x,
                             
               Address.NR41    => x => Noise.NR41 = x,
               Address.NR42    => x => Noise.NR42 = x,
               Address.NR43    => x => Noise.NR43 = x,
               Address.NR44    => x => Noise.NR44 = x,
                             
               Address.NR50    => x => NR50 = x,
               Address.NR51    => x => NR51 = x,
               Address.NR52    => x => NR52 = x,
                             
               Address.Wave0   => x => Wave[0] = x,
               Address.Wave1   => x => Wave[1] = x,
               Address.Wave2   => x => Wave[2] = x,
               Address.Wave3   => x => Wave[3] = x,
               Address.Wave4   => x => Wave[4] = x,
               Address.Wave5   => x => Wave[5] = x,
               Address.Wave6   => x => Wave[6] = x,
               Address.Wave7   => x => Wave[7] = x,
               Address.Wave8   => x => Wave[8] = x,
               Address.Wave9   => x => Wave[9] = x,
               Address.Wave10  => x => Wave[10] = x,
               Address.Wave11  => x => Wave[11] = x,
               Address.Wave12  => x => Wave[12] = x,
               Address.Wave13  => x => Wave[13] = x,
               Address.Wave14  => x => Wave[14] = x,
                Address.Wave15 => x => Wave[15] = x,

                _ => x => _ = x
            };
            f(value);
        }
    }
}