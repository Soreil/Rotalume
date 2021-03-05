using System;

namespace emulator
{
    //TODO: Add heuristic detection of multicart ROMs.

    /*From Gameboy Complete Technical Reference by Gekkio:

    Detecting multicarts
    MBC1 multicarts are not detectable by simply looking at the ROM header, because the ROM type value is just
    one of the normal MBC1 values. However, detection is possible by going through BANK2 values and looking
    at "bank 0" of each multicart game and doing some heuristics based on the header data. All the included
    games, including the game selection menu, have proper header data. One example of a good heuristic is logo
    data verification.
    So, if you have a 8 Mbit cart with MBC1, first assume that it’s a multicart and bank numbers are 6-bit
    values. Set BANK1 to zero and loop through the four possible BANK2 values while checking the data at
    0x0104-0x0133. In other words, check logo data starting from physical ROM locations 0x00104, 0x40104,
    0x80104, and 0xC0104. If proper logo data exists with most of the BANK2 values, the cart is most likely a
    multicart. Note that multicarts can just have two actual games, so one of the locations might not have the
    header data in place.
    */
    internal class MBC1 : MBC
    {
        private readonly byte[] gameROM;

        private bool RAMEnabled = false;
        private const int ROMBankSize = 0x4000;
        private readonly int RAMBankSize = RAMSize;

        private int LowBank => GetLowBankNumber();

        //This can return 0/20/40/60h
        private int GetLowBankNumber() => BankingMode == 1 ? (UpperBitsOfROMBank << 5) & (ROMBankCount - 1) : 0;

        private int HighBank => (LowerBitsOfROMBank | (UpperBitsOfROMBank << 5)) & (ROMBankCount - 1);

        private int RamBank => RAMBankCount == 1 ? 0 : (BankingMode == 1 ? UpperBitsOfROMBank : 0);

        private readonly int RAMBankCount;
        private readonly int ROMBankCount;
        private int LowerBitsOfROMBank = 1;
        private int UpperBitsOfROMBank = 0;
        private int BankingMode = 0;
        public MBC1(CartHeader header, byte[] gameROM, System.IO.MemoryMappedFiles.MemoryMappedFile? file = null)
        {
            this.gameROM = gameROM;
            ROMBankCount = this.gameROM.Length / 0x4000;

            RAMBankCount = Math.Max(1, header.RAM_Size / RAMBankSize);
            RAMBankSize = Math.Min(header.RAM_Size, 0x2000);

            if (header.RAM_Size != 0)
            {
                RAMBanks = file!.CreateViewAccessor(0, header.RAM_Size);
            }
        }

        public override byte this[int n]
        {
            get => n >= RAMStart ? GetRAM(n) : GetROM(n);
            set
            {
                switch (n)
                {
                    case var v when v < 0x2000:
                    RAMEnabled = (value & 0x0F) == 0x0A;
                    break;
                    case var v when v < 0x4000:
                    LowerBitsOfROMBank = (value & 0x1f) == 0 ? 1 : value & 0x1f; //0x1f should be parameterizable depending on if it's multicart
                    break;
                    case var v when v < 0x6000:
                    UpperBitsOfROMBank = value & 0x03;
                    break;
                    case var v when v < 0x8000:
                    BankingMode = value & 0x01;
                    break;
                    default:
                    SetRAM(n, value);
                    break;
                }
            }
        }

        public byte GetROM(int n) => IsUpperBank(n) ? ReadHighBank(n) : ReadLowBank(n);

        private byte ReadLowBank(int n) => gameROM[LowBank * ROMBankSize + n];

        private byte ReadHighBank(int n) => gameROM[HighBank * ROMBankSize + (n - ROMBankSize)];

        private static bool IsUpperBank(int n) => n >= ROMBankSize;

        public byte GetRAM(int n) => (byte)(RAMEnabled ? RAMBanks!.ReadByte((RamBank * RAMBankSize) + n - RAMStart) : 0xff);

        public void SetRAM(int n, byte v)
        {
            if (RAMEnabled)
            {
                RAMBanks!.Write((RamBank * RAMBankSize) + n - RAMStart, v);
            }
        }
    }
}