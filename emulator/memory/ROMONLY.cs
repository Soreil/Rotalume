using System.Collections.Generic;

namespace emulator
{
    internal class ROMONLY : MBC
    {
        private readonly byte[] gameROM;

        public ROMONLY(byte[] gameROM)
        {
            this.gameROM = gameROM;
            RAMBanks = new byte[0x2000];
        }

        public override byte this[int n]
        {
            get => n >= RAMStart && n < RAMStart + RAMSize ? RAMBanks[n - RAMStart] : gameROM[n];
            set
            {
                if (n >= RAMStart && n < RAMStart + RAMSize) RAMBanks[n - RAMStart] = value;
            }
        }
    }
}