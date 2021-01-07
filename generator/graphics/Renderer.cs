using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


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
            List<Shade> background = null;
            List<Shade> sprites = null;
            if (PPU.BGDisplayEnable)
            {
                var palette = GetBackgroundPalette();
                background = GetBackgroundLineShades(palette, YScrolled(PPU.LY, PPU.SCY), PPU.BGTileMapDisplaySelect);
            }
            if (PPU.OBJDisplayEnable)
            {
                sprites = GetSpriteLineShades();
            }

            List<Shade> line;
            if (background is not null && sprites is not null)
                line = Merge(background, sprites);
            else
            {
                if (background is not null)
                    line = background;
                else if (sprites is not null)
                    line = sprites;
                else
                {
                    line = new List<Shade>();
                    for (int i = 0; i < 160; i++) line.Add(Shade.White);
                }
            }
            fs.Write(line.ConvertAll(ShadeToGray).ToArray());
        }
        private List<Shade> Merge(List<Shade> background, List<Shade> sprites)
        {
            var pixels = new List<Shade>(160);
            for (int i = 0; i < 160; i++)
            {
                if (sprites[i] == Shade.Transparant) pixels.Add(background[i]);
                else pixels.Add(sprites[i]);
            }
            return pixels;
        }

        public static byte ShadeToGray(Shade s) => s switch
        {
            Shade.White => 0xff,
            Shade.LightGray => 0xc0,
            Shade.DarkGray => 0x40,
            Shade.Black => 0,
            Shade.Transparant => 0xff, //We shouldn't hit this one but white is the default
            _ => throw new Exception(),
        };

        public string GetLine(Shade[] palette, byte yScrolled, ushort tilemap)
        {
            var pixels = GetBackgroundLineShades(palette, yScrolled, tilemap);

            var s = MakePrintableLine(pixels);
            return s;
        }

        private List<Shade> GetBackgroundLineShades(Shade[] palette, byte yScrolled, ushort tilemap)
        {
            var pixelsBG = new List<Shade>(160);

            for (int tileNumber = 0; tileNumber < 20; tileNumber++)
            {
                var curPix = TilePixelLine(palette, yScrolled, tilemap, tileNumber);
                for (int cur = 0; cur < curPix.Length; cur++)
                    pixelsBG.Add(curPix[cur]);
            }

            return pixelsBG;
        }
        private List<Shade> GetSpriteLineShades()
        {
            var pixelsSprite = new List<Shade>(160);

            for (int x = 0; x < 160; x++)
            {
                pixelsSprite.Add(GetSpritePixel(x));
            }

            return pixelsSprite;
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

        private Shade GetSpritePixel(int xPos)
        {
            if (!PPU.OBJDisplayEnable) throw new Exception("Sprites disabled");
            if (!SpriteAttributes.Any(s => s.X >= xPos && (s.X < xPos + 7))) return Shade.Transparant;

            var sprite = SpriteAttributes.First(s => s.X >= xPos && (s.X < xPos + 7));

            //if (sprite.X == 0x18 && sprite.Y == 0x78 && sprite.XFlipped == false && sprite.YFlipped == true && sprite.ID == 0x92) 
            //    System.Diagnostics.Debugger.Break();

            var index = sprite.X - xPos;
            if (sprite.XFlipped) index = 7 - index;

            var line = PPU.LY - sprite.Y;
            if (sprite.YFlipped) line = 7 - line;

            var palette = sprite.Palette == 1 ? GetSpritePalette1() : GetSpritePalette0();

            var at = 0x8000 + (sprite.ID * 16) + (line * 2);
            var tileDataLow = PPU.VRAM[at]; //low byte of line
            var tileDataHigh = PPU.VRAM[at + 1]; //high byte of line

            var paletteIndex = tileDataLow.GetBit(index) ? 1 : 0;
            paletteIndex += tileDataHigh.GetBit(index) ? 2 : 0;

            return palette[paletteIndex];
        }

        private Shade[] GetTileLine(Shade[] palette, int line, byte currentTileIndex)
        {
            var tileData = PPU.BGAndWindowTileDataSelect;

            var pixels = new Shade[8];

            //16 bytes per tile so times 16 on the tileindex
            //We need the line in the 8x8 tile so we take y mod 8 to get it
            //Times 2 is needed because a tile has two bytes per line.
            int at = 0;
            if (tileData != 0x8800)
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
            var palette = GetBackgroundPalette();

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

        public Shade[] GetBackgroundPalette() => new Shade[4] {
                PPU.BackgroundColor(0),
                PPU.BackgroundColor(1),
                PPU.BackgroundColor(2),
                PPU.BackgroundColor(3)
            };
        public Shade[] GetSpritePalette0() => new Shade[4] {
                Shade.Transparant,
                PPU.SpritePalette0(1),
                PPU.SpritePalette0(2),
                PPU.SpritePalette0(3)
            };
        public Shade[] GetSpritePalette1() => new Shade[4] {
                Shade.Transparant,
                PPU.SpritePalette1(1),
                PPU.SpritePalette1(2),
                PPU.SpritePalette1(3)
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