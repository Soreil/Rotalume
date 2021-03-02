using System;

namespace emulator
{
    public class APU
    {
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
        const int baseClock = 1 << 22;

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
        private bool Sound1OnEnabled = false;
        private bool Sound2OnEnabled = false;
        private bool Sound3OnEnabled = false;
        private bool Sound4OnEnabled = false;

        public byte NR23 { get; internal set; }
        public byte NR31 { get; internal set; }
        public byte NR33 { get; internal set; }
        public byte NR41 { get; internal set; }
        public byte[] Wave { get; internal set; } = new byte[0x10];

        private Func<long> Clock;
        public APU(Func<long> clock)
        {
            Clock = clock;
        }
    }
}