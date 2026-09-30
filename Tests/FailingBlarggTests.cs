using NUnit.Framework;

namespace Tests;

internal class FailingBlarggTests
{
    [TestCase(@"rom\blargg\interrupt_time.gb")]
    [TestCase(@"rom\blargg\cpu_instrs\11-op_a,(hl).gb")]
    [TestCase(@"rom\blargg\dmg_sound\01-registers.gb")]
    [TestCase(@"rom\blargg\dmg_sound\02-len_ctr.gb")]
    [TestCase(@"rom\blargg\dmg_sound\03-trigger.gb")]
    [TestCase(@"rom\blargg\dmg_sound\04-sweep.gb")]
    [TestCase(@"rom\blargg\dmg_sound\05-sweep_details.gb")]
    [TestCase(@"rom\blargg\dmg_sound\06-overflow_on_trigger.gb")]
    [TestCase(@"rom\blargg\dmg_sound\07-len_sweep_period_sync.gb")]
    [TestCase(@"rom\blargg\dmg_sound\08-len_ctr_during_power.gb")]
    [TestCase(@"rom\blargg\dmg_sound\09-wave_read_while_on.gb")]
    [TestCase(@"rom\blargg\dmg_sound\10-wave_trigger_while_on.gb")]
    [TestCase(@"rom\blargg\dmg_sound\11-regs_after_power.gb")]
    [TestCase(@"rom\blargg\dmg_sound\12-wave_write_while_on.gb")]
    [TestCase(@"rom\blargg\oam_bug\4-scanline_timing.gb")]
    [TestCase(@"rom\blargg\oam_bug\6-timing_no_bug.gb")]
    [TestCase(@"rom\blargg\oam_bug\8-instr_effect.gb")]
    public void TestFrameMatchesExpectedFrame(string romPath)
    {
        var imagePath = Path.ChangeExtension(romPath, ".png");
        var outputFile = Path.Combine(Path.GetDirectoryName(romPath)!, Path.GetFileNameWithoutExtension(romPath) + "_output.bmp");
        //The longer sound ROMs are still running at frame 100. Keep exact image comparison,
        //but allow up to one emulated minute; the helper exits as soon as the image matches.
        int frameLimit = romPath.Contains(@"blargg\dmg_sound\", StringComparison.OrdinalIgnoreCase) ? 3600 : 100;
        var (frame, output, success) = GraphicalOutputTestHelpers.FrameMatchesExpectedFrame(romPath, imagePath, outputFile, frameLimit);

        Assert.That(success, Is.True,
            $"Tested {romPath}. Images did not match after {frame} frames. Wrote debug image to {output}");
    }
}
