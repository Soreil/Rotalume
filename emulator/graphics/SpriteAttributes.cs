
namespace emulator;

public record struct SpriteAttributes(

    byte Y,
    byte X,
    byte ID,
    byte Flags) : IComparable<SpriteAttributes>
{
    public bool SpriteToBackgroundPriority => Flags.GetBit(7);
    public bool YFlipped => Flags.GetBit(6);
    public bool XFlipped => Flags.GetBit(5);
    public byte Palette => Convert.ToByte(Flags.GetBit(4));

    int IComparable<SpriteAttributes>.CompareTo(SpriteAttributes other) => X.CompareTo(other.X);
}
