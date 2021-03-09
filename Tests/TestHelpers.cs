using emulator;

using System;
using System.IO;


namespace Tests
{
    public static class TestHelpers
    {
        private static byte[] LoadGameROM() => File.ReadAllBytes(@"..\..\..\rom\Tetris (World) (Rev A).gb");
        private static byte[] LoadBootROM() => File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin");
        public static Core NewCore(byte[] bootrom = null, byte[] gamerom = null)
        {
            bootrom ??= LoadBootROM();
            gamerom ??= LoadGameROM();

            return new Core(gamerom,
                bootrom,
                new(new System.Collections.Concurrent.ConcurrentDictionary<Hardware.JoypadKey, bool>()),
                () => false, new(() => { }, () => { },
                    IntPtr.Zero,
                    false));
        }
        public static Core NewCore(byte[] gamerom)
        {
            byte[] gameromPaddedToSize;
            if (gamerom.Length < 0x8000)
            {
                gameromPaddedToSize = new byte[0x8000];
                gamerom.CopyTo(gameromPaddedToSize, 0x100);
            }
            else gameromPaddedToSize = gamerom;

            return new Core(gameromPaddedToSize,
                null,
                new(new System.Collections.Concurrent.ConcurrentDictionary<Hardware.JoypadKey, bool>()),
                () => false, new(() => { }, () => { },
                    IntPtr.Zero,
                    false));
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
