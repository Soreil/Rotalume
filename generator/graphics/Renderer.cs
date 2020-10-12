using System;

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
        int clocksInFrame => Clock() % TicksPerFrame;
        int line => clocksInFrame / TicksPerScanline;
        int clockInScanline => clocksInFrame % TicksPerScanline;
        private void UpdateLineRegister()
        {
            PPU.LY = (byte)(line % ScanlinesPerFrame);
            PPU.LYCInterrupt = PPU.LY == PPU.LYC;
        }

        public Renderer(PPU ppu)
        {
            PPU = ppu;
            var startTime = PPU.Clock();
            Clock = () => ppu.Clock() - startTime;
        }
        public void Render()
        {
            var currentTime = Clock();
            if (currentTime > TimeUntilWhichToPause)
            {
                UpdateLineRegister();
                SetNewClockTarget();
                IncrementMode();
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

        private bool FinalStageOfFinalPrintedLine() => (line == DrawlinesPerFrame && PPU.Mode == Mode.HBlank);

        private void SetNewClockTarget() => TimeUntilWhichToPause += PPU.Mode switch
        {
            Mode.OAMSearch => 80,
            Mode.Transfer => 172, //Transfer can take longer than this, what matters is that  transfer and hblank add up to be 376
            Mode.HBlank => 204, //HBlank can take shorter than this
            Mode.VBlank => 4560, //
            _ => throw new NotImplementedException(),
        };

    }
}