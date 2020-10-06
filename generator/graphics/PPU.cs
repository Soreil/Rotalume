using System;
using System.Diagnostics;

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

        public void DoPPU(int currentTime)
        {
            if (LCDEnable && TimePPUWasStarted == 0)
            {
                TimePPUWasStarted = currentTime;
                Mode = Mode.OAMSearch;
                return;
            }
            else if (!LCDEnable)
            {
                TimePPUWasStarted = 0;
                return;
            }

            var delta = (currentTime - TimePPUWasStarted) - TimeSince;
            Step(delta);
        }

        private void Step(int delta)
        {
            var newTime = TimeSince + delta;
            if (newTime > TimeUntilWhichToPause)
            {
                var oldMode = Mode;

                SetNewClockTarget();

                Mode newMode;
                if (!(line == DrawlinesPerFrame && oldMode == Mode.HBlank))
                {
                    newMode = oldMode switch
                    {
                        Mode.OAMSearch => Mode.Transfer,
                        Mode.Transfer => Mode.HBlank,
                        Mode.HBlank => Mode.OAMSearch,
                        Mode.VBlank => Mode.OAMSearch, //This isn't great, we should handle VBlank a line at a time instead of a block
                        _ => throw new NotImplementedException(),
                    };
                }
                else newMode = Mode.VBlank;
                Mode = newMode;
            }
            TimeSince = newTime;
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
