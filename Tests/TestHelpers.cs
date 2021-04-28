using emulator;

using System;
using System.IO;


namespace Tests
{
    public static class TestHelpers
    {
        public const string ROMpath = @"..\..\..\rom\";
        public const string ROMResultPath = @"..\..\..\expected_rom_result\";
        private static byte[] LoadGameROM() => File.ReadAllBytes(ROMpath + @"Tetris (World) (Rev A).gb");
        private static byte[] LoadBootROM() => File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin");
        public static Core NewBootCore(FrameSink? frameSink = null)
        {
            var bootrom = LoadBootROM();
            var gamerom = LoadGameROM();
            frameSink ??= new FrameSink(() => IntPtr.Zero, () => { }, false);

            return new Core(
                gamerom,
                bootrom,
                new(new InputDevices(new MockGameController(), new())),
                frameSink);
        }
        public static Core NewCore(byte[] gamerom, FrameSink? frameSink = null)
        {
            byte[] gameromPaddedToSize;
            if (gamerom.Length < 0x8000)
            {
                gameromPaddedToSize = new byte[0x8000];
                gamerom.CopyTo(gameromPaddedToSize, 0x100);
            }
            else gameromPaddedToSize = gamerom;

            frameSink ??= new FrameSink(() => IntPtr.Zero, () => { }, false);

            return new Core(gameromPaddedToSize,
                null,
                new(new InputDevices(new MockGameController(), new())),
                frameSink);
        }

        public static void StepOneCPUInstruction(Core c)
        {
            c.Step();
            while (c.CPU.TicksWeAreWaitingFor != 1)
            {
                c.Step();
            }
        }
    }
}
