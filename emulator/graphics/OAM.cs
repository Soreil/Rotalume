using System;

namespace emulator
{
    public class OAM
    {
        private readonly SpriteAttributes[] sprites;

        public const int Start = 0xFE00;
        public const int Size = 0xa0;
        public bool Locked = false;

        public OAM()
        {
            sprites = new SpriteAttributes[Size / 4];
        }

        public byte this[int n]
        {
            get => ((byte)((n - Start) % 4) switch
            {
                0 => sprites[(n - Start) / 4].Y,
                1 => sprites[(n - Start) / 4].X,
                2 => sprites[(n - Start) / 4].ID,
                3 => sprites[(n - Start) / 4].Flags,
            });
            set
            {
                var old = sprites[(n - Start) / 4];

                sprites[(n - Start) / 4] = ((n - Start) % 4) switch
                {
                    0 => new(value, old.X, old.ID, old.Flags),
                    1 => new(old.Y, value, old.ID, old.Flags),
                    2 => new(old.Y, old.X, value, old.Flags),
                    3 => new(old.Y, old.X, old.ID, value),
                };
            }
        }

        const int maxSpritesOnLine = 10;

        private static bool OnLine(SpriteAttributes s, int line, int spriteHeight) =>
                (s.Y + spriteHeight) > 16 &&
                s.Y < 160 &&
                s.X != 0 &&
                s.X < 168 &&
                line >= s.Y - 16 &&
                line < s.Y - 16 + spriteHeight;
        public int SpritesOnLine(SpriteAttributes[] buffer, int line, int spriteHeight)
        {
            int spriteCount = 0;
            foreach (var s in sprites)
            {
                if (OnLine(s, line, spriteHeight))
                {
                    buffer[spriteCount++] = s;
                    if (spriteCount == maxSpritesOnLine) break;
                }
            }
            Order(buffer, spriteCount);
            return spriteCount;
        }

        private static void Order(SpriteAttributes[] buffer, int count) => Array.Sort(buffer, 0, count);
    }
}
