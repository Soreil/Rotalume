namespace emulator
{
    public abstract class MBC
    {
        protected const int ROMStart = 0x0000;
        protected const int ROMSize = 0x8000;
        protected const int RAMStart = 0xa000;
        protected const int RAMSize = 0x2000;
        public abstract byte this[int n]
        {
            get;
            set;
        }
    }
}