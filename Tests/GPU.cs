using emulator;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;
using System;

namespace Tests
{
    class GPU
    {
        public static byte[] LoadGameROM() => File.ReadAllBytes(@"..\..\..\rom\Tetris (World) (Rev A).gb");
        public static byte[] LoadCPUTestROM() => File.ReadAllBytes(@"..\..\..\rom\cpu_instrs.gb");

        [Test]
        public void LineRegisterGetsIncrementedDuringVBlank()
        {
            Core Proc = new Core(LoadGameROM(), Core.LoadBootROM());

            var oldLY = Proc.PPU.LY;
            while (Proc.PC != 0x100)
            {
                Proc.Step();
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
            Core Proc = new Core(LoadGameROM(), Core.LoadBootROM());
            var oldLY = Proc.PPU.LY;
            while (Proc.PC != 0x100)
            {
                Proc.Step();
                if (Proc.PPU.LY != oldLY)
                {
                    oldLY = Proc.PPU.LY;
                    if (oldLY < 144) Assert.AreNotEqual(emulator.Mode.VBlank, Proc.PPU.Mode);
                    else Assert.AreEqual(emulator.Mode.VBlank, Proc.PPU.Mode);
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

            Core Proc = new(LoadGameROM(), Core.LoadBootROM());

            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var pallette = Proc.PPU.Renderer.GetBackgroundPalette();

            List<string> lines = new();
            for (byte y = 0; y < 255; y++)
            {
                var line = GetLine(pallette, y, Proc.PPU.BGTileMapDisplaySelect, Proc.PPU.Renderer);
                lines.Add(line);
            }
            var screen = string.Join("\r\n", lines);
            Console.WriteLine(screen);
            Assert.AreEqual(expected, screen);
        }

        private static string MakePrintableLine(Shade[] pixels)
        {
            char[] ScreenLine = new char[pixels.Length];
            for (var p = 0; p < pixels.Length; p++)
            {
                if (pixels[p] == Shade.White) ScreenLine[p] = '.';
                else if (pixels[p] == Shade.Black) ScreenLine[p] = '#';
                else throw new ArgumentException("Expected a white or black pixel only in the boot up screen");
            }

            var s = new string(ScreenLine);
            return s;
        }

        public static string GetTile(byte tileNumber, Renderer r)
        {
            var palette = r.GetBackgroundPalette();

            var lines = new List<Shade[]>(Renderer.TileWidth);
            for (int y = 0; y < Renderer.TileWidth; y++)
            {
                var line = r.GetTileLine(palette, y % Renderer.TileWidth, tileNumber);
                lines.Add(line);
            }

            List<string> output = new();
            foreach (var line in lines)
            {
                output.Add(MakePrintableLine(line));
            }
            return string.Join("\r\n", output);
        }

        public static string GetLine(Shade[] palette, byte yScrolled, ushort tilemap, Renderer r)
        {
            var pixels = new Shade[Renderer.DisplayWidth];
            r.GetBackgroundLineShades(palette, tilemap, pixels);

            var s = MakePrintableLine(pixels);
            return s;
        }

        //Shows the state of the background tile ID map
        //During boot this does not change and the nintendo logo is laid out in order aside
        //from the copyright logo
        [Test]
        public void DrawTileMap()
        {
            Core Proc = new(LoadGameROM());

            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var tilemap = Proc.PPU.BGTileMapDisplaySelect;
            for (var y = 0; y < 32; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    var TileID = Proc.PPU.VRAM[tilemap + x + (y * 32)]; //Background ID map is laid out as 32x32 tiles of size 8x8
                    System.Console.Write(TileID);
                    System.Console.Write('\t');
                }
                System.Console.WriteLine();
            }
        }

        [Test]
        public void DrawAllTiles()
        {
            Core Proc = new(LoadGameROM());

            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var render = new emulator.Renderer(Proc.PPU);
            for (byte i = 0; i < 27; i++)
            {
                System.Console.WriteLine(GetTile(i, render));
                System.Console.WriteLine();
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

            Core Proc = new(LoadGameROM(), Core.LoadBootROM());

            //Run boot so the memory is fully populated
            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var render = new emulator.Renderer(Proc.PPU);
            var tile = GetTile(0, render);

            Assert.AreEqual(expectedEmpty, tile);

            string expectedCopyright = @"..####..
.#....#.
#.###..#
#.#..#.#
#.###..#
#.#..#.#
.#....#.
..####..";

            var copyrightTile = GetTile(25, render);

            Assert.AreEqual(expectedCopyright, copyrightTile);
        }

        [Test]
        public void GPUBoot()
        {
            Core Proc = new Core(LoadGameROM(), Core.LoadBootROM());

            //LCD is off
            Assert.AreEqual(Proc.PPU.LCDC, 0);

            //Happy location where we check the value of 0xff44
            while (Proc.PC != 0x64)
                Proc.Step();

            while (Proc.PC != 0x100)
                Proc.Step();

            Assert.AreEqual(0x100, Proc.PC);

            //LCD is on and a layer is being drawn
            Assert.AreEqual(Proc.PPU.LCDC, 0x91);
            //Palette has a value so we can see white and black
            Assert.AreEqual(Proc.PPU.BGP, 0xFC);
            //We have scrolled to the top of the screen
            Assert.AreEqual(Proc.PPU.SCY, 0);
            Assert.AreEqual(Proc.PPU.LYC, 0);


            Assert.AreEqual(Proc.CPU.InterruptControlRegister, 0);

            Assert.AreEqual(0x01b0, Proc.CPU.Registers.AF);
            Assert.AreEqual(0x0013, Proc.CPU.Registers.BC);
            Assert.AreEqual(0x00d8, Proc.CPU.Registers.DE);
            Assert.AreEqual(0x014d, Proc.CPU.Registers.HL);
            Assert.AreEqual(0xfffe, Proc.CPU.Registers.SP);


            Assert.AreEqual(Proc.PPU.OBP0, 0xff);
            Assert.AreEqual(Proc.PPU.OBP1, 0xff);
        }

        [Test]
        public void TetrisHangs()
        {
            Core Proc = new Core(LoadGameROM(), Core.LoadBootROM());

            while (Proc.PC != 0x100)
                Proc.Step();

            Assert.AreEqual(0x100, Proc.PC);

            //LCD is on and a layer is being drawn
            Assert.AreEqual(Proc.PPU.LCDC, 0x91);
            //Palette has a value so we can see white and black
            Assert.AreEqual(Proc.PPU.BGP, 0xFC);
            //We have scrolled to the top of the screen
            Assert.AreEqual(Proc.PPU.SCY, 0);

            //We should make it past the JR at some point
            /*2798 - 27ac (HL = 9bff)
             * Load 0x400 in to BC
             * while BC is not 0:
             * Load 2F in to (HL) and decrement HL
             * decrement BC
             * 
             * return
             */
            while (Proc.PC != 0x2798) Proc.Step();
            //while (Proc.PC < 0x27a1) Proc.Step();
            //Proc.CPU.Registers.Mark(Flag.Z);
            //Proc.Step();

            Proc.Step();
            System.Console.WriteLine("BC:{0:X}", Proc.CPU.Registers.BC);
            System.Console.WriteLine("HL:{0:X}", Proc.CPU.Registers.HL);

            while (Proc.PC < 0x27a3)
            {
                Proc.Step();
                System.Console.WriteLine("Load 2f in to A:{0:X}", Proc.CPU.Registers.A);
                System.Console.WriteLine("HL before:{0:X}", Proc.CPU.Registers.HL);
                Proc.Step();
                System.Console.WriteLine("HL after:{0:X}", Proc.CPU.Registers.HL);
                Proc.Step();
                System.Console.WriteLine("BC:{0:X}", Proc.CPU.Registers.BC);
                Proc.Step();
                System.Console.WriteLine("Loaded B in to A:{0:X}", Proc.CPU.Registers.A);
                Proc.Step();
                System.Console.WriteLine("OR C and A, C:{0:X} Zero:{1}", Proc.CPU.Registers.C, Proc.CPU.Registers.Get(Flag.Z));
                Proc.Step();
                System.Console.WriteLine();
            }

            Assert.AreEqual(0x27a3, Proc.PC);
        }


        [Test]
        public void TetrisDraw()
        {
            Core Proc = new Core(LoadGameROM(), Core.LoadBootROM());

            while (Proc.PC != 0x02a0) Proc.Step();
            while (Proc.PC != 0x02ba) Proc.Step();
            while (Proc.PC != 0x02ba) Proc.Step(); //Does not make it past this. 0x02ba is enable interrupts so it gets stuck in an interrupt.
            while (Proc.PC != 0x02c7) Proc.Step();

            while (Proc.PC != 0x0028) Proc.Step(); //ISR 28?

            while (Proc.PC != 0x0032) Proc.Step(); //Pop HL
            while (Proc.PC != 0x0033) Proc.Step(); //Jump HL

            while (Proc.PC != 0x02ca) Proc.Step(); //Call 02f8
            while (Proc.PC != 0x02cd) Proc.Step(); //Call 7ff0
        }
    }
}