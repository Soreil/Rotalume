namespace Tests
{
    public class OAM
    {
        private readonly byte[] mem;

        public OAM() => mem = new byte[0x100];

        public SpriteAttributes Entry(int n) => new SpriteAttributes(mem[n * 4], mem[n * 4 + 1], mem[n * 4 + 2], mem[n * 4 + 3]);
        public byte this[int n]
        {
            get => mem[n];
            set => mem[n] = value;
        }
    }
}
