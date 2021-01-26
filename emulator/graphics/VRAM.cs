namespace emulator
{
    public class VRAM
    {
        private readonly byte[] mem;

        public const int Start = 0x8000;
        public const int Size = 0x2000;
        public bool Locked = false;

        public VRAM()
        {
            mem = new byte[Size];
            for (int i = 0; i < Size; i++) mem[i] = 0x00;
        }
        public byte this[int n]
        {
            get => mem[n - Start];
            set => mem[n - Start] = value;
        }
    }
    public class UnusableMEM
    {
        private readonly byte[] mem;

        public const int Start = 0xfea0;
        public const int Size = 0x60;
        public UnusableMEM()
        {
            mem = new byte[Size];
            for (int i = 0; i < Size; i++) mem[i] = 0xff;
        }
        public byte this[int n]
        {
            get => mem[n - Start];
            set => mem[n - Start] = value;
        }
    }
}


