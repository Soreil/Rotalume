using System.Collections.Generic;

namespace emulator
{
    internal class ROMONLY : MBC
    {
        private readonly CartHeader header;
        private readonly List<byte> gameROM;

        public ROMONLY(CartHeader header, List<byte> gameROM)
        {
            this.header = header;
            this.gameROM = gameROM;
        }

        public override byte this[int n] { get => gameROM[n]; set => _ = value; }
    }
}