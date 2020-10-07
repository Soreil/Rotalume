using System.Collections.Generic;
using System.Linq;

namespace generator
{
    public class OAM
    {
        private readonly byte[] mem;

        public OAM() => mem = new byte[0xa0];

        private SpriteAttributes Entry(int n) => new SpriteAttributes(mem[n * 4], mem[n * 4 + 1], mem[n * 4 + 2], mem[n * 4 + 3]);
        private IEnumerable<SpriteAttributes> Entries()
        {
            List<SpriteAttributes> res = new List<SpriteAttributes>(mem.Length / 4);
            for (int i = 0; i < res.Count; i++)
                res[i] = Entry(i);
            return res;
        }
        public byte this[int n]
        {
            get => mem[n];
            set => mem[n] = value;
        }

        const int maxSpritesOnLine = 10;

        public List<SpriteAttributes> SpritesOnLine(int line) => Entries().Where(x => x.X == line).Take(maxSpritesOnLine).ToList();
    }
}
