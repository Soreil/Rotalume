using emulator.glue;
using emulator.graphics;
using emulator.input;

using ImageMagick;

using Microsoft.Extensions.Logging.Abstractions;

namespace Tests;

public static class TestHelpers
{
    private static byte[] LoadGameROM() => File.ReadAllBytes(@"rom\Tetris (World) (Rev A).gb");
    private static byte[] LoadBootROM() => File.ReadAllBytes(@"rom\dmg_rom.bin");
    public static Core NewBootCore(IFrameSink? frameSink = null)
    {
        var bootrom = LoadBootROM();
        var gamerom = LoadGameROM();

        var logger = NullLogger<FrameSink>.Instance;
        frameSink ??= new FrameSink(() => false, logger);

        return new Core(
            gamerom,
            bootrom,
            "tetrisTest",
            new(new InputDevices(new MockGameController(), [])),
            frameSink);
    }

    public static Core NewCore(byte[] gamerom, IFrameSink? frameSink = null) => NewCore(gamerom, "", frameSink);

    public static Core NewCore(byte[] gamerom, string fileName, IFrameSink? frameSink = null)
    {
        byte[] gameromPaddedToSize;
        if (gamerom.Length < 0x8000)
        {
            gameromPaddedToSize = new byte[0x8000];
            gamerom.CopyTo(gameromPaddedToSize, 0x100);
        }
        else gameromPaddedToSize = gamerom;

        var logger = NullLogger<FrameSink>.Instance;
        frameSink ??= new FrameSink(() => false, logger);

        return new Core(gameromPaddedToSize,
            null,
            fileName,
            new(new InputDevices(new MockGameController(), [])),
            frameSink);
    }

    public static void StepOneCPUInstruction(Core c) =>
        c.Step();//while (c.CPU.TicksWeAreWaitingFor != 1)//{//    c.Step();//}
}

public static class ImageComparer
{
    public static bool AreImagesEqual(MagickImage image1, MagickImage image2)
    {
        var error = image1.Compare(image2, ErrorMetric.Absolute, Channels.All);
        return error == 0;
    }
}