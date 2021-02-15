﻿using System.Collections.Generic;

namespace emulator
{
    internal class ROMONLY : MBC
    {
        private readonly byte[] gameROM;
        private readonly byte[] _RAMBanks;

        public ROMONLY(byte[] gameROM)
        {
            this.gameROM = gameROM;
            _RAMBanks = new byte[0x2000];
        }

        public override byte this[int n]
        {
            get => n >= RAMStart && n < RAMStart + RAMSize ? _RAMBanks[n - RAMStart] : gameROM[n];
            set
            {
                if (n >= RAMStart && n < RAMStart + RAMSize) _RAMBanks[n - RAMStart] = value;
            }
        }
    }
}