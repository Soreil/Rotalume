using emulator.glue;

using NUnit.Framework;

namespace Tests;

internal class DMABusTests
{
    private static Core NewCore()
    {
        var rom = Enumerable.Repeat((byte)0x55, 0x8000).ToArray();
        rom[0x147] = 0x02; // MBC1 with RAM, without battery-backed files.
        rom[0x148] = 0;
        rom[0x149] = 0x02;
        var core = TestHelpers.NewCore(rom, new TestRenderDevice());
        core.Memory[0xff40] = 0;
        core.Memory[0x0000] = 0x0a;
        return core;
    }

    [Test]
    public void CPUReadsAndWritesAreBlockedUntilDMACompletes()
    {
        using var core = NewCore();
        ushort[] ramAddresses = [0xa000, 0xbfff, 0xc000, 0xdfff, 0xe000, 0xfdff, 0xfe00, 0xfe9f];
        foreach (var address in ramAddresses)
        {
            core.Memory[address] = 0x55;
        }
        ushort[] romAddresses = [0x0000, 0x3fff, 0x4000, 0x7fff];
        foreach (var address in romAddresses.Concat(ramAddresses))
            Assert.That(core.Memory[address], Is.EqualTo(0x55), $"Before DMA: {address:X4}");

        core.Memory[0xff46] = 0x00;
        foreach (var address in romAddresses.Concat(ramAddresses))
            Assert.That(core.Memory[address], Is.EqualTo(0xff), $"During DMA: {address:X4}");
        foreach (var address in ramAddresses)
            core.Memory[address] = 0xaa;
        core.Memory[0x0000] = 0; // Must not disable cartridge RAM during DMA.

        for (int i = 0; i < 159; i++)
            core.CPU.Cycler.Cycle();
        Assert.That(core.Memory[0xc000], Is.EqualTo(0xff), "DMA must last all 160 cycles.");
        core.CPU.Cycler.Cycle();
        foreach (var address in romAddresses.Concat(ramAddresses))
            Assert.That(core.Memory[address], Is.EqualTo(0x55), $"After DMA: {address:X4}");
    }

    [Test]
    public void VRAMAndHRAMRemainAccessibleDuringDMA()
    {
        using var core = NewCore();
        core.Memory[0xff46] = 0;
        ushort[] addresses = [0x8000, 0x9fff, 0xff80, 0xfffe];
        foreach (var address in addresses)
        {
            core.Memory[address] = 0x42;
            Assert.That(core.Memory[address], Is.EqualTo(0x42), $"During DMA: {address:X4}");
        }
        Assert.That(core.Memory[0xff46], Is.Zero);
    }

    [Test]
    public void DMAReadsSourceDataRatherThanBlockedCPUBusValues()
    {
        using var core = NewCore();
        ushort[] sources = [0x0000, 0x4000, 0x8000, 0xa000, 0xc000, 0xe000];
        foreach (var source in sources)
        {
            var expected = new byte[160];
            for (int i = 0; i < expected.Length; i++)
            {
                var address = (ushort)(source + i);
                if (source >= 0x8000)
                    core.Memory[address] = (byte)i;
                expected[i] = core.Memory[address];
            }
            core.Memory[0xff46] = (byte)(source >> 8);
            for (int i = 0; i < 160; i++)
                core.CPU.Cycler.Cycle();
            for (int i = 0; i < expected.Length; i++)
                Assert.That(core.Memory[(ushort)(0xfe00 + i)], Is.EqualTo(expected[i]), $"Source {source:X4}, byte {i:X2}");
        }
    }
}
