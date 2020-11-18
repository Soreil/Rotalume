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
                SetNewClockTarget();
                IncrementMode();
            }
        }

        private void Draw()
        {
            int totalCycles = 0;

            GetTile();
        }

        private void GetTile()
        {
            for (int tile = 0; tile < 20; tile++)
            {
                var x = ((PPU.SCX / 8) + tile * 8) & 0x1f; //Not sure if we need the *8
                var y = (PPU.LY + PPU.SCY) & 0xff;

                var tilemap = PPU.BGAndWindowTileDataSelect;
                var tileDataLow = PPU.VRAM[tilemap + tile * 2];
                var tileDataHigh = PPU.VRAM[tilemap + tile * 2 + 1];
            }
        }

        private void IncrementMode()
        {
            if (!FinalStageOfFinalPrintedLine())
            {
                PPU.Mode = PPU.Mode switch
                {
                    Mode.OAMSearch => Mode.Transfer,
                    Mode.Transfer => Mode.HBlank,
                    Mode.HBlank => Mode.OAMSearch,
                    Mode.VBlank => Mode.OAMSearch, //This isn't great, we should handle VBlank a line at a time instead of a block
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                PPU.Mode = Mode.VBlank;
            }
        }

        private bool FinalStageOfFinalPrintedLine() => (Line == DrawlinesPerFrame && PPU.Mode == Mode.HBlank);

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