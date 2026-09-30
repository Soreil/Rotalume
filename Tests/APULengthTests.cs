using emulator.sound;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Tests;

internal class APULengthTests
{
    [TestCase(Address.NR11, Address.NR12, Address.NR14, 64, 1)]
    [TestCase(Address.NR21, Address.NR22, Address.NR24, 64, 2)]
    [TestCase(Address.NR31, Address.NR30, Address.NR34, 256, 4)]
    [TestCase(Address.NR41, Address.NR42, Address.NR44, 64, 8)]
    public void NonTriggeringControlWriteDoesNotStopChannel(Address length, Address dac, Address control, int maximum, int statusBit)
    {
        var apu = NewAPU();
        apu[length] = 0;
        apu[dac] = 0x80;
        apu[control] = 0x80;
        apu[control] = 0x40;
        Assert.That(apu[Address.NR52] & statusBit, Is.EqualTo(statusBit));
        for (int i = 0; i < maximum * 2 - 1; i++) apu.FrameSequencerClock(null, EventArgs.Empty);
        Assert.That(apu[Address.NR52] & statusBit, Is.Zero);
    }

    [TestCase(Address.NR11, Address.NR12, Address.NR14, 64, 1)]
    [TestCase(Address.NR21, Address.NR22, Address.NR24, 64, 2)]
    [TestCase(Address.NR31, Address.NR30, Address.NR34, 256, 4)]
    [TestCase(Address.NR41, Address.NR42, Address.NR44, 64, 8)]
    public void LengthClocksWhileDACIsOff(Address length, Address dac, Address control, int maximum, int statusBit)
    {
        var apu = NewAPU();
        apu[length] = (byte)(maximum - 1);
        apu[control] = 0x40;
        apu.FrameSequencerClock(null, EventArgs.Empty);
        apu[dac] = 0x80;
        apu[control] = 0x80; //The expired counter must reload, not retain length one.
        for (int i = 0; i < 4; i++) apu.FrameSequencerClock(null, EventArgs.Empty);
        apu[control] = 0x40;
        for (int i = 0; i < 4; i++) apu.FrameSequencerClock(null, EventArgs.Empty);
        Assert.That(apu[Address.NR52] & statusBit, Is.EqualTo(statusBit));
    }

    [TestCase(Address.NR11, Address.NR12, Address.NR14, 64, 1)]
    [TestCase(Address.NR21, Address.NR22, Address.NR24, 64, 2)]
    [TestCase(Address.NR31, Address.NR30, Address.NR34, 256, 4)]
    [TestCase(Address.NR41, Address.NR42, Address.NR44, 64, 8)]
    public void EnablingLengthBetweenClocksImmediatelyExpiresLengthOne(Address length, Address dac, Address control, int maximum, int statusBit)
    {
        var apu = NewAPU();
        apu.FrameSequencerClock(null, EventArgs.Empty); //Next step does not clock length.
        apu[length] = (byte)(maximum - 1);
        apu[dac] = 0x80;
        apu[control] = 0x80;
        Assert.That(apu[Address.NR52] & statusBit, Is.EqualTo(statusBit));
        apu[control] = 0x40;
        Assert.That(apu[Address.NR52] & statusBit, Is.Zero);
    }

    private static APU NewAPU()
    {
        var apu = new APU(NullLogger<APU>.Instance);
        apu[Address.NR52] = 0;
        apu[Address.NR52] = 0x80;
        return apu;
    }
}
