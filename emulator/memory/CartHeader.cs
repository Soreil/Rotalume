using System;
using System.Collections.Generic;

namespace emulator
{
    public enum CartType : byte
    {
        ROM_ONLY = 0x00,
        MBC1 = 0x01,
        MBC1_RAM = 0x02,
        MBC1_RAM_BATTERY = 0x03,
        MBC2 = 0x05,
        MBC2_BATTERY = 0x06,
        ROM_RAM = 0x08,
        ROM_RAM_BATTERY = 0x09,
        MMM01 = 0x0B,
        MMM01_RAM = 0x0C,
        MMM01_RAM_BATTERY = 0x0D,
        MBC3_TIMER_BATTERY = 0x0F,
        MBC3_TIMER_RAM_BATTERY = 0x10,
        MBC3 = 0x11,
        MBC3_RAM = 0x12,
        MBC3_RAM_BATTERY = 0x13,
        MBC5 = 0x19,
        MBC5_RAM = 0x1A,
        MBC5_RAM_BATTERY = 0x1B,
        MBC5_RUMBLE = 0x1C,
        MBC5_RUMBLE_RAM = 0x1D,
        MBC5_RUMBLE_RAM_BATTERY = 0x1E,
        MBC6 = 0x20,
        MBC7_SENSOR_RUMBLE_RAM_BATTERY = 0x22,
        POCKET_CAMERA = 0xFC,
        BANDAI_TAMA5 = 0xFD,
        HuC3 = 0xFE,
        HuC1_RAM_BATTERY = 0xFF,
    }
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
                t[i] = gameROM[0x134 + i] == '\0' ? ' ' : (char)gameROM[0x134 + i];
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
    }
}