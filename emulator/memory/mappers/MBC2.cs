﻿using System.IO.MemoryMappedFiles;

namespace emulator.memory.mappers;
internal class HalfRAM : IDisposable
{
    private readonly MemoryMappedViewAccessor _ram;
    private bool disposedValue;

    public byte this[int at]
    {
        get => _ram.ReadByte(at & 0x1FF);
        set => _ram.Write(at & 0x1FF, (byte)(value & 0xf));
    }

    public HalfRAM(MemoryMappedViewAccessor v) => _ram = v;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _ram.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~HalfRAM()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
internal class MBC2 : MBC
{
    private readonly byte[] gameROM;

    private bool RAMEnabled;
    private const int ROMBankSize = 0x4000;
    private int _rombank = 1;

    private int ROMBank { get => _rombank; set => _rombank = value == 0 ? 1 : value & (ROMBankCount - 1); }

    private readonly int ROMBankCount;


    private readonly HalfRAM RAM;


    public MBC2(byte[] gameROM, MemoryMappedFile file)
    {
        this.gameROM = gameROM;
        ROMBankCount = this.gameROM.Length / 0x4000;
        var RAMBanks = file.CreateViewAccessor(0, 0x2000);
        RAM = new(RAMBanks);
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
                {
                    RAMEnabled = (value & 0xf) == 0x0a;
                }
                else
                {
                    ROMBank = value & 0x0f;
                }

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

    private static bool IsUpperBank(int n) => n >= ROMBankSize;

    public byte GetRAM(int n) => (byte)(RAMEnabled ? RAM[n - RAMStart] | 0xf0 : 0xff);

    public void SetRAM(int n, byte v)
    {
        if (RAMEnabled) RAM[n - RAMStart] = v;
    }

    public override void Dispose()
    {
        if (RAM is not null)
            RAM.Dispose();
    }
}
