using System;

namespace generator
{
    public record SpriteAttributes(byte Y, byte X, byte ID, byte Flags)
    {
        bool SpriteToBackgroundPriority => Flags.GetBit(7);
        bool YFlipped => Flags.GetBit(6);
        bool XFlipped => Flags.GetBit(5);
        int Palette => Convert.ToInt32(Flags.GetBit(4));
    }
}
