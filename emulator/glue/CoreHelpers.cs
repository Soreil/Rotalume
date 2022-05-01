namespace emulator;

internal static class CoreHelpers
{
    internal static MBC MakeCard(GameROM gameROM, Keypad Keypad, IFrameSink frameSink, MasterClock masterclock)
    {
        var Header = new CartHeader(gameROM.ROM);

        MBC Card;
        if (Header.HasBattery())
        {
            var mmf = Header.MakeMemoryMappedFile(gameROM.FileName);
            Card = Header.HasClock() ? Header.MakeMBC(gameROM.ROM, mmf, masterclock) : Header.MakeMBC(gameROM.ROM, mmf);
        }
        else
        {
            Card = Header.MakeMBC(gameROM.ROM);
        }

        //Writing out the RTC too often would be very heavy. This writes it out once per frame.
        //
        if (Header.Type == CartType.MBC3_TIMER_RAM_BATTERY)
        {
            var SaveRTC = ((MBC3)Card).SaveRTC();

            void h(object? x, EventArgs y) => SaveRTC();
            frameSink.FramePushed += h;
        }

        if (Card is MBC5WithRumble rumble)
        {
            rumble.RumbleStateChange += Keypad.ToggleRumble;
        }

        return Card;
    }
}