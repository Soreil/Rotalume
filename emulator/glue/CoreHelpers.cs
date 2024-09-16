using emulator.graphics;
using emulator.input;
using emulator.memory;
using emulator.memory.mappers;

namespace emulator.glue;

internal static class CoreHelpers
{
    internal static MBC MakeCard(GameROM gameROM, Keypad keypad, IFrameSink frameSink, MasterClock masterClock)
    {
        var header = new CartHeader(gameROM.ROM);

        MBC card;
        if (header.HasBattery())
        {
            var mmf = header.MakeMemoryMappedFile(gameROM.FileName);
            card = header.HasClock() ? header.MakeMBC(gameROM.ROM, mmf, masterClock) : header.MakeMBC(gameROM.ROM, mmf);
        }
        else
        {
            card = header.MakeMBC(gameROM.ROM);
        }

        // Writing out the RTC too often would be very heavy. This writes it out once per frame.
        if (header.Type == CartType.MBC3_TIMER_RAM_BATTERY)
        {
            var saveRTC = ((MBC3)card).SaveRTC();
            frameSink.FramePushed += (x, y) => saveRTC();
        }

        if (card is MBC5WithRumble rumble)
        {
            rumble.RumbleStateChange += keypad.ToggleRumble;
        }

        return card;
    }
}
