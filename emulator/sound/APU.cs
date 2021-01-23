namespace emulator
{
    public class APU
    {
        public byte NR10 = 0xff;
        private byte _nr11 = 0xff;
        public byte NR11
        {
            get => (byte)(_nr11 & 0xb0 | 0x3f);
            set => _nr11 = value;
        }
        public byte NR12 = 0xff;

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
            get => _nr52;
            set => _nr52 = (byte)(value & 0x80 | (_nr52&0x7f));
        }

        public APU()
        {

        }
    }
}