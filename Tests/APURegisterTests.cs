using emulator.sound;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Tests;

internal class APURegisterTests
{
    [Test]
    public void WaveRAMIsInaccessibleImmediatelyAfterTrigger()
    {
        using var core = TestHelpers.NewCore(new byte[0x8000]);
        core.Memory[0xff1a] = 0;
        core.Memory[0xff30] = 0x12;
        core.Memory[0xff1a] = 0x80;
        core.Memory[0xff1d] = 0;
        core.Memory[0xff1e] = 0x80;
        Assert.That(core.Memory[0xff30], Is.EqualTo(0xff));
        core.Memory[0xff30] = 0x34;
        core.Memory[0xff1a] = 0;
        Assert.That(core.Memory[0xff30], Is.EqualTo(0x12));
    }

    [Test]
    public void SweepTriggerOverflowDisablesChannel()
    {
        var apu = new APU(NullLogger<APU>.Instance);
        apu[Address.NR52] = 0x80;
        apu[Address.NR12] = 0xf0;
        apu[Address.NR10] = 0x01; // Addition, shift 1: 2047 + 1023 overflows immediately.
        apu[Address.NR13] = 0xff;
        apu[Address.NR14] = 0x87;
        Assert.That(apu[Address.NR52] & 1, Is.Zero);
    }

    [TestCase(Address.NR11, Address.NR12, Address.NR14, 0x01)]
    [TestCase(Address.NR21, Address.NR22, Address.NR24, 0x02)]
    public void PowerOffClearsDutyButPreservesLength(Address duty, Address envelope, Address trigger, int statusBit)
    {
        var apu = new APU(NullLogger<APU>.Instance);
        apu[Address.NR52] = 0x80;
        apu[duty] = 0xff; // Duty 3 and a length of one.
        apu[Address.NR52] = 0;
        Assert.That(apu[duty], Is.EqualTo(0x3f));

        apu[Address.NR52] = 0x80;
        apu[envelope] = 0xf0;
        apu[trigger] = 0xc0;
        Assert.That(apu[Address.NR52] & statusBit, Is.EqualTo(statusBit));
        apu.FrameSequencerClock(null, EventArgs.Empty);
        Assert.That(apu[Address.NR52] & statusBit, Is.Zero);

        apu[Address.NR52] = 0;
        apu[duty] = 0xbf; // Only the length bits are writable with power off.
        Assert.That(apu[duty], Is.EqualTo(0x3f));
    }

    [TestCase(Address.NR14, 0x01)]
    [TestCase(Address.NR24, 0x02)]
    [TestCase(Address.NR34, 0x04)]
    [TestCase(Address.NR44, 0x08)]
    public void TriggerWithDACOffDoesNotEnableChannel(Address trigger, int statusBit)
    {
        var apu = new APU(NullLogger<APU>.Instance);
        apu[Address.NR52] = 0x80;

        apu[trigger] = 0x80;

        Assert.That(apu[Address.NR52] & statusBit, Is.Zero);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void WaveRAMIsAddressableAfterDACIsDisabled(bool retrigger)
    {
        var apu = new APU(NullLogger<APU>.Instance);
        apu[Address.NR52] = 0x80;
        apu[Address.NR30] = 0x80;
        apu[Address.NR34] = 0x80;
        Assert.That(apu[Address.NR52] & 0x04, Is.EqualTo(0x04));

        apu[Address.NR30] = 0;
        if (retrigger) apu[Address.NR34] = 0x80;

        Assert.That(apu[Address.NR52] & 0x04, Is.Zero);
        for (int i = 0; i < 16; i++)
            apu[(Address)((int)Address.Wave0 + i)] = (byte)(0x80 + i);
        for (int i = 0; i < 16; i++)
            Assert.That(apu[(Address)((int)Address.Wave0 + i)], Is.EqualTo((byte)(0x80 + i)));

        apu[Address.NR30] = 0x80;
        Assert.That(apu[Address.NR52] & 0x04, Is.Zero, "Enabling the DAC must not restart the channel.");
        apu[Address.NR34] = 0x80;
        Assert.That(apu[Address.NR52] & 0x04, Is.EqualTo(0x04));
    }
}
