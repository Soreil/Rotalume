namespace emulator
{
    public class VRAM
    {
        private readonly byte[] mem;

        public const int Start = 0x8000;
        public const int Size = 0x2000;
        public bool Locked;

        public VRAM()
        {
            mem = new byte[Size];
            for (int i = 0; i < Size; i++)
            {
                mem[i] = 0x00;
            }
        }
        public byte this[int n]
        {
            get => mem[n - Start];
            set => mem[n - Start] = value;
        }
    }
}
