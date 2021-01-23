using System.Collections.Generic;

namespace emulator
{
    internal class ROMONLY : MBC
    {
        private readonly byte[] gameROM;
        private byte[] EXT_RAM;

        public ROMONLY(byte[] gameROM)
        {
            this.gameROM = gameROM;
            EXT_RAM = new byte[0x2000];
        }

        public override byte this[int n]
        {
            get => n >= RAMStart && n < RAMStart + RAMSize ? EXT_RAM[n - RAMStart] : gameROM[n];
            set
            {
                if (n >= RAMStart && n < RAMStart + RAMSize) EXT_RAM[n - RAMStart] = value;
            }
        }
    }
}