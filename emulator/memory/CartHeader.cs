using emulator.glue;
using emulator.memory.mappers;

using System.Security.Cryptography;

namespace emulator.memory;

internal record CartHeader
{
    public string Title { get; init; }
    public CartType Type { get; init; }

    private static readonly string SaveFormatExtension = ".sav";

    private static int ROM_Size_Mapping(byte b) => b switch
    {
        0x00 => 32 * 1024,
        0x01 => 64 * 1024,
        0x02 => 128 * 1024,
        0x03 => 256 * 1024,
        0x04 => 512 * 1024,
        0x05 => 1024 * 1024,
        0x06 => 2048 * 1024,
        0x07 => 4096 * 1024,
        0x08 => 8192 * 1024,
        0x52 => 1152 * 1024,
        0x53 => 1280 * 1024,
        0x54 => 1536 * 1024,
        _ => throw new UnexpectedSize("Non standard ROM size")
    };

    private static int RAM_Size_Mapping(byte b) => b switch
    {
        0x00 => 0,
        0x01 => 2048,
        0x02 => 8196,
        0x03 => 32768,
        0x04 => 131072,
        0x05 => 65536,
        _ => throw new UnexpectedSize("Non standard RAM size")
    };

    public int ROM_Size { get; init; }
    public int RAM_Size { get; init; }

    internal bool HasRumble() => Type switch
    {
        CartType.MBC5_RUMBLE => true,
        CartType.MBC5_RUMBLE_RAM => true,
        CartType.MBC5_RUMBLE_RAM_BATTERY => true,
        _ => false
    };

    public CartHeader(ReadOnlySpan<byte> gameROM)
    {
        var titleArea = gameROM.Slice(0x134, 16);
        Span<char> t = stackalloc char[16];

        for (int i = 0; i < titleArea.Length; i++)
            t[i] = titleArea[i] == '\0' ? ' ' : (char)titleArea[i];

        Title = new string(t).Trim();
        Type = (CartType)gameROM[0x147];
        ROM_Size = ROM_Size_Mapping(gameROM[0x148]);
        RAM_Size = RAM_Size_Mapping(gameROM[0x149]);

        using var hash = SHA256.Create();
    }

    internal bool HasBattery() => Type switch
    {
        CartType.MBC1_RAM_BATTERY => true,
        CartType.MBC2_BATTERY => true,
        CartType.ROM_RAM_BATTERY => true,
        CartType.MMM01_RAM_BATTERY => true,
        CartType.MBC3_TIMER_BATTERY => true,
        CartType.MBC3_TIMER_RAM_BATTERY => true,
        CartType.MBC3_RAM_BATTERY => true,
        CartType.MBC5_RAM_BATTERY => true,
        CartType.MBC5_RUMBLE_RAM_BATTERY => true,
        CartType.MBC7_SENSOR_RUMBLE_RAM_BATTERY => true,
        CartType.HuC1_RAM_BATTERY => true,
        _ => false,
    };

    internal bool HasClock() => Type switch
    {
        CartType.MBC3_TIMER_BATTERY => true,
        CartType.MBC3_TIMER_RAM_BATTERY => true,
        _ => false,
    };

    public MBC MakeMBC(byte[] gameROM, System.IO.MemoryMappedFiles.MemoryMappedFile file, MasterClock clock) => Type switch
    {
        CartType.MBC3_TIMER_BATTERY => new MBC3(this, gameROM, file, clock),
        CartType.MBC3_TIMER_RAM_BATTERY => new MBC3(this, gameROM, file, clock),
        _ => throw new NotImplementedException(),
    };

    public MBC MakeMBC(byte[] gameROM, System.IO.MemoryMappedFiles.MemoryMappedFile file) => Type switch
    {
        CartType.MBC1_RAM_BATTERY => new MBC1WithBatteryBackedRAM(this, gameROM, file),
        CartType.MBC2 => new MBC2(gameROM, file),
        CartType.MBC2_BATTERY => new MBC2(gameROM, file),
        CartType.MBC3 => new MBC3(this, gameROM, file),
        CartType.MBC3_RAM => new MBC3(this, gameROM, file),
        CartType.MBC3_RAM_BATTERY => new MBC3(this, gameROM, file),
        CartType.MBC5_RAM => new MBC5(this, gameROM, file),
        CartType.MBC5_RAM_BATTERY => new MBC5(this, gameROM, file),
        CartType.MBC5_RUMBLE_RAM_BATTERY => new MBC5WithRumble(this, gameROM, file),
        CartType.MBC5_RUMBLE => new MBC5WithRumble(this, gameROM, file),
        CartType.MBC5_RUMBLE_RAM => new MBC5WithRumble(this, gameROM, file),
        _ => throw new NotImplementedException(),
    };

    public MBC MakeMBC(byte[] gameROM) => Type switch
    {
        CartType.ROM_ONLY => new ROMONLY(gameROM),
        CartType.MBC1 => new MBC1(this, gameROM),
        CartType.MBC1_RAM => new MBC1(this, gameROM),
        CartType.MBC5 => new MBC5(this, gameROM),
        _ => throw new NotImplementedException(),
    };

    public System.IO.MemoryMappedFiles.MemoryMappedFile MakeMemoryMappedFile(string fileName)
    {
        if (!HasBattery())
        {
            throw new NoBatteryPresentException("We need a battery for it to be relevant to keep a save file on disk");
        }

        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var RotalumeFolder = Path.Combine(root, "rotalume");
        var saveFolder = Path.Combine(RotalumeFolder, "saves");

        if (!Directory.Exists(saveFolder))
        {
            _ = Directory.CreateDirectory(saveFolder);
        }

        var path = Path.Combine(saveFolder, fileName + SaveFormatExtension);

        if (!File.Exists(path))
        {
            var size = RequiredSaveFileSize();
            var buffer = new byte[size];
            buffer.AsSpan().Fill(0xff);
            File.WriteAllBytes(path, buffer);
        }
        else if (new FileInfo(path).Length < RequiredSaveFileSize())
        {
            throw new FileLoadException("Existing save size too small");
        }

        System.IO.MemoryMappedFiles.MemoryMappedFile? res = null;

        while (res is null)
        {
            try
            {
                res = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(path);
            }
            catch (IOException)
            {
                Thread.Sleep(10);
            }
        }
        return res!;
    }

    private int RequiredSaveFileSize()
    {
        int size = RAM_Size != 0 ? RAM_Size : (Type == CartType.MBC2_BATTERY ? 0x2000 : 0);
        if (HasClock())
        {
            size += 16;
        }
        return size;
    }

    private static string SanitizeFilename(string title)
    {
        var invalids = new HashSet<char>(Path.GetInvalidFileNameChars());
        return new string(title.Where(x => !invalids.Contains(x)).ToArray());
    }
}
