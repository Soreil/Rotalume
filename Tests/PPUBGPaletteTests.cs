using emulator.graphics;
using emulator.input;
using emulator.opcodes;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace Tests;

internal class PPUBGPaletteTests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void BGPWriteOverlapsOldAndNewBitsForExactlyOneDot(int colorIndex)
    {
        var interrupts = new InterruptRegisters(new Keypad(new InputDevices(new MockGameController(), [])));
        var ppu = new PPU(new TestRenderDevice(), interrupts, new OAM(), new VRAM(), NullLogger<PPU>.Instance);
        byte oldPalette = (byte)(1 << (colorIndex * 2));
        byte newPalette = (byte)(2 << (colorIndex * 2));

        ppu[Address.BGP] = oldPalette;
        ppu.Tick();
        ppu.Tick();
        Assert.That(ppu.BackgroundColor(colorIndex), Is.EqualTo(Shade.LightGray));

        ppu[Address.BGP] = newPalette;
        Assert.That(ppu[Address.BGP], Is.EqualTo(newPalette), "CPU readback must not include overlap bits.");
        Assert.That(ppu.BackgroundColor(colorIndex), Is.EqualTo(Shade.DarkGray));

        ppu.Tick();
        Assert.That(ppu.BackgroundColor(colorIndex), Is.EqualTo(Shade.Black));
        Assert.That(ppu[Address.BGP], Is.EqualTo(newPalette));

        // Expiry follows elapsed dots, even when the LCD is off and no pixel is emitted.
        ppu.Tick();
        Assert.That(ppu.BackgroundColor(colorIndex), Is.EqualTo(Shade.DarkGray));
        for (int otherIndex = 0; otherIndex < 4; otherIndex++)
        {
            if (otherIndex != colorIndex)
                Assert.That(ppu.BackgroundColor(otherIndex), Is.EqualTo(Shade.White));
        }
    }
}
