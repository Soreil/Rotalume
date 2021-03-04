using System;

namespace emulator
{
    public readonly struct SpriteAttributes : IComparable<SpriteAttributes>
    {
        public bool SpriteToBackgroundPriority => Flags.GetBit(7);
        public bool YFlipped => Flags.GetBit(6);
        public bool XFlipped => Flags.GetBit(5);
        public int Palette => Convert.ToInt32(Flags.GetBit(4));

        public readonly byte Y;
        public readonly byte X;
        public readonly byte ID;
        public readonly byte Flags;
        public SpriteAttributes(byte Y, byte X, byte ID, byte Flags)
        {
            this.Y = Y;
            this.X = X;
            this.ID = ID;
            this.Flags = Flags;
        }

        public int CompareTo(SpriteAttributes obj) => X.CompareTo(obj.X);
    }
}
