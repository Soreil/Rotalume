using System;

namespace generator
{
    public class PPU
    {
        //FF40 - FF4B, PPU control registers
        public byte SCY; //FF42
        public byte SCX; //FF43
        public byte LY; //FF44
        public byte LYC; //FF45
        public byte WV; //FF4A
        public byte WX; //FF4B

        public byte BGP; //FF47
        public byte OBP0; //FF48
        public byte OBP1; //FF49

        public byte DMA; //FF46

        public byte LCDC; //FF40
        byte STAT; //FF41

        ushort TileMapDisplaySelect => LCDC.GetBit(6) ? 0x9C00 : 0x9800;
        ushort BGAndWindowTileDataSelect => LCDC.GetBit(4) ? 0x8000 : 0x8800;
        ushort BGTileMapDisplaySelect => LCDC.GetBit(3) ? 0x9c00 : 0x9800;
        int SpriteHeight => LCDC.GetBit(2) ? 16 : 8;
        public bool LCDEnable => LCDC.GetBit(7);
        bool WindowDisplayEnable => LCDC.GetBit(5);
        bool OBJDisplayEnable => LCDC.GetBit(1);
        bool BGOrWindowDisplayOrPriority => LCDC.GetBit(0);

        private Mode Mode
        {
            get => (Mode)(STAT & 0x03);
            set => STAT = (byte)(STAT & 0xFC | (int)value & 0x3);
        }

        private bool LYCInterrupt
        {
            get => STAT.GetBit(2);
            set => STAT.SetBit(2, value);
        }

        public int TimePPUWasStarted;
        public int TimeSince;
        public int TimeUntilWhichToPause;

        const int DrawlinesPerFrame = 144;
        const int ScanlinesPerFrame = DrawlinesPerFrame + 10;

        const int TicksPerScanline = 456;
        const int TicksPerFrame = ScanlinesPerFrame * TicksPerScanline;
        int clocksInFrame => TimeSince % TicksPerFrame;
        int line => clocksInFrame / TicksPerScanline;
        int clockInScanline => clocksInFrame % TicksPerScanline;
        public int FramesDrawn = 0;
        public void DoPPU(int currentTime)
        {
            if (ScreenJustTurnedOn()) ReInitialize(currentTime);
            else if (!LCDEnable) TimePPUWasStarted = 0; //We only have to do this at the moment the screen is turned off, might be good to handle it in the write call to LCDC?
            else Step(currentTime);
        }

        private bool ScreenJustTurnedOn() => LCDEnable && TimePPUWasStarted == 0;

        private void ReInitialize(int currentTime)
        {
            TimePPUWasStarted = currentTime;
            Mode = Mode.OAMSearch;
        }

        private void Step(int currentTime)
        {
            if (currentTime > TimeUntilWhichToPause)
            {
                UpdateLineRegister();
                SetNewClockTarget();
                IncrementMode();
            }
            TimeSince = currentTime;
        }

        private void IncrementMode()
        {
            if (!FinalStageOfFinalPrintedLine())
            {
                Mode = Mode switch
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
                Mode = Mode.VBlank;
                FramesDrawn++;
            }
        }

        private bool FinalStageOfFinalPrintedLine() => (line == DrawlinesPerFrame && Mode == Mode.HBlank);

        private void UpdateLineRegister()
        {
            LY = (byte)(line % ScanlinesPerFrame);
            LYCInterrupt = LY == LYC;
        }

        private void SetNewClockTarget() => TimeUntilWhichToPause += Mode switch
        {
            Mode.OAMSearch => 80,
            Mode.Transfer => 172, //Transfer can take longer than this, what matters is that  transfer and hblank add up to be 376
            Mode.HBlank => 204, //HBlank can take shorter than this
            Mode.VBlank => 4560, //
            _ => throw new NotImplementedException(),
        };
    }
}
