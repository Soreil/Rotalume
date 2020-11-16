namespace generator
{
    public class VRAM
    {
        private readonly byte[] mem;

        public const int Start = 0x8000;
        public const int Size = 0x2000;
        public VRAM() => mem = new byte[Size];
        public byte this[int n]
        {
            get => mem[n - Start];
            set => mem[n - Start] = value;
        }
    }
}
