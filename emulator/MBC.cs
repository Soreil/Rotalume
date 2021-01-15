using System.Collections.Generic;

namespace emulator
{
    public abstract class MBC
    {
        public const int ROMStart = 0x0000;
        public const int ROMSize = 0x8000;
        public const int RAMStart = 0xa000;
        public const int RAMSize = 0x2000;
        public abstract byte this[int n]
        {
            get;
            set;
        }
    }
}