using emulator.graphics;
using emulator.opcodes;

using NUnit.Framework;

namespace Tests;

internal class OAMCorruptionTests
{
    [TestCase(0, false)]  // First scan row is immune.
    [TestCase(4, true)]   // First corruptible row.
    [TestCase(76, true)]  // Last scan row.
    [TestCase(80, false)] // Pixel transfer.
    [TestCase(300, false)] // HBlank.
    [TestCase(456, false)] // First row of the next scanline.
    [TestCase(143 * 456, false)] // VBlank (LY 144).
    public void IncDECorruptsOnlyDuringEligibleScanRows(int dotsAfterLineStart, bool expectedCorruption)
    {
        using var core = TestHelpers.NewCore([(byte)Opcode.NOP], new TestRenderDevice());
        core.Memory[0xff40] = 0;
        for (ushort address = OAM.Start; address < OAM.Start + OAM.Size; address++)
            core.Memory[address] = 0xaa;
        core.Memory[0xff40] = 0x91;

        // Synchronize to a regular scanline, avoiding the LCD startup offset.
        for (int cycles = 0; cycles < 114 && core.Memory[0xff44] != 1; cycles++)
            core.CPU.Cycle();
        Assert.That(core.Memory[0xff44], Is.EqualTo(1));
        Assert.That(core.Memory[0xff41] & 3, Is.EqualTo((int)Mode.OAMSearch));

        for (int dots = 0; dots < dotsAfterLineStart; dots += 4)
            core.CPU.Cycle();

        core.CPU.Registers.DE = 0xfe00;
        // Invoke the execution phase directly: the event occurs before its internal cycle.
        core.CPU.Op(Opcode.INC_DE)();
        core.Memory[0xff40] = 0; // Unlock OAM so reads inspect storage, not the bus lock value.

        bool corrupted = false;
        for (ushort address = OAM.Start; address < OAM.Start + OAM.Size; address++)
            corrupted |= core.Memory[address] != 0xaa;
        Assert.That(corrupted, Is.EqualTo(expectedCorruption));
        Assert.That(core.CPU.Registers.DE, Is.EqualTo(0xfe01));
    }

    [Test]
    public void IncDEEmitsAddressEventAfterFetchAndDoesNotCorruptWithLCDOff()
    {
        using var core = TestHelpers.NewCore([(byte)Opcode.INC_DE], new TestRenderDevice());
        core.Memory[0xff40] = 0;
        core.Memory[OAM.Start] = 0xaa;
        core.CPU.Registers.DE = 0xfe00;
        int events = 0;
        long eventTime = -1;
        bool? isReadOrWrite = null;
        ushort addressAtEvent = 0;
        core.CPU.OAMCorruption += (_, e) =>
        {
            events++;
            eventTime = core.Time();
            isReadOrWrite = e.IsOAMReadOrWrite;
            addressAtEvent = core.CPU.Registers.DE;
        };

        long start = core.Time();
        core.Step();

        Assert.That(events, Is.EqualTo(1));
        Assert.That(eventTime - start, Is.EqualTo(4));
        Assert.That(core.Time() - start, Is.EqualTo(8));
        Assert.That(isReadOrWrite, Is.False);
        Assert.That(addressAtEvent, Is.EqualTo(0xfe00));
        Assert.That(core.CPU.Registers.DE, Is.EqualTo(0xfe01));
        Assert.That(core.Memory[OAM.Start], Is.EqualTo(0xaa));
    }
}
