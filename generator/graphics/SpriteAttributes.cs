using System;

namespace emulator
{
    public record SpriteAttributes(byte Y, byte X, byte ID, byte Flags)
    {
        public bool SpriteToBackgroundPriority => Flags.GetBit(7);
        public bool YFlipped => Flags.GetBit(6);
        public bool XFlipped => Flags.GetBit(5);
        public int Palette => Convert.ToInt32(Flags.GetBit(4));
    }
}
