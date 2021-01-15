﻿using System.Collections.Generic;

namespace emulator
{
    internal class ROMONLY : MBC
    {
        private readonly List<byte> gameROM;

        public ROMONLY(List<byte> gameROM)
        {
            this.gameROM = gameROM;
        }

        public override byte this[int n] { get => n > RAMStart && n < RAMStart + RAMSize ? 0xff : gameROM[n]; set => _ = value; }
    }
}