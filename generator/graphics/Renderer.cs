using System;
using System.Collections.Generic;
using System.IO;


namespace generator
{
    public class Renderer
    {
        readonly PPU PPU;
        readonly Func<int> Clock;
        public int TimeUntilWhichToPause;
        //readonly StreamWriter fs;

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

        public Renderer(PPU ppu)
        {
            PPU = ppu;
            var startTime = PPU.Clock();
            Clock = () => ppu.Clock() - startTime;

            //fs = new(@"screens.txt");
        }

        public List<SpriteAttributes> SpriteAttributes = new();

        public void Render()
        {
            var currentTime = Clock();

            if (currentTime >= TimeUntilWhichToPause)
            {
                if (PPU.Mode == Mode.OAMSearch)
                    SpriteAttributes = PPU.OAM.SpritesOnLine(PPU.LY);
                if (PPU.Mode == Mode.Transfer)
                    Draw();

                UpdateLineRegister();
                IncrementMode();
                SetNewClockTarget();
            }
        }


        private void Draw()
        {
            var line = GetLine();
            //fs.Write(line);
            //fs.Write(' ');
            //fs.Write(PPU.LY);
            //fs.Write(' ');
            //fs.Write(ClocksInFrame);
            //fs.WriteLine();
        }
        private string GetLine()
        {
            var palette = GetPalette();

            var pixels = new List<Shade>(160);

            var yOffset = (PPU.LY + PPU.SCY) & 0xff;
            var tilemap = PPU.BGTileMapDisplaySelect;

            for (int tileNumber = 0; tileNumber < 20; tileNumber++)
            {
                var curPix = TilePixelLine(palette, yOffset, tilemap, tileNumber);
                for (int cur = 0; cur < curPix.Length; cur++)
                    pixels.Add(curPix[cur]);
            }

            var s = MakePrintableLine(pixels);
            return s;
        }

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

            var TileID = PPU.VRAM[tilemap + xOffset + (yOffset * 4)]; //Background ID map is laid out as 32x32 tiles of size 8x8

            var pixels = GetTileLine(palette, yOffset, TileID);

            return pixels;
        }

        private Shade[] GetTileLine(Shade[] palette, int yOffset, byte currentTileIndex)
        {
            var tileData = PPU.BGAndWindowTileDataSelect;

            var pixels = new Shade[8];

            //16 bytes per tile so times 16 on the tileindex
            //We need the line in the 8x8 tile so we take y mod 8 to get it
            //Times 2 is needed because a tile has two bytes per line.
            var at = tileData + (currentTileIndex * 16) + ((yOffset % 8) * 2);

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
                var line = GetTileLine(palette, y, tileNumber);
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
            else
            {
                PPU.Mode = Mode.VBlank;
                //fs.WriteLine();
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