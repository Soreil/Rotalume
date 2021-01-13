using System.Collections.Generic;

namespace emulator
{
    public abstract class MBC
    {
        public const int Start = 0x0000;
        public const int Size = 0x8000;
        public abstract byte this[int n]
        {
            get;
            set;
        }
    }
}