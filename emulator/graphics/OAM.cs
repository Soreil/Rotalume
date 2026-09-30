namespace emulator.graphics;

public class OAM
{
    private readonly SpriteAttributes[] sprites = new SpriteAttributes[Size / 4];

    public const int Start = 0xFE00;
    public const int Size = 0xa0;
    public bool Locked;

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
                0 => old with { Y = value },
                1 => old with { X = value },
                2 => old with { ID = value },
                3 => old with { Flags = value },
                _ => throw new NotImplementedException(),
            };
        }
    }

    private const int maxSpritesOnLine = 10;

    //Check if the sprite is on the current line. The sprite's Y position is offset by 16,
    //so we need to subtract that from the line number to get the actual Y position of the sprite.
    //The sprite's X position is also offset by 8,
    //so we need to subtract that from the line number to get the actual X position of the sprite.
    private static bool OnLine(SpriteAttributes s, int line, int spriteHeight) =>
        (s.Y + spriteHeight) > GraphicConstants.DoubleSpriteHeight &&
        s.Y < GraphicConstants.ScreenWidth &&
        s.X != 0 &&
        s.X < GraphicConstants.ScreenWidth + GraphicConstants.SpriteWidth &&
        line >= s.Y - GraphicConstants.DoubleSpriteHeight &&
        line < s.Y - GraphicConstants.DoubleSpriteHeight + spriteHeight;

    //Sprites are accessed sequentially. The only check if the sprite overlaps the current line's Y position
    //Only 10 sprites can be used per line
    public int SpritesOnLine(Span<SpriteAttributes> buffer, int line, int spriteHeight)
    {
        var data = sprites
            .Where(s => OnLine(s, line, spriteHeight))
            .Take(maxSpritesOnLine)
            .ToArray();

        Array.Sort(data);

        data.CopyTo(buffer);

        return data.Length;
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
