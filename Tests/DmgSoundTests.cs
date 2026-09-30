using NUnit.Framework;

namespace Tests;

internal class DmgSoundTests
{
    [TestCase("01-registers")]
    [TestCase("02-len_ctr")]
    [TestCase("03-trigger")]
    [TestCase("04-sweep")]
    [TestCase("05-sweep_details")]
    [TestCase("06-overflow_on_trigger")]
    [TestCase("07-len_sweep_period_sync")]
    [TestCase("08-len_ctr_during_power")]
    [TestCase("09-wave_read_while_on")]
    [TestCase("10-wave_trigger_while_on")]
    [TestCase("11-regs_after_power")]
    [TestCase("12-wave_write_while_on")]
    public void ROMReportsSuccess(string name)
    {
        var path = Path.Combine("rom", "blargg", "dmg_sound", name + ".gb");
        var render = new TestRenderDevice();
        using var core = TestHelpers.NewCore(File.ReadAllBytes(path), name, render);
        int frames = 0;
        bool finished = false;
        render.FramePushed += (_, _) =>
        {
            frames++;
            finished = core.Memory[0xa001] == 0xde && core.Memory[0xa002] == 0xb0 &&
                core.Memory[0xa003] == 0x61 && core.Memory[0xa000] != 0x80;
        };
        //Several sound ROMs take longer than 100 frames; 0x80 means still running, not failure.
        long deadline = core.Time() + 60L * 4_194_304;
        while (!finished && core.Time() < deadline) core.Step();

        var text = new System.Text.StringBuilder();
        for (int address = 0xa004; address < 0xc000; address++)
        {
            byte value = core.Memory[(ushort)address];
            if (value == 0) break;
            text.Append((char)value);
        }
        Assert.That(core.Memory[0xa001], Is.EqualTo(0xde), "Missing blargg result signature.");
        Assert.That(core.Memory[0xa002], Is.EqualTo(0xb0), "Missing blargg result signature.");
        Assert.That(core.Memory[0xa003], Is.EqualTo(0x61), "Missing blargg result signature.");
        Assert.That(core.Memory[0xa000], Is.Zero, text.ToString());
    }
}
