using System;
using System.IO;
using System.Linq;

using NUnit.Framework;


namespace Tests
{
    class GPU
    {
        public static byte[] LoadGameROM() => File.ReadAllBytes(@"..\..\..\rom\Tetris (World) (Rev A).gb");


        [Ignore("No GPU yet")]
        public static void DoBootGPU()
        {
            BootBase Proc = new BootBase(BootBase.LoadBootROM(), LoadGameROM().ToList());
            while (Proc.PC != 0x1d)
                Proc.DoNextOP();
            while (Proc.PC != 0x28)
                Proc.DoNextOP();
            while (Proc.PC != 0x98)
                Proc.DoNextOP();
            while (Proc.PC != 0x9c)
                Proc.DoNextOP();
            while (Proc.PC != 0xa1)
                Proc.DoNextOP();
            while (Proc.PC != 0xa3)
                Proc.DoNextOP();
            while (Proc.PC != 0xe0) //Start of logo check
                Proc.DoNextOP();
            while (Proc.PC != 0xf1) //logo checksum initialized
                Proc.DoNextOP();
            while (Proc.PC != 0xf9) //Past first subloop
                Proc.DoNextOP();
            while (Proc.PC != 0xfa) //Logo checksum validation
                Proc.DoNextOP();

            Proc.DoNextOP();
            Assert.AreNotEqual(0xfa, Proc.PC); //Logo if logo check failed and we are stuck

            while (Proc.PC != 0x100)
                Proc.DoNextOP();

            Assert.AreEqual(0x100, Proc.PC);
        }

        [Test]
        public void GPUFieldsGetSetDuringBoot()
        {
            BootBase Proc = new BootBase(BootBase.LoadBootROM(), LoadGameROM().ToList());

            Assert.AreEqual(Proc.PPU.LCDC, 0);
            Assert.AreEqual(Proc.PPU.BGP, 0);
            Assert.AreEqual(Proc.PPU.SCY, 0);

            //Sets GPU to be in VBlank
            Proc.dec.Storage.Write(0xff44, 0x90);
            while (Proc.PC != 0x100)
                Proc.DoNextOP();

            Assert.AreEqual(0x100, Proc.PC);

            Assert.AreEqual(Proc.PPU.LCDC, 0x91);
            Assert.AreEqual(Proc.PPU.BGP, 0xFC);
            Assert.AreEqual(Proc.PPU.SCY, 0);
        }

        [Test]
        public void LineRegisterGetsIncrementedDuringVBlank()
        {
            BootBase Proc = new BootBase(BootBase.LoadBootROM(), LoadGameROM().ToList());
            var step = Stepper(Proc);

            var oldLY = Proc.PPU.LY;
            while (Proc.PC != 0x100)
            {
                step();
                if (Proc.PPU.LY != oldLY)
                {
                    if (oldLY >= 144 && oldLY != 153)
                        Assert.Greater(Proc.PPU.LY, oldLY);
                    oldLY = Proc.PPU.LY;
                }
            }
        }
        [Test]
        public void ModeIsOnlyVBlankDuringVBlank()
        {
            BootBase Proc = new BootBase(BootBase.LoadBootROM(), LoadGameROM().ToList());
            var step = Stepper(Proc);

            while (Proc.PC != 0x100)
            {
                step();
                if (Proc.PPU.LY < 144) Assert.AreNotEqual(generator.Mode.VBlank, Proc.PPU.Mode);
                else Assert.AreEqual(generator.Mode.VBlank, Proc.PPU.Mode);
            }
        }

        [Test]
        public void GPUTileDraw()
        {
            string expectedEmpty = @"........
........
........
........
........
........
........
........";

            BootBase Proc = new(BootBase.LoadBootROM(), LoadGameROM().ToList());
            var step = Stepper(Proc);

            //Run boot so the memory is fully populated
            while (Proc.PC != 0x100)
                step();

            var render = new generator.Renderer(Proc.PPU);
            var tile = render.GetTile(0);

            Assert.AreEqual(expectedEmpty, tile);

            string expectedCopyright = @"..####..
.#....#.
#.###..#
#.#..#.#
#.###..#
#.#..#.#
.#....#.
..####..";

            var copyrightTile = render.GetTile(25);

            Assert.AreEqual(expectedCopyright, copyrightTile);
        }

        [Test]
        public void GPUBoot()
        {
            BootBase Proc = new BootBase(BootBase.LoadBootROM(), LoadGameROM().ToList());
            var step = Stepper(Proc);

            //LCD is off
            Assert.AreEqual(Proc.PPU.LCDC, 0);
            //No palette
            Assert.AreEqual(Proc.PPU.BGP, 0);
            //No scroll
            Assert.AreEqual(Proc.PPU.SCY, 0);

            //Happy location where we check the value of 0xff44
            while (Proc.PC != 0x64)
                step();

            while (Proc.PC != 0x100)
                step();

            Assert.AreEqual(0x100, Proc.PC);

            //LCD is on and a layer is being drawn
            Assert.AreEqual(Proc.PPU.LCDC, 0x91);
            //Palette has a value so we can see white and black
            Assert.AreEqual(Proc.PPU.BGP, 0xFC);
            //We have scrolled to the top of the screen
            Assert.AreEqual(Proc.PPU.SCY, 0);
        }

        private static Action Stepper(BootBase b)
        {
            return () =>
            {
                b.DoNextOP();
                b.DoPPU();
            };
        }
    }
}
