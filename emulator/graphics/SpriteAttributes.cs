
namespace emulator.graphics;

public readonly record struct SpriteAttributes(
    byte Y,
    byte X,
    byte ID,
    byte Flags) : IComparable<SpriteAttributes>
{
    public readonly bool SpriteToBackgroundPriority => Flags.GetBit(7);
    public readonly bool YFlipped => Flags.GetBit(6);
    public readonly bool XFlipped => Flags.GetBit(5);
    public readonly byte Palette => Convert.ToByte(Flags.GetBit(4));

    readonly int IComparable<SpriteAttributes>.CompareTo(SpriteAttributes other) => X.CompareTo(other.X);
}
