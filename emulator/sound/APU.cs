﻿using Microsoft.Extensions.Logging;

namespace emulator.sound;

public class APU(ILogger<APU> logger)
{
    internal void SetStateWithoutBootrom()
    {
        MasterSoundDisable = false;

        ToneSweep.NR10 = 0x80;
        ToneSweep.NR11 = 0x80;
        ToneSweep.NR12 = 0xf3;
        ToneSweep.NR13 = 0xc1;
        ToneSweep.NR14 = 0xbf;
        Tone.NR21 = 0x00;
        Tone.NR22 = 0x00;
        Tone.NR23 = 0x00;
        Tone.NR24 = 0xb8;
        Wave.NR30 = 0x7f;
        Wave.NR31 = 0x00;
        Wave.NR32 = 0x9f;
        Wave.NR33 = 0x00;
        Wave.NR34 = 0xb8;
        Noise.NR41 = 0xc0;
        Noise.NR42 = 0x00;
        Noise.NR43 = 0x00;
        Noise.NR44 = 0xbf;
        NR50 = 0x77;
        NR51 = 0xf3;
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
            byte channels = (byte)((Convert.ToByte(Noise.IsOn()) << 3) |
                            (Convert.ToByte(Wave.IsOn()) << 2) |
                            (Convert.ToByte(Tone.IsOn()) << 1) |
                            (Convert.ToByte(ToneSweep.IsOn()) << 0));

            return (byte)(0x70 | channels | (Convert.ToByte(!MasterSoundDisable) << 7));
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
        //ToneSweep.NR11 = 0;
        ToneSweep.NR12 = 0;
        ToneSweep.NR13 = 0;
        ToneSweep.NR14 = 0;

        //Tone.NR21 = 0;
        Tone.NR22 = 0;
        Tone.NR23 = 0;
        Tone.NR24 = 0;

        Wave.NR30 = 0;
        //Wave.NR31 = 0;
        Wave.NR32 = 0;
        Wave.NR33 = 0;
        Wave.NR34 = 0;

        //Noise.NR41 = 0;
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

    private int SoundClock;

    //MasterSoundDisable is a global toggle with which the APU hardware can be disabled.
    private bool MasterSoundDisable;

    private ToneSweepChannel ToneSweep { get; set; } = new();
    private ToneChannel Tone { get; set; } = new();
    private WaveChannel Wave { get; set; } = new();
    private NoiseChannel Noise { get; set; } = new();

    private ILogger Logger { get; set; } = logger;

    private int FrameSequencerState;

    //The Frame Sequencer state machine cycles through 8 states which
    //can clock audio channel length counters
    //the source for the frame sequencer clock derives from the
    //div register in the timer subsystem.
    public void FrameSequencerClock(object? o, EventArgs e)
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
            ToneSweep.envelope.Tick();
            Tone.envelope.Tick();
            Noise.envelope.Tick();

            FrameSequencerState = 0;
            break;

            default:
            throw new Exception("Illegal Frameclock step");
        }
    }

    //In the current design a tick is executed every T cycle,
    //It should be possible to modify the APU to work on M cycles
    //Instead because supposedly everything is divisible by 4.
    internal void Tick()
    {
        if (MasterSoundDisable) return;

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
            Noise.Clock();
        }

        SoundClock++;
    }

    private double capacitorLeft;
    private double capacitorRight;

    //The HighPass filter is supposed to smooth out pops and produce
    //a more realistic sound. The current values here are completely
    //untested for accuracy.
    private (double, double) HighPass(double left, double right)
    {
        var outputLeft = left - capacitorLeft;
        capacitorLeft = left - outputLeft * Math.Pow(0.999958, 64);

        var outputRight = right - capacitorRight;
        capacitorRight = right - outputRight * Math.Pow(0.999958, 64);

        return (outputLeft, outputRight);
    }

    private static int ReadSample(Channel ch) => ch.DACOn() ? (short)(ch.Sample() - 7.5) : 0;

    //Sample produces a 2 channel 16 bit sample, this method affects the internal state
    //of the APU because it call the HighPass filter which affects the emulated capacitors.
    public (short left, short right) Sample()
    {
        //Early exit in case all the channels are disabled.
        if (!ToneSweep.IsOn() && !Tone.IsOn() && !Wave.IsOn() && !Noise.IsOn()) return (0, 0);

        double volumeLeft = 0;
        double volumeRight = 0;

        if (ToneSweep.IsOn())
        {
            var sample = ReadSample(ToneSweep);
            if (Sound1LeftOn) volumeLeft += sample;
            if (Sound1RightOn) volumeRight += sample;
        }
        if (Tone.IsOn())
        {
            var sample = ReadSample(Tone);
            if (Sound2LeftOn) volumeLeft += sample;
            if (Sound2RightOn) volumeRight += sample;
        }
        if (Wave.IsOn())
        {
            var sample = ReadSample(Wave);
            if (Sound3LeftOn) volumeLeft += sample;
            if (Sound3RightOn) volumeRight += sample;
        }
        if (Noise.IsOn())
        {
            var sample = ReadSample(Noise);
            if (Sound4LeftOn) volumeLeft += sample;
            if (Sound4RightOn) volumeRight += sample;
        }

        if (OutputToLeftTerminal)
        {
            volumeLeft *= (short)(OutputVolumeLeft + 1);
        }
        if (OutputToRightTerminal)
        {
            volumeRight *= (short)(OutputVolumeRight + 1);
        }

        //map to 16 bit space
        volumeLeft *= 128;
        volumeRight *= 128;

        var asFloatLeft = volumeLeft / (1 << 15);
        var asFloatRight = volumeRight / (1 << 15);

        (var left, var right) = HighPass(asFloatLeft, asFloatRight);

        return ((short)(left * (1 << 15)), (short)(right * (1 << 15)));
    }

    public byte this[Address index]
    {
        get =>
            //Length registers are accesible during power off on the dmg
            //if (MasterSoundDisable &&
            //    index is not Address.NR52
            //              or Address.NR11
            //              or Address.NR21
            //              or Address.NR31
            //              or Address.NR41)
            //    return 0xff;

            index switch
            {
                Address.NR10 => ToneSweep.NR10,
                Address.NR11 => ToneSweep.NR11,
                Address.NR12 => ToneSweep.NR12,
                Address.NR13 => ToneSweep.NR13,
                Address.NR14 => ToneSweep.NR14,

                Address.NR21 => Tone.NR21,
                Address.NR22 => Tone.NR22,
                Address.NR23 => Tone.NR23,
                Address.NR24 => Tone.NR24,

                Address.NR30 => Wave.NR30,
                Address.NR31 => Wave.NR31,
                Address.NR32 => Wave.NR32,
                Address.NR33 => Wave.NR33,
                Address.NR34 => Wave.NR34,

                Address.NR41 => Noise.NR41,
                Address.NR42 => Noise.NR42,
                Address.NR43 => Noise.NR43,
                Address.NR44 => Noise.NR44,

                Address.NR50 => NR50,
                Address.NR51 => NR51,
                Address.NR52 => NR52,

                Address.Wave0 => Wave[0],
                Address.Wave1 => Wave[1],
                Address.Wave2 => Wave[2],
                Address.Wave3 => Wave[3],
                Address.Wave4 => Wave[4],
                Address.Wave5 => Wave[5],
                Address.Wave6 => Wave[6],
                Address.Wave7 => Wave[7],
                Address.Wave8 => Wave[8],
                Address.Wave9 => Wave[9],
                Address.Wave10 => Wave[10],
                Address.Wave11 => Wave[11],
                Address.Wave12 => Wave[12],
                Address.Wave13 => Wave[13],
                Address.Wave14 => Wave[14],
                Address.Wave15 => Wave[15],

                _ => 0xff
            };

        set
        {
            //We want to allow writes to the length counters
            //As well as the wave table, even when the APU is off
            if (MasterSoundDisable && UnwriteableDuringPowerOff(index)) return;

            //Logger.LogInformation($@"Time:{DateTime.Now:ss.ffff} 		CYC:{SoundClock}			 addr:{index}		{value}");

            Action<byte> f = index switch
            {
                Address.NR10 => x => ToneSweep.NR10 = x,
                Address.NR11 => x => ToneSweep.NR11 = x,
                Address.NR12 => x => ToneSweep.NR12 = x,
                Address.NR13 => x => ToneSweep.NR13 = x,
                Address.NR14 => x => ToneSweep.NR14 = x,

                Address.NR21 => x => Tone.NR21 = x,
                Address.NR22 => x => Tone.NR22 = x,
                Address.NR23 => x => Tone.NR23 = x,
                Address.NR24 => x => Tone.NR24 = x,

                Address.NR30 => x => Wave.NR30 = x,
                Address.NR31 => x => Wave.NR31 = x,
                Address.NR32 => x => Wave.NR32 = x,
                Address.NR33 => x => Wave.NR33 = x,
                Address.NR34 => x => Wave.NR34 = x,

                Address.NR41 => x => Noise.NR41 = x,
                Address.NR42 => x => Noise.NR42 = x,
                Address.NR43 => x => Noise.NR43 = x,
                Address.NR44 => x => Noise.NR44 = x,

                Address.NR50 => x => NR50 = x,
                Address.NR51 => x => NR51 = x,
                Address.NR52 => x => NR52 = x,

                Address.Wave0 => x => Wave[0] = x,
                Address.Wave1 => x => Wave[1] = x,
                Address.Wave2 => x => Wave[2] = x,
                Address.Wave3 => x => Wave[3] = x,
                Address.Wave4 => x => Wave[4] = x,
                Address.Wave5 => x => Wave[5] = x,
                Address.Wave6 => x => Wave[6] = x,
                Address.Wave7 => x => Wave[7] = x,
                Address.Wave8 => x => Wave[8] = x,
                Address.Wave9 => x => Wave[9] = x,
                Address.Wave10 => x => Wave[10] = x,
                Address.Wave11 => x => Wave[11] = x,
                Address.Wave12 => x => Wave[12] = x,
                Address.Wave13 => x => Wave[13] = x,
                Address.Wave14 => x => Wave[14] = x,
                Address.Wave15 => x => Wave[15] = x,

                _ => x => _ = x
            };
            f(value);
        }
    }

    private static bool UnwriteableDuringPowerOff(Address index) => index is
                             (not Address.NR52
                              or Address.NR11
                              or Address.NR21
                              or Address.NR31
                              or Address.NR41
                              or Address.Wave0
                              or Address.Wave1
                              or Address.Wave2
                              or Address.Wave3
                              or Address.Wave4
                              or Address.Wave5
                              or Address.Wave6
                              or Address.Wave7
                              or Address.Wave8
                              or Address.Wave9
                              or Address.Wave10
                              or Address.Wave11
                              or Address.Wave12
                              or Address.Wave13
                              or Address.Wave14
                              or Address.Wave15);
}