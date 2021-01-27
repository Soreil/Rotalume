using System.Collections.Generic;
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
        const int ROMBankSize = 0x4000;
        int RAMBankSize = RAMSize;

        int lowBank => GetLowBankNumber();

        //This can return 0/20/40/60h
        private int GetLowBankNumber() => BankingMode == 1 ? (UpperBitsOfROMBank << 5) & (ROMBankCount - 1) : 0;

        private int HighBank() => (LowerBitsOfROMBank | (UpperBitsOfROMBank << 5)) & (ROMBankCount - 1);
        int highBank => HighBank();

        int ramBank => RAMBankCount == 1 ? 0 : (BankingMode == 1 ? UpperBitsOfROMBank : 0);
        int RAMBankCount;
        int ROMBankCount;

        int LowerBitsOfROMBank = 1;
        int UpperBitsOfROMBank = 0;
        int BankingMode = 0;
        public MBC1(CartHeader header, byte[] gameROM)
        {
            this.gameROM = gameROM;
            ROMBankCount = this.gameROM.Length / 0x4000;
            if (header.Type == CartType.MBC1_RAM && header.RAM_Size == 0) header = header with { RAM_Size = 0x2000 };
            RAMBankCount = Math.Max(1, header.RAM_Size / RAMBankSize);
            RAMBanks = new byte[header.RAM_Size];

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0)
                RAMBankSize = 0;

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0x800)
                RAMBankSize = 0x800;
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
        private byte ReadLowBank(int n) => gameROM[lowBank * ROMBankSize + n];
        private byte ReadHighBank(int n) => gameROM[highBank * ROMBankSize + (n - ROMBankSize)];

        private bool IsUpperBank(int n) => n >= ROMBankSize;

        public byte GetRAM(int n) => RAMEnabled ? RAMBanks[(ramBank * RAMBankSize) + n - RAMStart] : 0xff;
        public byte SetRAM(int n, byte v) => RAMEnabled ? RAMBanks[(ramBank * RAMBankSize) + n - RAMStart] = v : _ = v;
    }
}