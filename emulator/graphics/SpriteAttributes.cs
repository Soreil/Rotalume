using System.Runtime.InteropServices;

namespace emulator;

[StructLayout(LayoutKind.Sequential)]
public readonly struct SpriteAttributes : IComparable<SpriteAttributes>
{
    public readonly byte Y;
    public readonly byte X;
    public readonly byte ID;
    public readonly byte Flags;

    public readonly byte Palette;
    public readonly bool SpriteToBackgroundPriority;
    public readonly bool YFlipped;
    public readonly bool XFlipped;
    public SpriteAttributes(byte Y, byte X, byte ID, byte Flags)
    {
        this.Y = Y;
        this.X = X;
        this.ID = ID;
        this.Flags = Flags;

        SpriteToBackgroundPriority = Flags.GetBit(7);
        YFlipped = Flags.GetBit(6);
        XFlipped = Flags.GetBit(5);
        Palette = Convert.ToByte(Flags.GetBit(4));

    }

    int IComparable<SpriteAttributes>.CompareTo(SpriteAttributes other) => X.CompareTo(other.X);
}
