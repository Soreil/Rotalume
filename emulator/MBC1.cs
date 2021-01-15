using System.Collections.Generic;
using System;

namespace emulator
{
    //MBC1 does not currently do multicart detection and as such won't work correctly since multicarts have different wiring
    internal class MBC1 : MBC
    {
        private readonly List<byte> gameROM;
        private readonly List<byte[]> RAMBanks;

        private bool RAMEnabled = false;
        const int ROMBankSize = 0x4000;
        int RAMBankSize = RAMSize;

        int lowBank => BankingMode == 0 ? 0 : GetLowBankNumber();

        //This can return 0/20/40/60h
        private int GetLowBankNumber() => UpperBitsOfROMBank * 0x20;

        //We should really be masking here but maybe just checking if it stays in bounds is sufficient.
        //5 should be parameterizable depending on if it's a multicart rom or not.
        private int HighBank() => Math.Min(ROMBankCount, (UpperBitsOfROMBank << 5) + LowerBitsOfROMBank);
        int highBank => (HighBank() & 0x0F) == 0 ? HighBank() + 1 : HighBank();

        int ramBank => RAMBankCount == 1 ? 0 : (BankingMode == 1 ? UpperBitsOfROMBank : 0);
        int RAMBankCount;
        int ROMBankCount;

        int LowerBitsOfROMBank = 0;
        int UpperBitsOfROMBank = 0;
        int BankingMode = 0;
        public MBC1(CartHeader header, List<byte> gameROM)
        {
            this.gameROM = gameROM;
            ROMBankCount = this.gameROM.Count / 0x4000;

            RAMBankCount = Math.Max(1, header.RAM_Size / RAMBankSize);
            RAMBanks = new List<byte[]>(RAMBankCount);

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0)
                RAMBankSize = 0;

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0x800)
                RAMBankSize = 0x800;

            for (int i = 0; i < RAMBankCount; i++)
                RAMBanks.Add(new byte[RAMBankSize]);
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
                        LowerBitsOfROMBank = value & 0x1f; //0x1f should be parameterizable depending on if it's multicart
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
        private byte ReadHighBank(int n) => gameROM[highBank * ROMBankSize + n];
        private bool IsUpperBank(int n) => n >= highBank * ROMBankSize;

        public byte GetRAM(int n) => RAMBanks[ramBank][n - RAMStart];
        public byte SetRAM(int n, byte v) => RAMBanks[ramBank][n - RAMStart] = v;
    }
}