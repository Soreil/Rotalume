using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;


namespace Tests
{
    class GPU
    {
        public static byte[] LoadGameROM() => File.ReadAllBytes(@"..\..\..\rom\Tetris (World) (Rev A).gb");

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

            var oldLY = Proc.PPU.LY;
            while (Proc.PC != 0x100)
            {
                step();
                if (Proc.PPU.LY != oldLY)
                {
                    oldLY = Proc.PPU.LY;
                    if (oldLY < 144) Assert.AreNotEqual(generator.Mode.VBlank, Proc.PPU.Mode);
                    else Assert.AreEqual(generator.Mode.VBlank, Proc.PPU.Mode);
                }
            }
        }

        [Test]
        public void DrawScreen()
        {

            var expected = @"................................####......####..####..........................................................####................####..........................
................................####......####..####..........................................................####...............#....#.........................
................................######....####..####................####......................................####..............#.###..#........................
................................######....####..####................####......................................####..............#.#..#.#........................
................................######....####....................########....................................####..............#.###..#........................
................................######....####....................########....................................####..............#.#..#.#........................
................................####..##..####..####..####..####....####....########....####..####......##########....########...#....#.........................
................................####..##..####..####..####..####....####....########....####..####......##########....########....####..........................
................................####..##..####..####..######..####..####..####....####..######..####..####....####..####....####................................
................................####..##..####..####..######..####..####..####....####..######..####..####....####..####....####................................
................................####....######..####..####....####..####..############..####....####..####....####..####....####................................
................................####....######..####..####....####..####..############..####....####..####....####..####....####................................
................................####....######..####..####....####..####..####..........####....####..####....####..####....####................................
................................####....######..####..####....####..####..####..........####....####..####....####..####....####................................
................................####......####..####..####....####..####....##########..####....####....##########....########..................................
................................####......####..####..####....####..####....##########..####....####....##########....########..................................";

            BootBase Proc = new(BootBase.LoadBootROM(), LoadGameROM().ToList());
            var step = Stepper(Proc);

            while (!Proc.PPU.LCDEnable)
                step();

            var render = new generator.Renderer(Proc.PPU);

            var pallette = render.GetPalette();

            List<string> lines = new();
            for (byte y = 64; y < 80; y++)
            {
                var line = render.GetLine(pallette, y, Proc.PPU.BGTileMapDisplaySelect);
                lines.Add(line);
            }
            var screen = string.Join("\r\n", lines);

            Assert.AreEqual(expected, screen);
        }

        //Shows the state of the background tile ID map
        //During boot this does not change and the nintendo logo is laid out in order aside
        //from the copyright logo
        [Test]
        public void DrawTileMap()
        {
            BootBase Proc = new(BootBase.LoadBootROM(), LoadGameROM().ToList());
            var step = Stepper(Proc);

            while (!Proc.PPU.LCDEnable)
                step();

            var render = new generator.Renderer(Proc.PPU);

            var tilemap = Proc.PPU.BGTileMapDisplaySelect;
            for (var y = 0; y < 32; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    var TileID = Proc.PPU.VRAM[tilemap + x + (y * 32)]; //Background ID map is laid out as 32x32 tiles of size 8x8
                    Console.Write(TileID);
                    Console.Write('\t');
                }
                Console.WriteLine();
            }

        }

        [Test]
        public void DrawAllTiles()
        {
            BootBase Proc = new(BootBase.LoadBootROM(), LoadGameROM().ToList());
            var step = Stepper(Proc);

            while (!Proc.PPU.LCDEnable)
                step();

            var render = new generator.Renderer(Proc.PPU);
            for (byte i = 0; i < 27; i++)
            {
                Console.WriteLine(render.GetTile(i));
                Console.WriteLine();
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
            while (!Proc.PPU.LCDEnable)
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
