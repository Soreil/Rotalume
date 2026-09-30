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
            core.Memory[address] = (byte)(address - OAM.Start);
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
            corrupted |= core.Memory[address] != (byte)(address - OAM.Start);
        Assert.That(corrupted, Is.EqualTo(expectedCorruption));
        Assert.That(core.CPU.Registers.DE, Is.EqualTo(0xfe01));
    }

    [TestCase(Opcode.INC_DE, 0x718e, false)]
    [TestCase(Opcode.DEC_DE, 0x718e, false)]
    [TestCase(Opcode.LD_AT_HL_A, 0x718e, false)]
    [TestCase(Opcode.LDI_AT_HL_A, 0x718e, false)]
    [TestCase(Opcode.LDD_AT_HL_A, 0x718e, false)]
    [TestCase(Opcode.LD_A_AT_HL, 0x73ce, false)]
    [TestCase(Opcode.LD_AI_AT_HL, 0x33cc, true)]
    [TestCase(Opcode.LD_AD_AT_HL, 0x33cc, true)]
    public void InstructionCorruptionMatchesExactRows(Opcode opcode, int firstWord, bool combinedRead)
    {
        using var core = TestHelpers.NewCore([(byte)Opcode.NOP], new TestRenderDevice());
        core.Memory[0xff40] = 0;
        var expected = Enumerable.Repeat((byte)0x69, OAM.Size).ToArray();
        byte[] precedingTwo = [0xf0, 0x0f, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66];
        byte[] preceding = [0xcc, 0x33, 0x77, 0x88, 0x0f, 0xf0, 0x99, 0xbb];
        byte[] current = [0xaa, 0x55, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc];
        precedingTwo.CopyTo(expected, 4 * 8);
        preceding.CopyTo(expected, 5 * 8);
        current.CopyTo(expected, 6 * 8);
        for (int i = 0; i < expected.Length; i++)
            core.Memory[(ushort)(OAM.Start + i)] = expected[i];
        core.Memory[0xff40] = 0x91;
        for (int cycles = 0; cycles < 114 && core.Memory[0xff44] != 1; cycles++)
            core.CPU.Cycle();
        Assert.That(core.Memory[0xff44], Is.EqualTo(1));
        for (int cycle = 0; cycle < 6; cycle++)
            core.CPU.Cycle();

        // CPU addresses do not select the damaged row; include the unusable FE page.
        core.CPU.Registers.DE = 0xfea0;
        core.CPU.Registers.HL = 0xfea0;
        core.CPU.Registers.A = 0xff;
        core.CPU.Op(opcode)();
        core.Memory[0xff40] = 0;

        preceding.CopyTo(expected, 6 * 8);
        expected[6 * 8] = (byte)firstWord;
        expected[6 * 8 + 1] = (byte)(firstWord >> 8);
        if (combinedRead)
            preceding.CopyTo(expected, 4 * 8);
        var actual = new byte[OAM.Size];
        for (int i = 0; i < actual.Length; i++)
            actual[i] = core.Memory[(ushort)(OAM.Start + i)];
        Assert.That(actual, Is.EqualTo(expected));
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

    [TestCase(Opcode.PUSH_BC, true)]
    [TestCase(Opcode.PUSH_AF, true)]
    [TestCase(Opcode.POP_BC, false)]
    [TestCase(Opcode.POP_AF, false)]
    public void StackCorruptionEventsFollowBusCycles(Opcode opcode, bool push)
    {
        using var core = TestHelpers.NewCore([(byte)opcode], new TestRenderDevice());
        core.Memory[0xff40] = 0;
        core.CPU.Registers.SP = 0xfe10;
        var events = new List<(long Tick, OAMCorruptionKind Kind)>();
        long start = core.Time();
        core.CPU.OAMCorruption += (_, e) => events.Add((core.Time() - start, e.Kind));

        core.Step();

        (long Tick, OAMCorruptionKind Kind)[] expected = push
            ? [(4, OAMCorruptionKind.Address), (8, OAMCorruptionKind.Write), (12, OAMCorruptionKind.Write)]
            : [(4, OAMCorruptionKind.ReadAndIncrement), (8, OAMCorruptionKind.Read)];
        Assert.That(events, Is.EqualTo(expected));
        Assert.That(core.Time() - start, Is.EqualTo(push ? 16 : 12));
        Assert.That(core.CPU.Registers.SP, Is.EqualTo(push ? 0xfe0e : 0xfe12));
    }
}
