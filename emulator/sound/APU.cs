using System;

namespace emulator
{
    public class APU
    {
        public void HookUpSound(ControlRegister controlRegisters)
        {
            controlRegisters.Writer[0x10] = (x) => NR10 = x;
            controlRegisters.Reader[0x10] = () => NR10;
            controlRegisters.Writer[0x11] = (x) => NR11 = x;
            controlRegisters.Reader[0x11] = () => NR11;
            controlRegisters.Writer[0x12] = (x) => NR12 = x;
            controlRegisters.Reader[0x12] = () => NR12;
            controlRegisters.Writer[0x13] = (x) => NR13 = x;
            controlRegisters.Reader[0x13] = () => NR13;
            controlRegisters.Writer[0x14] = (x) => NR14 = x;
            controlRegisters.Reader[0x14] = () => NR14;
            controlRegisters.Writer[0x16] = (x) => NR21 = x;
            controlRegisters.Reader[0x16] = () => NR21;
            controlRegisters.Writer[0x17] = (x) => NR22 = x;
            controlRegisters.Reader[0x17] = () => NR22;
            controlRegisters.Writer[0x18] = (x) => NR23 = x;
            controlRegisters.Reader[0x18] = () => NR23;
            controlRegisters.Writer[0x19] = (x) => NR24 = x;
            controlRegisters.Reader[0x19] = () => NR24;
            controlRegisters.Writer[0x1a] = (x) => NR30 = x;
            controlRegisters.Reader[0x1a] = () => NR30;
            controlRegisters.Writer[0x1b] = (x) => NR31 = x;
            controlRegisters.Reader[0x1b] = () => NR31;
            controlRegisters.Writer[0x1c] = (x) => NR32 = x;
            controlRegisters.Reader[0x1c] = () => NR32;
            controlRegisters.Writer[0x1d] = (x) => NR33 = x;
            controlRegisters.Reader[0x1d] = () => NR33;
            controlRegisters.Writer[0x1e] = (x) => NR34 = x;
            controlRegisters.Reader[0x1e] = () => NR34;
            controlRegisters.Writer[0x20] = (x) => NR41 = x;
            controlRegisters.Reader[0x20] = () => NR41;
            controlRegisters.Writer[0x21] = (x) => NR42 = x;
            controlRegisters.Reader[0x21] = () => NR42;
            controlRegisters.Writer[0x22] = (x) => NR43 = x;
            controlRegisters.Reader[0x22] = () => NR43;
            controlRegisters.Writer[0x23] = (x) => NR44 = x;
            controlRegisters.Reader[0x23] = () => NR44;
            controlRegisters.Writer[0x24] = (x) => NR50 = x;
            controlRegisters.Reader[0x24] = () => NR50;
            controlRegisters.Writer[0x25] = (x) => NR51 = x;
            controlRegisters.Reader[0x25] = () => NR51;
            controlRegisters.Writer[0x26] = (x) => NR52 = x;
            controlRegisters.Reader[0x26] = () => NR52;

            controlRegisters.Writer[0x30] = (x) => Wave[0] = x;
            controlRegisters.Reader[0x30] = () => Wave[0];
            controlRegisters.Writer[0x31] = (x) => Wave[1] = x;
            controlRegisters.Reader[0x31] = () => Wave[1];
            controlRegisters.Writer[0x32] = (x) => Wave[2] = x;
            controlRegisters.Reader[0x32] = () => Wave[2];
            controlRegisters.Writer[0x33] = (x) => Wave[3] = x;
            controlRegisters.Reader[0x33] = () => Wave[3];
            controlRegisters.Writer[0x34] = (x) => Wave[4] = x;
            controlRegisters.Reader[0x34] = () => Wave[4];
            controlRegisters.Writer[0x35] = (x) => Wave[5] = x;
            controlRegisters.Reader[0x35] = () => Wave[5];
            controlRegisters.Writer[0x36] = (x) => Wave[6] = x;
            controlRegisters.Reader[0x36] = () => Wave[6];
            controlRegisters.Writer[0x37] = (x) => Wave[7] = x;
            controlRegisters.Reader[0x37] = () => Wave[7];
            controlRegisters.Writer[0x38] = (x) => Wave[8] = x;
            controlRegisters.Reader[0x38] = () => Wave[8];
            controlRegisters.Writer[0x39] = (x) => Wave[9] = x;
            controlRegisters.Reader[0x39] = () => Wave[9];
            controlRegisters.Writer[0x3a] = (x) => Wave[10] = x;
            controlRegisters.Reader[0x3a] = () => Wave[10];
            controlRegisters.Writer[0x3b] = (x) => Wave[11] = x;
            controlRegisters.Reader[0x3b] = () => Wave[11];
            controlRegisters.Writer[0x3c] = (x) => Wave[12] = x;
            controlRegisters.Reader[0x3c] = () => Wave[12];
            controlRegisters.Writer[0x3d] = (x) => Wave[13] = x;
            controlRegisters.Reader[0x3d] = () => Wave[13];
            controlRegisters.Writer[0x3e] = (x) => Wave[14] = x;
            controlRegisters.Reader[0x3e] = () => Wave[14];
            controlRegisters.Writer[0x3f] = (x) => Wave[15] = x;
            controlRegisters.Reader[0x3f] = () => Wave[15];
        }

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
        public byte NR41 { get; internal set; }
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

        private int APUClock { get; set; }
        public int TicksPerSample { get; }

        const int FrameSequencerFrequency = baseClock / 512;
        internal void Tick()
        {
            if (APUClock % TicksPerSample == 0)
            {
                SampleCount++;
            }
            if (APUClock == FrameSequencerFrequency)
            {
                TickFrameSequencer();
                APUClock = 0;
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