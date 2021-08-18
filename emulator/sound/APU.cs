namespace emulator
{
    public class APU
    {
        public (Action<byte> Write, Func<byte> Read)[][] HookUpSound() => new (Action<byte> Write, Func<byte> Read)[][] {
            new (Action<byte> Write, Func<byte> Read)[] {
            ((x) => NR10 = x,
            () => NR10),
            ((x) => NR11 = x,
            () => NR11),
            ((x) => NR12 = x,
            () => NR12),
            ((x) => NR13 = x,
            () => NR13),
            ((x) => NR14 = x,
            () => NR14),
            }, new (Action<byte> Write, Func<byte> Read)[]{
            ((x) => NR21 = x,
            () => NR21),
            ((x) => NR22 = x,
            () => NR22),
            ((x) => NR23 = x,
            () => NR23),
            ((x) => NR24 = x,
            () => NR24),
            ((x) => NR30 = x,
            () => NR30),
            ((x) => NR31 = x,
            () => NR31),
            ((x) => NR32 = x,
            () => NR32),
            ((x) => NR33 = x,
            () => NR33),
            ((x) => NR34 = x,
            () => NR34),
            }, new (Action<byte> Write, Func<byte> Read)[]{
            ((x) => NR41 = x,
            () => NR41),
            ((x) => NR42 = x,
            () => NR42),
            ((x) => NR43 = x,
            () => NR43),
            ((x) => NR44 = x,
            () => NR44),
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


        private byte _nr10 = 0xff;
        public byte NR10
        {
            get => (byte)(_nr10 & 0x7f | 0x80);
            set => _nr10 = value;
        }

        internal void SetStateWithoutBootrom()
        {
            NR10 = 0x80;
            NR11 = 0xB0;
            NR12 = 0xf3;
            NR14 = 0xbf;
            NR21 = 0x3f;
            NR22 = 0x00;
            NR23 = 0xff;
            NR24 = 0xbf;
            NR30 = 0x7f;
            NR31 = 0xff;
            NR32 = 0x9f;
            NR33 = 0xff;
            NR34 = 0xbf;
            NR42 = 0x00;
            NR43 = 0x00;
            NR44 = 0xbf;
            NR50 = 0x77;
            NR51 = 0xf3;
            NR52 = 0xf1;
        }

        private byte _nr11 = 0xff;
        private byte NR11
        {
            get => (byte)(_nr11 & 0xc0 | 0x3f);
            set => _nr11 = value;
        }

        private byte NR12
        {
            get => (byte)(Channel1Enveloppe << 4 |
                (Channel1EnveloppeIncreasing ? 1 : 0) << 3 |
                Channel1EnveloppeSweepNumber);
            set
            {
                Channel1Enveloppe = (byte)(value >> 4);
                Channel1EnveloppeIncreasing = value.GetBit(3);
                Channel1EnveloppeSweepNumber = value & 0x07;
            }
        }

        private byte NR13 = 0xff;

        private byte _nr14 = 0xff;
        private byte NR14
        {
            get => (byte)(_nr14 & 0x40 | 0xbf);
            set
            {
                _nr14 = value;
                if (_nr14.GetBit(7))
                {
                    Sound1OnEnabled = true;
                    if ((NR11 & 0x3f) == 0) NR11 = (byte)((NR11 & 0xC0) | 0x3f);
                }
            }
        }

        private byte _nr21 = 0xff;
        private byte NR21
        {
            get => (byte)(_nr21 & 0xb0 | 0x3f);
            set => _nr21 = value;
        }

        private byte NR22 = 0xff;

        private byte _nr24 = 0xff;
        private byte NR24
        {
            get => (byte)(_nr24 & 0x40 | 0xbf);
            set => _nr24 = value;
        }

        private byte _nr30 = 0xff;
        private byte NR30
        {
            get => (byte)(_nr30 & 0x80 | 0x7f);
            set => _nr30 = value;
        }
        private byte _nr32 = 0xff;
        private byte NR32
        {
            get => (byte)(_nr32 & 0x60 | 0x9f);
            set => _nr32 = value;
        }
        private byte _nr34 = 0xff;
        private byte NR34
        {
            get => (byte)(_nr34 & 0x40 | 0xbf);
            set => _nr34 = value;
        }

        private byte _nr41 = 0xff;
        private byte NR41
        {
            get => (byte)(_nr41 & 0x3f | 0xc0);
            set => _nr41 = value;
        }


        private byte NR42 = 0xff;

        private byte NR43 = 0xff;

        private byte _nr44 = 0xff;
        private byte NR44
        {
            get => (byte)(_nr44 & 0x40 | 0xbf);
            set => _nr44 = value;
        }

        private byte NR50 = 0xff;

        private byte NR51 = 0xff;

        private byte _nr52 = 0xf1;
        private byte NR52
        {
            get
            {
                byte channels = (byte)(((Sound1OnEnabled ? 1 : 0) << 3) |
                                ((Sound2OnEnabled ? 1 : 0) << 2) |
                                ((Sound3OnEnabled ? 1 : 0) << 1) |
                                ((Sound4OnEnabled ? 1 : 0) << 0));

                return (byte)(_nr52 | channels);
            }
            set => _nr52 = (byte)(value & 0x80 | (_nr52 & 0x7f));
        }

        //We should have this available as a namespace wide thing somehow
        private const int baseClock = cpu.Constants.Frequency;

        //All of the Channel 1 fields
        private int Channel1SweepTime => ((NR10 & 0x70) >> 4) * baseClock / 128;
        private bool Channel1SweepIncreasing => NR10.GetBit(3);
        private int Channel1SweepShifts => NR10 & 0x07;

        private double Channel1DutyCycle => ((NR11 & 0xc0) >> 6) switch
        {
            0 => 1 / 8d,
            1 => 2 / 8d,
            2 => 4 / 8d,
            3 => 6 / 8d,
            _ => throw new NotImplementedException(),
        };

        private bool Channel1EnveloppeIncreasing;
        private int Channel1EnveloppeSweepNumber;
        private int Channel1Frequency => NR13 | ((NR14 & 0x3) << 8);
        private bool Channel1Initial => NR14.GetBit(7);
        private bool Channel1Counter => NR14.GetBit(6);

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
        private bool Sound1OnEnabled = false;
        private bool Sound2OnEnabled = false;
        private bool Sound3OnEnabled = false;
        private bool Sound4OnEnabled = false;

        private byte NR23 { get; set; }
        private byte NR31 { get; set; }
        private byte NR33 { get; set; }
        private byte[] Wave { get; set; } = new byte[0x10];

        private int _sampleCount;
        public int SampleCount
        {
            get => _sampleCount;
            set => _sampleCount = (_sampleCount + value) % Samples.Length;
        }
        private readonly float[] Samples;
        public APU(int sampleRate)
        {
            TicksPerSample = baseClock / sampleRate;
            Samples = new float[sampleRate];

            if (TicksPerSample * sampleRate != baseClock)
            {
                throw new Exception("We want a sample rate which is evenly divisible in to the base clock");
            }
        }

        private int APUClock;
        private readonly int TicksPerSample;

        private const int FrameSequencerFrequency = baseClock / 512;
        internal void Tick()
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

        private float SampleSound() => 0.0f;

        private int FrameSequencerClock;
        private byte Channel1Enveloppe;
        private void TickFrameSequencer()
        {
            //Length counter
            //We have to disable the channel when NRx1 ticks to 0
            //We should only be accessing the lower 6 bits of these channels, the top bits
            //are used for a different function
            if ((FrameSequencerClock & 1) == 0)
            {
                if ((NR11 & 0x3f) > 0) NR11 = (byte)((NR11 & 0xc0) | ((NR11 & 0x3f) - 1));
                if ((NR21 & 0x3f) > 0) NR21 = (byte)((NR21 & 0xc0) | ((NR21 & 0x3f) - 1));
                if (NR31 > 0) NR31--; //NR31 uses the full byte for the length counter
                if ((NR41 & 0x3f) > 0) NR41 = (byte)((NR41 & 0xc0) | ((NR41 & 0x3f) - 1));

                if ((NR11 & 0x3f) == 0) Sound1OnEnabled = false;
                if ((NR21 & 0x3f) == 0) Sound2OnEnabled = false;
                if (NR31 == 0) Sound3OnEnabled = false;
                if ((NR41 & 0x3f) == 0) Sound4OnEnabled = false;
            }

            //Tick volume envelope internal counter
            if (FrameSequencerClock == 7)
            {
                //Sweep until we have done the requested number of sweeps
                if (Channel1EnveloppeSweepNumber != 0)
                {
                    Channel1EnveloppeSweepNumber--;

                    if (Channel1EnveloppeIncreasing)
                    {
                        //If we are not maxed out yet, increase
                        if (Channel1Enveloppe != 0xf)
                            Channel1Enveloppe++;
                    }
                    else
                    {
                        //If we are not bottomed out yet, decrease
                        if (Channel1Enveloppe != 0)
                            Channel1Enveloppe--;
                    }
                }
            }
            //Tick frequency sweep internal counter
            if ((FrameSequencerClock & 2) == 2)
            {

            }

            FrameSequencerClock = (FrameSequencerClock + 1) % 8;
        }
    }
}