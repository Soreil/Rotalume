
namespace emulator.graphics;

public readonly record struct SpriteAttributes(
    byte Y,
    byte X,
    byte ID,
    byte Flags) : IComparable<SpriteAttributes>
{
    public bool SpriteToBackgroundPriority => (Flags & 0x80) != 0;
    public bool YFlipped => (Flags & 0x40) != 0;
    public bool XFlipped => (Flags & 0x20) != 0;
    public byte Palette => (byte)((Flags & 0x10) >> 4);

    public int CompareTo(SpriteAttributes other) => X.CompareTo(other.X);
}
