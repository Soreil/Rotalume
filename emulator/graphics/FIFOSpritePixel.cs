namespace emulator.graphics;

public readonly record struct FIFOSpritePixel
     (int Palette,
     byte Color,
     bool Priority);