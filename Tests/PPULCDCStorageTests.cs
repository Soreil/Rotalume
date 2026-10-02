using emulator.graphics;
using emulator.input;
using emulator.opcodes;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace Tests;

internal class PPULCDCStorageTests
{
    [Test]
    public void EveryRegisterValueRoundTripsAndUpdatesFlags()
    {
        var interrupts = new InterruptRegisters(new Keypad(new InputDevices(new MockGameController(), [])));
        var ppu = new PPU(new TestRenderDevice(), interrupts, new OAM(), new VRAM(), NullLogger<PPU>.Instance);

        for (int value = 0; value <= byte.MaxValue; value++)
        {
            ppu[Address.LCDC] = (byte)value;
            Assert.That(ppu[Address.LCDC], Is.EqualTo((byte)value), $"LCDC={value:X2}");
            Assert.That(ppu.TileMapDisplaySelect, Is.EqualTo((value & 0x40) != 0 ? VRAM.TileMap1Start : VRAM.TileMap0Start));
            Assert.That(ppu.WindowDisplayEnable, Is.EqualTo((value & 0x20) != 0));
            Assert.That(ppu.BGAndWindowTileDataSelect, Is.EqualTo((value & 0x10) != 0 ? VRAM.TileBlock0Start : VRAM.TileBlock2Start));
            Assert.That(ppu.BGTileMapDisplaySelect, Is.EqualTo((value & 0x08) != 0 ? VRAM.TileMap1Start : VRAM.TileMap0Start));
            Assert.That(ppu.SpriteHeight, Is.EqualTo((value & 0x04) != 0 ? 16 : 8));
            Assert.That(ppu.OBJDisplayEnable, Is.EqualTo((value & 0x02) != 0));
            Assert.That(ppu.BGDisplayEnable, Is.EqualTo((value & 0x01) != 0));
        }

        ppu[Address.LCDC] = 0;
        Assert.That(ppu[Address.LCDC], Is.Zero);
        Assert.That(ppu.WindowDisplayEnable, Is.False);
        Assert.That(ppu.OBJDisplayEnable, Is.False);
        Assert.That(ppu.BGDisplayEnable, Is.False);
        Assert.That(ppu.SpriteHeight, Is.EqualTo(8));
    }

    [Test]
    public void LCDTransitionsStillPauseResumeAndUnlockMemory()
    {
        var output = new TestRenderDevice();
        var oam = new OAM();
        var vram = new VRAM();
        var interrupts = new InterruptRegisters(new Keypad(new InputDevices(new MockGameController(), [])));
        var ppu = new PPU(output, interrupts, oam, vram, NullLogger<PPU>.Instance);
        int framesDrawn = 0;
        output.FramePushed += (_, _) => framesDrawn++;

        ppu[Address.LCDC] = 0x80;
        Assert.That(output.Paused, Is.False);

        ppu.LY = 42;
        oam.Locked = true;
        vram.Locked = true;
        ppu[Address.LCDC] = 0x7f;

        Assert.That(ppu[Address.LCDC], Is.EqualTo(0x7f));
        Assert.That(framesDrawn, Is.EqualTo(1));
        Assert.That(output.Paused, Is.True);
        Assert.That(ppu.LY, Is.Zero);
        Assert.That(ppu.Mode, Is.EqualTo(Mode.HBlank));
        Assert.That(oam.Locked, Is.False);
        Assert.That(vram.Locked, Is.False);

        ppu[Address.LCDC] = 0;
        Assert.That(framesDrawn, Is.EqualTo(1));
        ppu[Address.LCDC] = 0x80;
        Assert.That(output.Paused, Is.False);
        ppu[Address.LCDC] = 0;
        Assert.That(framesDrawn, Is.EqualTo(2));
    }
}
