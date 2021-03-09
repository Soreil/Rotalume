using System;

namespace emulator
{
    internal record CartHeader
    {
        public string Title { get; init; }
        public CartType Type { get; init; }

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
            _ => throw new Exception("Non standard ROM size")
        };

        private static int RAM_Size_Mapping(byte b) => b switch
        {
            0x00 => 0,
            0x01 => 2048,
            0x02 => 8196,
            0x03 => 32768,
            0x04 => 131072,
            0x05 => 65536,
            _ => throw new Exception("Non standard RAM size")
        };

        public int ROM_Size { get; init; }
        public int RAM_Size { get; init; }
        public CartHeader(byte[] gameROM)
        {
            char[] t = new char[16];
            for (int i = 0; i < 16; i++)
            {
                t[i] = gameROM[0x134 + i] == '\0' ? ' ' : (char)gameROM[0x134 + i];
            }

            Title = new string(t);

            Type = (CartType)gameROM[0x147];

            ROM_Size = ROM_Size_Mapping(gameROM[0x148]);
            RAM_Size = RAM_Size_Mapping(gameROM[0x149]);
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

        public MBC MakeMBC(byte[] gameROM, System.IO.MemoryMappedFiles.MemoryMappedFile? file, Func<long> clock) => Type switch
        {
            CartType.ROM_ONLY => new ROMONLY(gameROM),
            CartType.MBC1 => new MBC1(this, gameROM),
            CartType.MBC1_RAM => new MBC1(this, gameROM),
            CartType.MBC1_RAM_BATTERY => new MBC1(this, gameROM, file),
            CartType.MBC2 => new MBC2(gameROM, file!),
            CartType.MBC2_BATTERY => new MBC2(gameROM, file!),
            CartType.MBC3 => new MBC3(this, gameROM, file!),
            CartType.MBC3_RAM => new MBC3(this, gameROM, file!),
            CartType.MBC3_RAM_BATTERY => new MBC3(this, gameROM, file!),
            CartType.MBC3_TIMER_BATTERY => new MBC3(this, gameROM, file!, clock),
            CartType.MBC3_TIMER_RAM_BATTERY => new MBC3(this, gameROM, file!, clock),
            CartType.MBC5 => new MBC5(this, gameROM, file!),
            CartType.MBC5_RAM => new MBC5(this, gameROM, file!),
            CartType.MBC5_RAM_BATTERY => new MBC5(this, gameROM, file!),
            _ => throw new NotImplementedException(),
        };

        public System.IO.MemoryMappedFiles.MemoryMappedFile? MakeMemoryMappedFile()
        {
            //A cartridge requires a battery in order to be able to keep state while the system is off
            if (!HasBattery())
            {
                return null;
            }

            //This retrieves %appdata% path
            var root = Environment.GetEnvironmentVariable("AppData") + "\\rotalume";
            if (!System.IO.Directory.Exists(root))
            {
                _ = System.IO.Directory.CreateDirectory(root);
            }

            //Filenames might be somewhat illegal depending on what characters are in the title?
            var path = string.Format(@"{0}\{1}.sav", root, Title);
            if (!System.IO.File.Exists(path))
            {
                int size = 0;
                if (RAM_Size != 0)
                {
                    size += RAM_Size;
                }
                //MBC2 does not report a size in the header but instead has a fixed 2k internal RAM
                else if (Type == CartType.MBC2_BATTERY)
                {
                    size += 0x2000;
                }
                //16 bytes to store clock should be plenty
                if (HasClock())
                {
                    size += 16;
                }

                var buffer = new byte[size];
                for (int i = 0; i < size; i++)
                {
                    buffer[i] = 0xff;
                }

                System.IO.File.WriteAllBytes(path, buffer);
            }
            return System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(path);
        }


    }
}