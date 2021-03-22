using System;

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

        private byte _nr11 = 0xff;
        public byte NR11
        {
            get => (byte)(_nr11 & 0xc0 | 0x3f);
            set => _nr11 = value;
        }
        public byte NR12 = 0xff;

        public byte NR13 = 0xff;

        private byte _nr14 = 0xff;
        public byte NR14
        {
            get => (byte)(_nr14 & 0x40 | 0xbf);
            set => _nr14 = value;
        }

        private byte _nr21 = 0xff;
        public byte NR21
        {
            get => (byte)(_nr21 & 0xb0 | 0x3f);
            set => _nr21 = value;
        }

        public byte NR22 = 0xff;

        private byte _nr24 = 0xff;
        public byte NR24
        {
            get => (byte)(_nr24 & 0x40 | 0xbf);
            set => _nr24 = value;
        }

        private byte _nr30 = 0xff;
        public byte NR30
        {
            get => (byte)(_nr30 & 0x80 | 0x7f);
            set => _nr30 = value;
        }
        private byte _nr32 = 0xff;
        public byte NR32
        {
            get => (byte)(_nr32 & 0x60 | 0x9f);
            set => _nr32 = value;
        }
        private byte _nr34 = 0xff;
        public byte NR34
        {
            get => (byte)(_nr34 & 0x40 | 0xbf);
            set => _nr34 = value;
        }

        private byte _nr41 = 0xff;
        public byte NR41
        {
            get => (byte)(_nr41 & 0x3f | 0xc0);
            set => _nr41 = value;
        }


        public byte NR42 = 0xff;

        public byte NR43 = 0xff;

        private byte _nr44 = 0xff;
        public byte NR44
        {
            get => (byte)(_nr44 & 0x40 | 0xbf);
            set => _nr44 = value;
        }

        public byte NR50 = 0xff;

        public byte NR51 = 0xff;

        private byte _nr52 = 0xf1;
        public byte NR52
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
        private const int baseClock = 1 << 22;

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
        private TimeSpan Channel1SoundLength => TimeSpan.FromSeconds((64 - (NR11 & 0x3f)) * (1 / 256d));

        private double Channel1InitialEnveloppeVolume => ((NR12 & 0xf0) >> 4) * (1 / 15d);
        private bool Channel1EnveloppeIncreasing => NR12.GetBit(3);
        private int Channel1EnveloppeSweepNumber => NR12 & 0x07;
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
        private readonly bool Sound1OnEnabled = false;
        private readonly bool Sound2OnEnabled = false;
        private readonly bool Sound3OnEnabled = false;
        private readonly bool Sound4OnEnabled = false;

        public byte NR23 { get; internal set; }
        public byte NR31 { get; internal set; }
        public byte NR33 { get; internal set; }
        public byte[] Wave { get; internal set; } = new byte[0x10];
        public int SampleCount { get; internal set; }
        public APU(int sampleRate)
        {
            TicksPerSample = baseClock / sampleRate;

            if (TicksPerSample * sampleRate != baseClock)
            {
                throw new Exception("We want a sample rate which is evenly divisible in to the base clock");
            }
        }

        private int APUClock;
        private readonly int TicksPerSample;

        const int FrameSequencerFrequency = baseClock / 512;
        internal void Tick()
        {
            if (((byte)APUClock) == TicksPerSample)
            {
                SampleCount++;
                if (APUClock == FrameSequencerFrequency)
                {
                    TickFrameSequencer();
                    APUClock = 0;
                }
            }
            APUClock++;
        }

        private byte FrameSequencerClock;
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
            }
            //Tick volume envelope internal counter
            if (FrameSequencerClock == 7)
            {

            }
            //Tick frequency sweep internal counter
            if ((FrameSequencerClock & 2) == 2)
            {

            }

            FrameSequencerClock = (byte)((FrameSequencerClock + 1) % 8);
        }
    }
}