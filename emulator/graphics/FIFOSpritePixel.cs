namespace emulator;

public readonly record struct FIFOSpritePixel

     (int Palette,
     byte color,
     bool priority);