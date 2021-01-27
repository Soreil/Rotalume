using System.Collections.Generic;
using System;

namespace emulator
{
    internal class HalfRAM
    {
        byte[] _ram = new byte[0x200];
        public byte this[int at]
        {
            get => _ram[at & 0x1FF];
            set => _ram[at & 0x1FF] = (byte)(value & 0xf);
        }

        public HalfRAM()
        {
            for (int i = 0; i < _ram.Length; i++)
                _ram[i] = 0xf;
        }
    }
    internal class MBC2 : MBC
    {
        private readonly byte[] gameROM;

        private bool RAMEnabled = false;
        const int ROMBankSize = 0x4000;

        int _rombank = 1;
        int ROMBank { get => _rombank; set => _rombank = value == 0 ? 1 : value & (ROMBankCount - 1); }
        int ROMBankCount;

        HalfRAM RAM = new HalfRAM();

        public MBC2(CartHeader header, byte[] gameROM)
        {
            this.gameROM = gameROM;
            ROMBankCount = (this.gameROM.Length) / 0x4000;
        }

        public override byte this[int n]
        {
            get => n >= RAMStart ? GetRAM(n) : GetROM(n);
            set
            {
                switch (n)
                {
                    case var v when v < 0x4000:
                        if ((v & 0x100) == 0)
                            RAMEnabled = (value & 0xf) == 0x0a;
                        else
                            ROMBank = value & 0x0f;
                        break;
                    case var v when v >= RAMStart:
                        SetRAM(n, value);
                        break;
                }
            }
        }

        public byte GetROM(int n) => IsUpperBank(n) ? ReadHighBank(n) : ReadLowBank(n);
        private byte ReadLowBank(int n) => gameROM[n];
        private byte ReadHighBank(int n) => gameROM[(ROMBank * ROMBankSize) + (n - ROMBankSize)];

        private bool IsUpperBank(int n) => n >= ROMBankSize;

        public byte GetRAM(int n) => RAMEnabled ? (byte)(RAM[n - RAMStart] | 0xf0) : 0xff;
        public byte SetRAM(int n, byte v) => RAMEnabled ? RAM[n - RAMStart] = v : _ = v;
    }
}