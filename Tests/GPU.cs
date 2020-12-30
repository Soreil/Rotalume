using emulator;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace Tests
{
    class GPU
    {
        public static List<byte> LoadGameROM() => File.ReadAllBytes(@"..\..\..\rom\Tetris (World) (Rev A).gb").ToList();

        [Test]
        public void LineRegisterGetsIncrementedDuringVBlank()
        {
            Core Proc = new Core(Core.LoadBootROM(), LoadGameROM());

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
            Core Proc = new Core(Core.LoadBootROM(), LoadGameROM());
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

            Core Proc = new(Core.LoadBootROM(), LoadGameROM());

            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var render = new emulator.Renderer(Proc.PPU);

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
            Core Proc = new(Core.LoadBootROM(), LoadGameROM());

            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var render = new emulator.Renderer(Proc.PPU);

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
            Core Proc = new(Core.LoadBootROM(), LoadGameROM());

            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var render = new emulator.Renderer(Proc.PPU);
            for (byte i = 0; i < 27; i++)
            {
                System.Console.WriteLine(render.GetTile(i));
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

            Core Proc = new(Core.LoadBootROM(), LoadGameROM());

            //Run boot so the memory is fully populated
            while (!Proc.PPU.LCDEnable)
                Proc.Step();

            var render = new emulator.Renderer(Proc.PPU);
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
            Core Proc = new Core(Core.LoadBootROM(), LoadGameROM());

            //LCD is off
            Assert.AreEqual(Proc.PPU.LCDC, 0);
            //No palette
            Assert.AreEqual(Proc.PPU.BGP, 0);
            //No scroll
            Assert.AreEqual(Proc.PPU.SCY, 0);

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
        }
        [Test]
        public void TetrisHangs()
        {
            Core Proc = new Core(Core.LoadBootROM(), LoadGameROM());

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
        public void TetrisDMA()
        {
            Core Proc = new Core(Core.LoadBootROM(), LoadGameROM());

            int dmaCount = 0;
            var SPOnCall = 0;
            var SPBefore = 0;
            var SPAfter = 0;
            while (dmaCount != 0x17a7)
            {
                //while (Proc.PC != 0x01d5) Proc.Step();

                //while (Proc.PC != 0xffb6) Proc.Step();

                ////We are now in DMA
                //while (Proc.PC != 0xffbf) Proc.Step();
                ////Return time
                //System.Console.WriteLine("From:{0:X}", Proc.PC);
                //Proc.Step();
                //System.Console.WriteLine("To:{0:X}", Proc.PC);
                ////return addr loaded by ret
                //System.Console.WriteLine();

                Proc.Step();
                if (Proc.PC == 0x01d5)
                {
                    SPOnCall = Proc.CPU.Registers.SP;
                }
                if (Proc.PC == 0xffbf)
                {
                    dmaCount++;
                }
            }


            while (Proc.PC != 0x01d5) Proc.Step();

            while (Proc.PC != 0xffb6)
            {
                System.Console.WriteLine("Stack value going to ffb6:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
                System.Console.WriteLine("PC going to ffb6:{0:X}", Proc.PC);
                Proc.Step();
            }
            System.Console.WriteLine("Stack value at ffb6:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
            System.Console.WriteLine("PC at ffb6:{0:X}", Proc.PC);


            Proc.Step();
            System.Console.WriteLine("Stack value going to ffb8:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
            System.Console.WriteLine("PC going to ffb8:{0:X}", Proc.PC);


            //Our stack pointer it set to a position in FFxx where LDH is going to write to execute
            //The DMA transfer. This happens to overwrite our stack. BGB has a stackpointer in WRAM
            //At CFF3. Our stack pointer should be somewhere there as well.
            Proc.Step();
            System.Console.WriteLine("Stack value at ffb8:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
            System.Console.WriteLine("PC at ffb8:{0:X}", Proc.PC);

            //Pausing for DMA duration
            while (Proc.PC != 0xffbf)
            {
                Proc.Step();
                System.Console.WriteLine("Stack value going to ffbf:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
                System.Console.WriteLine("PC going to ffbf:{0:X}", Proc.PC);
            }

            //Return time
            System.Console.WriteLine("From:{0:X} PC at ffbf", Proc.PC);
            System.Console.WriteLine("Stack value at ffbf:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
            Proc.Step();
            System.Console.WriteLine("To:{0:X} PC after ffbf", Proc.PC);
            System.Console.WriteLine("Stack value after ffbf:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
            //return addr loaded by ret
            System.Console.WriteLine();


            SPBefore = Proc.CPU.Registers.SP;
            Proc.Step();
            System.Console.WriteLine("Stack value after returning:{0:X}", Proc.CPU.Memory.ReadWide(Proc.CPU.Registers.SP));
            SPAfter = Proc.CPU.Registers.SP;
            //0x17a8 here for some reason. If SP was 1 different we would have gotten expected 01d8
            //Something is changing the stackpointer in between the call to DMA function and return?
            Assert.AreEqual(0x01d8, Proc.PC);
            Assert.AreEqual(SPOnCall, SPBefore);
        }
    }
}
