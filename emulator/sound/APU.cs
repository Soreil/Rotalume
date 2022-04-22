namespace emulator.sound;

public class APU
{
    public (Action<byte> Write, Func<byte> Read)[][] HookUpSound() => new (Action<byte> Write, Func<byte> Read)[][] {
            new (Action<byte> Write, Func<byte> Read)[] {
            ((x) => ToneSweep.NR10 = x,
            () => ToneSweep.NR10),
            ((x) => ToneSweep.NR11 = x,
            () => ToneSweep.NR11),
            ((x) => ToneSweep.NR12 = x,
            () => ToneSweep.NR12),
            ((x) => ToneSweep.NR13 = x,
            () => ToneSweep.NR13),
            ((x) => ToneSweep.NR14 = x,
            () => ToneSweep.NR14),
            }, new (Action<byte> Write, Func<byte> Read)[]{
            ((x) => Tone.NR21 = x,
            () => Tone.NR21),
            ((x) => Tone.NR22 = x,
            () => Tone.NR22),
            ((x) => Tone.NR23 = x,
            () => Tone.NR23),
            ((x) => Tone.NR24 = x,
            () => Tone.NR24),
            ((x) => Wave.NR30 = x,
            () => Wave.NR30),
            ((x) => Wave.NR31 = x,
            () => Wave.NR31),
            ((x) => Wave.NR32 = x,
            () => Wave.NR32),
            ((x) => Wave.NR33 = x,
            () => Wave.NR33),
            ((x) => Wave.NR34 = x,
            () => Wave.NR34),
            }, new (Action<byte> Write, Func<byte> Read)[]{
            ((x) => Noise.NR41 = x,
            () => Noise.NR41),
            ((x) => Noise.NR42 = x,
            () => Noise.NR42),
            ((x) => Noise.NR43 = x,
            () => Noise.NR43),
            ((x) => Noise.NR44 = x,
            () => Noise.NR44),
            ((x) => NR50 = x,
            () => NR50),
            ((x) => NR51 = x,
            () => NR51),
            ((x) => NR52 = x,
            () => NR52),
            },new (Action<byte> Write, Func<byte> Read)[] {
            ((x) => Wave[0] = x,
            () => Wave[0]),
            ((x) => Wave[1] = x,
            () => Wave[1]),
            ((x) => Wave[2] = x,
            () => Wave[2]),
            ((x) => Wave[3] = x,
            () => Wave[3]),
            ((x) => Wave[4] = x,
            () => Wave[4]),
            ((x) => Wave[5] = x,
            () => Wave[5]),
            ((x) => Wave[6] = x,
            () => Wave[6]),
            ((x) => Wave[7] = x,
            () => Wave[7]),
            ((x) => Wave[8] = x,
            () => Wave[8]),
            ((x) => Wave[9] = x,
            () => Wave[9]),
            ((x) => Wave[10] = x,
            () => Wave[10]),
            ((x) => Wave[11] = x,
            () => Wave[11]),
            ((x) => Wave[12] = x,
            () => Wave[12]),
            ((x) => Wave[13] = x,
            () => Wave[13]),
            ((x) => Wave[14] = x,
            () => Wave[14]),
            ((x) => Wave[15] = x,
            () => Wave[15])
        } };

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

    private byte _nr52 = 0xf1;
    private byte NR52
    {
        get
        {
            byte channels = (byte)((Convert.ToByte(Sound1OnEnabled) << 3) |
                            (Convert.ToByte(Sound2OnEnabled) << 2) |
                            (Convert.ToByte(Sound3OnEnabled) << 1) |
                            (Convert.ToByte(Sound4OnEnabled) << 0));

            return (byte)(_nr52 | channels);
        }
        set => _nr52 = (byte)(value & 0x80 | (_nr52 & 0x7f));
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

    private bool MasterSoundDisable => NR52.GetBit(7);
    private bool Sound1OnEnabled;
    private bool Sound2OnEnabled;
    private bool Sound3OnEnabled;
    private bool Sound4OnEnabled;

    private WaveChannel Wave { get; set; }
    private NoiseChannel Noise { get; set; }
    private ToneSweepChannel ToneSweep { get; set; }
    private ToneChannel Tone { get; set; }

    private int _sampleCount;
    public int SampleCount
    {
        get => _sampleCount;
        set => _sampleCount = (_sampleCount + value) % Samples.Length;
    }
    private readonly float[] Samples;
    public APU(int sampleRate)
    {
        Tone = new();
        ToneSweep = new();
        Wave = new();
        Noise = new();

        TicksPerSample = baseClock / sampleRate;
        Samples = new float[sampleRate];

        if (TicksPerSample * sampleRate != baseClock)
        {
            throw new IllegalSampleRateException("We want a sample rate which is evenly divisible in to the base clock");
        }
    }

    private int APUClock;
    private readonly int TicksPerSample;

    private const int FrameSequencerFrequency = baseClock / 512;
    internal void Tick(object? o, EventArgs e)
    {
        if (((byte)APUClock) == TicksPerSample)
        {
            Samples[SampleCount++] = SampleSound();
            if (APUClock == FrameSequencerFrequency)
            {
                TickFrameSequencer();
                APUClock = 0;
            }
        }
        APUClock++;
    }

    private static float SampleSound() => 0.0f;

    private int FrameSequencerClock;
    private void TickFrameSequencer()
    {
        //Length counter
        //We have to disable the channel when NRx1 ticks to 0
        //We should only be accessing the lower 6 bits of these channels, the top bits
        //are used for a different function
        if ((FrameSequencerClock & 1) == 0)
        {
            if ((ToneSweep.NR11 & 0x3f) > 0) ToneSweep.NR11 = (byte)((ToneSweep.NR11 & 0xc0) | ((ToneSweep.NR11 & 0x3f) - 1));
            if ((Tone.NR21 & 0x3f) > 0) Tone.NR21 = (byte)((Tone.NR21 & 0xc0) | ((Tone.NR21 & 0x3f) - 1));
            if (Wave.NR31 > 0) Wave.NR31--; //NR31 uses the full byte for the length counter
            if ((Noise.NR41 & 0x3f) > 0) Noise.NR41 = (byte)((Noise.NR41 & 0xc0) | ((Noise.NR41 & 0x3f) - 1));

            if ((ToneSweep.NR11 & 0x3f) == 0) Sound1OnEnabled = false;
            if ((Tone.NR21 & 0x3f) == 0) Sound2OnEnabled = false;
            if (Wave.NR31 == 0) Sound3OnEnabled = false;
            if ((Noise.NR41 & 0x3f) == 0) Sound4OnEnabled = false;
        }

        //Tick volume envelope internal counter
        if (FrameSequencerClock == 7)
        {
            ////Sweep until we have done the requested number of sweeps
            //if (Channel1EnveloppeSweepNumber != 0)
            //{
            //    Channel1EnveloppeSweepNumber--;

            //    if (Channel1EnveloppeIncreasing)
            //    {
            //        //If we are not maxed out yet, increase
            //        if (Channel1Enveloppe != 0xf)
            //            Channel1Enveloppe++;
            //    }
            //    else
            //    {
            //        //If we are not bottomed out yet, decrease
            //        if (Channel1Enveloppe != 0)
            //            Channel1Enveloppe--;
            //    }
            //}
        }
        //Tick frequency sweep internal counter
        if ((FrameSequencerClock & 2) == 2)
        {

        }

        FrameSequencerClock = (FrameSequencerClock + 1) % 8;
    }
}
