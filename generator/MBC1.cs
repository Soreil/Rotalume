using System.Collections.Generic;

namespace emulator
{
    internal class MBC1 : MBC
    {
        private CartHeader header;
        private List<byte> gameROM;

        const int bankSize = 0x4000;
        int lowBankAddr = 0;
        int highBankAddr = bankSize;
        public MBC1(CartHeader header, List<byte> gameROM)
        {
            this.header = header;
            this.gameROM = gameROM;
        }

        public override byte this[int n] { 
            get => n>=bankSize ? gameROM[highBankAddr+n-bankSize] : gameROM[lowBankAddr+n]; 
            set; }
    }
}