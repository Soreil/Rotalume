﻿using System.Collections;

namespace emulator.sound;

internal class ToneChannel : Channel
{
    private WavePatternDuty wavePatternDuty;
    public byte NR21
    {
        get => (byte)(((byte)wavePatternDuty << 6) | 0x3f);

        set
        {
            wavePatternDuty = (WavePatternDuty)(value >> 6);
            SoundLength = value & 0x3f;
        }
    }

    public void TickVolEnv()
    {
        if (envelopeVolume is 0 or 15) return;
        envelopeVolume += EnvelopeIncreasing ? +1 : -1;
    }

    int envelopeVolume;

    private int InitialEnvelopeVolume;
    private bool EnvelopeIncreasing;
    private int EnvelopeSweepNumber;

    public byte NR22
    {
        get => (byte)((InitialEnvelopeVolume << 4) | (Convert.ToByte(EnvelopeIncreasing) << 3) | (EnvelopeSweepNumber & 0x07));

        set
        {
            InitialEnvelopeVolume = value >> 4;
            EnvelopeIncreasing = value.GetBit(3);
            EnvelopeSweepNumber = value & 0x7;
        }
    }

    ushort Frequency;

    public byte NR23 { get => 0xff; set => Frequency = (ushort)((Frequency & 0xFFF0) | value); }

    private bool CounterSelection;

    public byte NR24
    {
        get => (byte)(Convert.ToByte(CounterSelection) | 0xbf);
        set
        {
            CounterSelection = value.GetBit(6);
            Frequency = (ushort)((Frequency & 0xF8FF) | ((value & 0x07) << 8));
            if (value.GetBit(7)) base.Trigger();
        }
    }

    protected override int SoundLengthMAX => 64;

    protected override int SoundLength { get; set; }

    private int WaveFormIndex;

    private static BitArray GetWaveForm(WavePatternDuty pattern) => pattern switch
    {
        WavePatternDuty.Eigth => new(new bool[] { false, false, false, false, false, false, false, true }),
        WavePatternDuty.Quarter => new(new bool[] { true, false, false, false, false, false, false, true }),
        WavePatternDuty.Half => new(new bool[] { false, false, false, false, false, true, true, true }),
        WavePatternDuty.ThreeFourths => new(new bool[] { false, true, true, true, true, true, true, false }),
        _ => throw new NotSupportedException()
    };

    byte CurrentSample;

    public override void Clock()
    {
        var sample = Convert.ToByte(GetWaveForm(wavePatternDuty).Get(WaveFormIndex));

        CurrentSample = (byte)(sample * envelopeVolume);

        WaveFormIndex++;
        WaveFormIndex &= 0x7;
    }
    public override byte Sample() => CurrentSample;
}
