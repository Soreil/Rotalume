using System;

namespace emulator
{
    public struct SpriteAttributes
    {
        public bool SpriteToBackgroundPriority => Flags.GetBit(7);
        public bool YFlipped => Flags.GetBit(6);
        public bool XFlipped => Flags.GetBit(5);
        public int Palette => Convert.ToInt32(Flags.GetBit(4));

        public byte Y;
        public byte X;
        public byte ID;
        public byte Flags;
        public SpriteAttributes(byte Y, byte X, byte ID, byte Flags)
        {
            this.Y = Y;
            this.X = X;
            this.ID = ID;
            this.Flags = Flags;
        }
    }
}
