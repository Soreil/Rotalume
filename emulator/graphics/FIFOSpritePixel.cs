namespace emulator;

public readonly struct FIFOSpritePixel
{
    public readonly int Palette;
    public readonly byte color;
    public readonly bool priority;

    public FIFOSpritePixel(byte paletteIndex, bool spriteToBackgroundPriority, int palette)
    {
        color = paletteIndex;
        priority = spriteToBackgroundPriority;
        Palette = palette;
    }
}
