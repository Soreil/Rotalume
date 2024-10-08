﻿namespace emulator.graphics;

public class OAM
{
    private readonly SpriteAttributes[] sprites;

    public const int Start = 0xFE00;
    public const int Size = 0xa0;
    public bool Locked;

    public OAM() => sprites = new SpriteAttributes[Size / 4];

    public byte this[int n]
    {
        get => (byte)((n - Start) % 4) switch
        {
            0 => sprites[(n - Start) / 4].Y,
            1 => sprites[(n - Start) / 4].X,
            2 => sprites[(n - Start) / 4].ID,
            3 => sprites[(n - Start) / 4].Flags,
            _ => throw new NotImplementedException(),
        };
        set
        {
            var old = sprites[(n - Start) / 4];

            sprites[(n - Start) / 4] = ((n - Start) % 4) switch
            {
                0 => new(value, old.X, old.ID, old.Flags),
                1 => new(old.Y, value, old.ID, old.Flags),
                2 => new(old.Y, old.X, value, old.Flags),
                3 => new(old.Y, old.X, old.ID, value),
                _ => throw new NotImplementedException(),
            };
        }
    }

    private const int maxSpritesOnLine = 10;

    private static bool OnLine(SpriteAttributes s, int line, int spriteHeight) => (s.Y + spriteHeight) > GraphicConstants.DoubleSpriteHeight &&
s.Y < GraphicConstants.ScreenWidth &&
s.X != 0 &&
s.X < GraphicConstants.ScreenWidth + GraphicConstants.SpriteWidth &&
line >= s.Y - GraphicConstants.DoubleSpriteHeight &&
line < s.Y - GraphicConstants.DoubleSpriteHeight + spriteHeight;

    //Sprites are accessed sequentially. The only check if the sprite overlaps the current line's Y position
    //Only 10 sprites can be used per line
    public int SpritesOnLine(Span<SpriteAttributes> buffer, int line, int spriteHeight)
    {
        int spriteCount = 0;
        foreach (var s in sprites)
        {
            if (OnLine(s, line, spriteHeight))
            {
                buffer[spriteCount++] = s;
                if (spriteCount == maxSpritesOnLine)
                {
                    break;
                }
            }
        }
        var selected = buffer[..spriteCount];
        selected.Sort();
        return spriteCount;
    }

    //TODO: actual proper corruption algorithm
    internal void Corrupt(object? sender, EventArgs e)
    {
        for (int i = 0; i < Size; i++)
        {
            this[i + Start] = (byte)i;
        }
    }
}
