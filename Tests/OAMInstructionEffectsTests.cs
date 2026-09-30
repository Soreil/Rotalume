using System.Text;
using emulator.graphics;
using ImageMagick;
using NUnit.Framework;

namespace Tests;

internal class OAMInstructionEffectsTests
{
    [Test]
    public void InstructionEffectsROM() => CheckROM(@"rom\blargg\oam_bug\8-instr_effect.gb");

    [TestCase("1-lcd_sync")]
    [TestCase("2-causes")]
    [TestCase("3-non_causes")]
    [TestCase("4-scanline_timing")]
    [TestCase("5-timing_bug")]
    [TestCase("6-timing_no_bug")]
    public void RelatedOAMROMs(string name) =>
        CheckROM($@"rom\blargg\oam_bug\{name}.gb");

    private static void CheckROM(string romPath, int frameLimit = 100)
    {
        var render = new TestRenderDevice();
        using var core = TestHelpers.NewCore(File.ReadAllBytes(romPath), Path.GetFileNameWithoutExtension(romPath), render);
        int frames = 0;
        render.FramePushed += (_, _) => frames++;
        // The pattern dumps spend substantial time with the LCD disabled.
        long deadline = core.Time() + 20L * frameLimit * GraphicConstants.TicksPerFrame;
        int instructions = 0;
        while (frames < frameLimit && core.Time() < deadline && instructions++ < frameLimit * 200_000)
            core.Step();

        var text = new StringBuilder();
        for (ushort address = 0xa004; address < 0xc000 && core.Memory[address] != 0; address++)
            text = text.Append((char)core.Memory[address]);
        var status = core.Memory[0xa000];
        Assert.That(frames, Is.EqualTo(frameLimit),
            $"ROM exceeded its execution limit: frames={frames}, ticks={core.Time()}, " +
            $"PC={core.CPU.PC:X4}, LCDC={core.Memory[0xff40]:X2}, status={status:X2}.\n{text}");
        Assert.That(status, Is.Zero, text.ToString());
        using var expected = new MagickImage(Path.ChangeExtension(romPath, ".png"));
        using var actual = new MagickImage(render.Image, new MagickReadSettings
        {
            Width = 160,
            Height = 144,
            Format = MagickFormat.Gray
        });
        Assert.That(ImageComparer.AreImagesEqual(expected, actual), Is.True, text.ToString());
    }
}
