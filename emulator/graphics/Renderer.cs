using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace emulator
{
    public class Renderer
    {
        readonly PPU PPU;
        readonly Func<long> Clock;
        public int TimeUntilWhichToPause;
        readonly Stream fs = Stream.Null;

        public const int TileWidth = 8;
        public const int DisplayWidth = 160;
        public const int TilesPerLine = DisplayWidth / TileWidth;
        public const int DisplayHeight = 144;
        public const int ScanlinesPerFrame = DisplayHeight + 10;

        public const int TicksPerScanline = 456;
        public const int TicksPerFrame = ScanlinesPerFrame * TicksPerScanline;

        public Renderer(PPU ppu, Stream destination = null)
        {
            fs = destination ?? Stream.Null;
            PPU = ppu;
            var startTime = PPU.Clock();
            Clock = () => ppu.Clock() - startTime;
        }

        public List<SpriteAttributes> SpriteAttributes = new();

        public void Render()
        {

            if (Clock() < TimeUntilWhichToPause) return;

            if (PPU.Mode == Mode.OAMSearch || PPU.Mode == Mode.VBlank)
            {
                PPU.LY = PPU.LY == 153 ? 0 : (byte)(PPU.LY + 1);
                if (PPU.LY == 0) WindowLY = 0;
                if (PPU.LY == PPU.LYC) PPU.LYCInterrupt = true;
            }
            if (PPU.Mode == Mode.OAMSearch)
            {
                SpriteAttributes = PPU.OAM.SpritesOnLine(PPU.LY, PPU.SpriteHeight);
            }
            if (PPU.Mode == Mode.Transfer)
                Draw();

            IncrementMode();
            SetLockStates(); //Doesn't work currently
            SetStatInterruptForMode();
            SetNewClockTarget();
        }

        private void SetLockStates()
        {
            switch (PPU.Mode)
            {
                case Mode.HBlank:
                case Mode.VBlank:
                    PPU.VRAM.Locked = false;
                    PPU.OAM.Locked = false;
                    break;
                case Mode.OAMSearch:
                    PPU.OAM.Locked = true;
                    PPU.VRAM.Locked = false;
                    break;
                case Mode.Transfer:
                    PPU.OAM.Locked = true;
                    PPU.VRAM.Locked = true;
                    break;
            }
        }

        private void SetStatInterruptForMode()
        {
            if (PPU.Mode == Mode.OAMSearch && PPU.STAT.GetBit(5)) PPU.EnableLCDCStatusInterrupt();
            else if (PPU.Mode == Mode.VBlank && PPU.STAT.GetBit(4) || PPU.STAT.GetBit(5)) PPU.EnableLCDCStatusInterrupt();
            else if (PPU.Mode == Mode.HBlank && PPU.STAT.GetBit(3)) PPU.EnableLCDCStatusInterrupt();
        }

        private readonly Shade[] background = new Shade[DisplayWidth];
        private readonly (int, int, bool)[] sprites = new (int, int, bool)[DisplayWidth];
        private readonly Shade[] window = new Shade[DisplayWidth];
        private readonly byte[] output = new byte[DisplayWidth];
        private void Draw()
        {
            if (PPU.BGDisplayEnable)
            {
                var palette = GetBackgroundPalette();
                GetBackgroundLineShades(palette, PPU.BGTileMapDisplaySelect, background);

                if (PPU.WindowDisplayEnable)
                {
                    if (PPU.WY <= PPU.LY && PPU.WX < 167)
                    {
                        GetWindowLineShades(window);
                        MergeWindow(background, window);
                    }
                }
            }
            else
            {
                var bgp = GetBackgroundPalette();
                for (int i = 0; i < DisplayWidth; i++)
                    background[i] = bgp[0];
            }
            if (PPU.OBJDisplayEnable && SpriteAttributes.Any())
            {
                GetSpriteLineShades(sprites);
                MergeSprites(background, sprites);
            }

            for (int i = 0; i < DisplayWidth; i++)
                output[i] = ShadeToGray(background[i]);

            fs.Write(output);
        }

        int WindowLY = 0;
        private void GetWindowLineShades(Shade[] line)
        {
            var windowStartX = PPU.WX - 7;
            var windowStartY = WindowLY++;
            if (windowStartX < 0) windowStartX = 0;

            for (int i = 0; i < line.Length; i++) line[i] = Shade.Empty;

            var firstTile = windowStartX / 8;
            for (int tileNumber = windowStartX / 8; tileNumber < TilesPerLine; tileNumber++)
            {
                var curPix = TilePixelLineWindow(GetBackgroundPalette(), windowStartY, PPU.TileMapDisplaySelect, tileNumber - firstTile);
                for (int cur = 0; cur < curPix.Length; cur++)
                {
                    if ((tileNumber * TileWidth) + cur >= windowStartX)
                        line[(tileNumber * TileWidth) + cur] = curPix[cur];
                }
            }

        }

        private void MergeSprites(Shade[] background, (int, int, bool)[] sprites)
        {
            var bgp0 = GetBackgroundPalette()[0];
            for (int i = 0; i < DisplayWidth; i++)
            {
                if (sprites[i].Item1 != 0)
                {
                    var palettes = new Shade[2][] { GetSpritePalette0(), GetSpritePalette1() };

                    //Handle background priority we got this wrong, we should be looking for an index of the background pixel not the actual colour after lookup most likely?
                    //BGBTEST only works right if I choose shade.black because that is index 0 for it
                    if (!sprites[i].Item3 || background[i] == bgp0)
                    {
                        var colour = palettes[sprites[i].Item2][(int)sprites[i].Item1];
                        if (colour != Shade.Transparant)
                            background[i] = colour;
                    }
                }
            }
        }

        private static void MergeWindow(Shade[] background, Shade[] window)
        {
            for (int i = 0; i < DisplayWidth; i++)
            {
                if (window[i] != Shade.Empty) background[i] = window[i];
            }
        }

        public static byte ShadeToGray(Shade s) => s switch
        {
            Shade.White => 0xff,
            Shade.LightGray => 0xc0,
            Shade.DarkGray => 0x40,
            Shade.Black => 0,
            _ => throw new Exception(),
        };

        public void GetBackgroundLineShades(Shade[] palette, ushort tilemap, Shade[] background)
        {

            //Offset is the amount of pixels we have to draw for the first tile if the first tile is going to be scrolled
            var offset = PPU.SCX & 0x7;

            var firstTilePixels = TilePixelLineNoScroll(palette, YScrolled(PPU.LY, PPU.SCY), tilemap, offset);
            for (int cur = 0; cur < firstTilePixels.Length; cur++)
                background[cur] = firstTilePixels[cur];

            for (int tileNumber = 1; tileNumber <= TilesPerLine; tileNumber++)
            {
                var curPix = TilePixelLine(palette, YScrolled(PPU.LY, PPU.SCY), tilemap, tileNumber);
                for (int cur = 0; cur < curPix.Length; cur++)
                {
                    var pos = 8 - offset + ((tileNumber - 1) * TileWidth) + cur;
                    if (pos >= DisplayWidth) break;
                    background[pos] = curPix[cur];
                }
            }
        }

        private Shade[] TilePixelLineNoScroll(Shade[] palette, byte yOffset, ushort tilemap, int offset)
        {
            var xOffset = (PPU.SCX / TileWidth) & 0x1f;

            var TileID = PPU.VRAM[tilemap + xOffset + ((yOffset / TileWidth) * 32)]; //Background ID map is laid out as 32x32 tiles of size TileWidthxTileWidth

            var pixels = GetTileLine(palette, yOffset % TileWidth, TileID);

            return pixels.Skip(offset).ToArray();
        }

        private void GetSpriteLineShades((int, int, bool)[] sprites)
        {
            for (int x = 1; x <= DisplayWidth; x++)
            {
                sprites[x - 1] = GetSpritePixel(x);
            }
        }

        private static byte YScrolled(byte LY, byte SCY) => (byte)((LY + SCY) & 0xff);

        private Shade[] TilePixelLine(Shade[] palette, int yOffset, ushort tilemap, int tileNumber)
        {
            var xOffset = ((PPU.SCX / TileWidth) + tileNumber) & 0x1f;

            var TileID = PPU.VRAM[tilemap + xOffset + ((yOffset / TileWidth) * 32)]; //Background ID map is laid out as 32x32 tiles of size TileWidthxTileWidth

            var pixels = GetTileLine(palette, yOffset % TileWidth, TileID);

            return pixels;
        }

        private Shade[] TilePixelLineWindow(Shade[] palette, int yOffset, ushort tilemap, int tileNumber)
        {
            var xOffset = (tileNumber) & 0x1f;

            var TileID = PPU.VRAM[tilemap + xOffset + ((yOffset / TileWidth) * 32)]; //Background ID map is laid out as 32x32 tiles of size TileWidthxTileWidth

            var pixels = GetTileLine(palette, yOffset % TileWidth, TileID);

            return pixels;
        }

        private (int, int, bool) GetSpritePixel(int xPos)
        {
            if (!PPU.OBJDisplayEnable) throw new Exception("Sprites disabled");
            if (!SpriteAttributes.Any(s => s.X >= xPos && (s.X <= xPos + 7))) return (0, 0, false);

            var sprites = SpriteAttributes.Where(s => s.X >= xPos && (s.X <= xPos + 7));

            List<(int, SpriteAttributes)> pairs = new();

            foreach (var sprite in sprites)
            {
                //if (PPU.SpriteHeight == 16) System.Diagnostics.Debugger.Break();

                var line = PPU.LY - sprite.Y + 16;
                if (sprite.YFlipped) line = PPU.SpriteHeight - 1 - line;

                var ID = PPU.SpriteHeight == 16 ? (sprite.ID & 0xfe) : sprite.ID;
                var at = 0x8000 + (ID * 16) + (line * 2);
                var tileDataLow = PPU.VRAM[at]; //low byte of line
                var tileDataHigh = PPU.VRAM[at + 1]; //high byte of line

                var index = sprite.X - xPos;
                if (sprite.XFlipped)
                    index = 7 - index;

                var paletteIndex = tileDataLow.GetBit(index) ? 1 : 0;
                paletteIndex += tileDataHigh.GetBit(index) ? 2 : 0;

                pairs.Add((paletteIndex, sprite));
            }

            if (!pairs.Any(x => x.Item1 != 0)) return (0, 0, false);

            var chosen = pairs.Where(x => x.Item1 != 0).First();

            return (chosen.Item1, chosen.Item2.Palette, chosen.Item2.SpriteToBackgroundPriority);
        }

        public Shade[] GetTileLine(Shade[] palette, int line, byte currentTileIndex)
        {
            var tileData = PPU.BGAndWindowTileDataSelect;

            var pixels = new Shade[TileWidth];

            //16 bytes per tile so times 16 on the tileindex
            //We need the line in the TileWidthxTileWidth tile so we take y mod TileWidth to get it
            //Times 2 is needed because a tile has two bytes per line.
            int at;
            if (tileData == 0x8000) at = tileData + (currentTileIndex * 16) + (line * 2);
            else at = 0x9000 + (((sbyte)currentTileIndex) * 16) + (line * 2);

            var tileDataLow = PPU.VRAM[at]; //low byte of line
            var tileDataHigh = PPU.VRAM[at + 1]; //high byte of line

            for (int i = TileWidth; i > 0; i--) //tilesIDs are stored with the first pixel at MSB
            {
                var paletteIndex = tileDataLow.GetBit(i - 1) ? 1 : 0;
                paletteIndex += tileDataHigh.GetBit(i - 1) ? 2 : 0;

                pixels[TileWidth - i] = palette[paletteIndex]; //We want the leftmost bit on the left
            }

            return pixels;
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
            else if (PPU.LY == DisplayHeight)
            {
                PPU.Mode = Mode.VBlank;
                PPU.EnableVBlankInterrupt();
                fs.Flush();
            }
        }

        //This currently doesn't work since the transition to the final draw line is when increment mode sees 143 for line count and HBlank for mode
        //We are effectively updating a register for something which has already happened?
        private bool FinalStageOfFinalPrintedLine() => (PPU.LY == DisplayHeight && PPU.Mode == Mode.HBlank);
        private bool FinalStageOrVBlanking() => FinalStageOfFinalPrintedLine() || PPU.LY > DisplayHeight;

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