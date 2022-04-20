namespace emulator;

internal class MBC1 : MBC
{
    private readonly byte[] gameROM;

    private bool RAMEnabled;
    private const int ROMBankSize = 0x4000;
    private readonly int RAMBankSize = RAMSize;

    private byte[] RAMBanks { get; }

    private int LowBank => GetLowBankNumber();

    //This can return 0/20/40/60h
    private int GetLowBankNumber() => BankingMode == 1 ? (UpperBitsOfROMBank << 5) & (ROMBankCount - 1) : 0;

    private int HighBank => (LowerBitsOfROMBank | (UpperBitsOfROMBank << 5)) & (ROMBankCount - 1);

    private int RamBank => RAMBankCount == 1 ? 0 : (BankingMode == 1 ? UpperBitsOfROMBank : 0);

    private readonly int RAMBankCount;
    private readonly int ROMBankCount;
    private int LowerBitsOfROMBank = 1;
    private int UpperBitsOfROMBank;
    private int BankingMode;
    public MBC1(CartHeader header, byte[] gameROM)
    {
        this.gameROM = gameROM;
        ROMBankCount = this.gameROM.Length / 0x4000;

        RAMBankCount = Math.Max(1, header.RAM_Size / RAMBankSize);
        RAMBankSize = Math.Min(header.RAM_Size, 0x2000);

        RAMBanks = new byte[Math.Max(0x2000, header.RAM_Size)];
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

    private byte GetROM(int n) => IsUpperBank(n) ? ReadHighBank(n) : ReadLowBank(n);

    private byte ReadLowBank(int n) => gameROM[LowBank * ROMBankSize + n];

    private byte ReadHighBank(int n) => gameROM[HighBank * ROMBankSize + (n - ROMBankSize)];

    private static bool IsUpperBank(int n) => n >= ROMBankSize;

    private byte GetRAM(int n) => (byte)(RAMEnabled ? RAMBanks[(RamBank * RAMBankSize) + n - RAMStart] : 0xff);

    private void SetRAM(int n, byte v)
    {
        if (RAMEnabled)
        {
            RAMBanks[(RamBank * RAMBankSize) + n - RAMStart] = v;
        }
    }
}
