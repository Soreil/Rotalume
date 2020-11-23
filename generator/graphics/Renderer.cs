using System;
using System.Collections.Generic;

namespace generator
{
    internal class Renderer
    {
        readonly PPU PPU;
        readonly Func<int> Clock;
        public int TimeUntilWhichToPause;

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
        }

        public List<SpriteAttributes> SpriteAttributes = new();

        public void Render()
        {
            var currentTime = Clock();

            if (PPU.Mode == Mode.OAMSearch)
                SpriteAttributes = PPU.OAM.SpritesOnLine(PPU.LY);
            if (PPU.Mode == Mode.Transfer)
                Draw();

            if (currentTime > TimeUntilWhichToPause)
            {
                UpdateLineRegister();
                IncrementMode();
                SetNewClockTarget();
            }
        }

        private void Draw()
        {
            int totalCycles = 0;

            GetTile();
        }

        private void GetTile()
        {
            var yOffset = (PPU.LY + PPU.SCY) & 0xff;

            var tilemap = PPU.BGTileMapDisplaySelect; //This one has the background tiles, the other similar
            for (int tileNumber = 0; tileNumber < 20; tileNumber++)
            {
                var xOffset = ((PPU.SCX / 8) + tileNumber) & 0x1f;
                //named field has the sprite tiles. We don't need sprites for the logo

                var currentTileIndex = PPU.VRAM[tilemap + xOffset + (yOffset * 4)];

                var tileData = PPU.BGAndWindowTileDataSelect;

                var at = tileData + (currentTileIndex * 16) + (yOffset % 8 * 2);

                var tileDataLow = PPU.VRAM[at];
                var tileDataHigh = PPU.VRAM[at + 1]; //This location is incorrect

                if (tileDataLow + tileDataHigh != 0)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }

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