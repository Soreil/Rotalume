﻿using System;
using System.Collections.Generic;
using System.IO;


namespace emulator
{
    public class Renderer
    {
        readonly PPU PPU;
        readonly Func<int> Clock;
        public int TimeUntilWhichToPause;
        readonly Stream fs = Stream.Null;

        const int DrawlinesPerFrame = 144;
        const int ScanlinesPerFrame = DrawlinesPerFrame + 10;

        const int TicksPerScanline = 456;
        const int TicksPerFrame = ScanlinesPerFrame * TicksPerScanline;
        int ClocksInFrame => Clock() % TicksPerFrame;
        int Line => ClocksInFrame / TicksPerScanline;
        int ClockInScanline => ClocksInFrame % TicksPerScanline;
        private void UpdateLineRegister()
        {
            PPU.LY = (byte)(Line % ScanlinesPerFrame);
            PPU.LYCInterrupt = PPU.LY == PPU.LYC;
        }
        public Renderer(PPU ppu, Stream destination) : this(ppu)
        {
            fs = destination;
        }
        public Renderer(PPU ppu)
        {
            PPU = ppu;
            var startTime = PPU.Clock();
            Clock = () => ppu.Clock() - startTime;
        }

        public List<SpriteAttributes> SpriteAttributes = new();

        public void Render()
        {
            var currentTime = Clock();

            if (currentTime > TimeUntilWhichToPause)
            {
                if (PPU.Mode == Mode.OAMSearch)
                    SpriteAttributes = PPU.OAM.SpritesOnLine(PPU.LY, PPU.SpriteHeight);
                if (PPU.Mode == Mode.Transfer)
                    Draw();

                UpdateLineRegister();
                IncrementMode();
                SetNewClockTarget();
            }
        }

        private void Draw()
        {
            var palette = GetPalette();
            var line = GetLineShades(palette, YScrolled(PPU.LY, PPU.SCY), PPU.BGTileMapDisplaySelect);
            fs.Write(line.ConvertAll(ShadeToGray).ToArray());
        }

        public static byte ShadeToGray(Shade s) => s switch
        {
            Shade.White => 0xff,
            Shade.LightGray => 0xc0,
            Shade.DarkGray => 0x40,
            Shade.Black => 0,
            _ => throw new Exception(),
        };

        public string GetLine(Shade[] palette, byte yScrolled, ushort tilemap)
        {
            var pixels = GetLineShades(palette, yScrolled, tilemap);

            var s = MakePrintableLine(pixels);
            return s;
        }

        private List<Shade> GetLineShades(Shade[] palette, byte yScrolled, ushort tilemap)
        {
            var pixels = new List<Shade>(160);

            for (int tileNumber = 0; tileNumber < 20; tileNumber++)
            {
                var curPix = TilePixelLine(palette, yScrolled, tilemap, tileNumber);
                for (int cur = 0; cur < curPix.Length; cur++)
                    pixels.Add(curPix[cur]);
            }

            return pixels;
        }

        private static byte YScrolled(byte LY, byte SCY) => (byte)((LY + SCY) & 0xff);

        private static string MakePrintableLine(List<Shade> pixels)
        {
            char[] ScreenLine = new char[pixels.Count];
            for (var p = 0; p < pixels.Count; p++)
            {
                if (pixels[p] == Shade.White) ScreenLine[p] = '.';
                else if (pixels[p] == Shade.Black) ScreenLine[p] = '#';
                else throw new ArgumentException("Expected a white or black pixel only in the boot up screen");
            }

            var s = new string(ScreenLine);
            return s;
        }

        private Shade[] TilePixelLine(Shade[] palette, int yOffset, ushort tilemap, int tileNumber)
        {
            var xOffset = ((PPU.SCX / 8) + tileNumber) & 0x1f;

            var TileID = PPU.VRAM[tilemap + xOffset + ((yOffset / 8) * 32)]; //Background ID map is laid out as 32x32 tiles of size 8x8
            //if (TileID > 26) System.Diagnostics.Debugger.Break();
            var pixels = GetTileLine(palette, yOffset % 8, TileID);

            return pixels;
        }

        private Shade[] GetTileLine(Shade[] palette, int line, byte currentTileIndex)
        {
            var tileData = PPU.BGAndWindowTileDataSelect;

            var pixels = new Shade[8];

            //16 bytes per tile so times 16 on the tileindex
            //We need the line in the 8x8 tile so we take y mod 8 to get it
            //Times 2 is needed because a tile has two bytes per line.
            int at = 0;
            if (tileData == 0x8000)
            {
                at = tileData + (currentTileIndex * 16) + (line * 2);
            }
            else
            {
                at = 0x9000 + (((sbyte)currentTileIndex) * 16) + (line * 2);
            }

            var tileDataLow = PPU.VRAM[at]; //low byte of line
            var tileDataHigh = PPU.VRAM[at + 1]; //high byte of line

            for (int i = 7; i >= 0; i--) //tilesIDs are stored with the first pixel at MSB
            {
                var paletteIndex = tileDataLow.GetBit(i) ? 1 : 0;
                paletteIndex += tileDataHigh.GetBit(i) ? 2 : 0;

                pixels[7 - i] = palette[paletteIndex]; //We want the leftmost bit on the left
            }

            return pixels;
        }

        public string GetTile(byte tileNumber)
        {
            var palette = GetPalette();

            var lines = new List<List<Shade>>(8);
            for (int y = 0; y < 8; y++)
            {
                var line = GetTileLine(palette, y % 8, tileNumber);
                lines.Add(new(line));
            }

            List<string> output = new();
            foreach (var line in lines)
            {
                output.Add(MakePrintableLine(line));
            }
            return string.Join("\r\n", output);
        }

        public Shade[] GetPalette() => new Shade[4] {
                PPU.BackgroundColor(0),
                PPU.BackgroundColor(1),
                PPU.BackgroundColor(2),
                PPU.BackgroundColor(3)
            };

        private void IncrementMode()
        {
            if (!FinalStageOrVBlanking())
            {
                PPU.Mode = PPU.Mode switch
                {
                    Mode.OAMSearch => Mode.Transfer,
                    Mode.Transfer => Mode.HBlank,
                    Mode.HBlank => Mode.OAMSearch,
                    Mode.VBlank => Mode.OAMSearch, //This should only ever be hit at the moment we wrap back to line zero.
                    _ => throw new InvalidOperationException(),
                };
            }
            else if (PPU.LY == 144)
            {
                PPU.Mode = Mode.VBlank;
                PPU.EnableVBlankInterrupt();
                fs.Flush();
            }
        }

        //This currently doesn't work since the transition to the final draw line is when increment mode sees 143 for line count and HBlank for mode
        //We are effectively updating a register for something which has already happened?
        private bool FinalStageOfFinalPrintedLine() => (Line == DrawlinesPerFrame && PPU.Mode == Mode.HBlank);
        private bool FinalStageOrVBlanking() => FinalStageOfFinalPrintedLine() || Line > DrawlinesPerFrame;

        private void SetNewClockTarget() => TimeUntilWhichToPause += PPU.Mode switch
        {
            Mode.OAMSearch => 80,
            Mode.Transfer => 172, //Transfer can take longer than this, what matters is that  transfer and hblank add up to be 376
            Mode.HBlank => 204, //HBlank can take shorter than this
            Mode.VBlank => TicksPerScanline, //We are going to blank every line since otherwise it would not increment the LY register in the current design
            _ => throw new NotImplementedException(),
        };
    }
}