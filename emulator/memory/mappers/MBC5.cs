using System;

namespace emulator
{
    internal class MBC5 : MBC
    {
        private readonly byte[] gameROM;

        private bool RAMEnabled = false;
        private const int ROMBankSize = 0x4000;
        private readonly int RAMBankSize = RAMSize;
        private readonly int RAMBankCount;
        private readonly int ROMBankCount;
        private int ROMBankNumber = 1;
        private int RAMBankNumber = 0;
        public MBC5(CartHeader header, byte[] gameROM, System.IO.MemoryMappedFiles.MemoryMappedFile file)
        {
            this.gameROM = gameROM;
            ROMBankCount = this.gameROM.Length / 0x4000;
            RAMBankCount = Math.Max(1, header.RAM_Size / RAMBankSize);
            RAMBanks = file.CreateViewAccessor(0, header.RAM_Size);

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0)
            {
                RAMBankSize = 0;
            }

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0x800)
            {
                RAMBankSize = 0x800;
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
                    RAMEnabled = value == 0x0A;
                    break;
                    case var v when v < 0x3000:
                    ROMBankNumber = (ROMBankNumber & 0x100) | (value & (ROMBankCount - 1));
                    break;
                    case var v when v < 0x4000:
                    if (ROMBankCount >= 0x100)
                    {
                        ROMBankNumber = (ROMBankNumber & 0xff) | ((value & 0x1) << 8);
                    }

                    break;
                    case var v when v < 0x6000:
                    RAMBankNumber = (value & 0xf) & (RAMBankCount - 1);
                    break;
                    case var v when v >= RAMStart:
                    SetRAM(n, value);
                    break;
                }
            }
        }

        public byte GetROM(int n) => IsUpperBank(n) ? ReadHighBank(n) : ReadLowBank(n);

        private byte ReadLowBank(int n) => gameROM[n];

        private byte ReadHighBank(int n) => gameROM[ROMBankNumber * ROMBankSize + (n - ROMBankSize)];

        private static bool IsUpperBank(int n) => n >= ROMBankSize;

        public byte GetRAM(int n) => (byte)(RAMEnabled ? RAMBanks!.ReadByte((RAMBankNumber * RAMBankSize) + n - RAMStart) : 0xff);

        public void SetRAM(int n, byte v)
        {
            if (RAMEnabled)
            {
                RAMBanks!.Write((RAMBankNumber * RAMBankSize) + n - RAMStart, v);
            }
        }

    }
}