using System;

namespace emulator
{
    public class Renderer
    {
        readonly PPU PPU;
        public long TimeUntilWhichToPause;
        readonly FrameSink fs;

        public const int TileWidth = 8;
        public const int DisplayWidth = 160;
        public const int TilesPerLine = DisplayWidth / TileWidth;
        public const int DisplayHeight = 144;
        public const int ScanlinesPerFrame = DisplayHeight + 10;

        public const int TicksPerScanline = 456;
        public const int TicksPerFrame = ScanlinesPerFrame * TicksPerScanline;

        public Renderer(PPU ppu, FrameSink destination = null, long offset = 0)
        {
            fs = destination ?? new();
            PPU = ppu;
            ppu.Mode = Mode.OAMSearch;
            fetcher = new PixelFetcher(PPU);
            TimeUntilWhichToPause += offset;
        }

        public PixelFetcher fetcher;
        public int Stage3TickCount = 0;

        public int PixelsPopped = 0;
        public int PixelsSentToLCD = 0;

        public int TotalTimeSpentInStage3 { get; private set; } = 0;

        Mode? ScheduledModeChange = null;
        public void Render()
        {
            //Increment mode and set lock states
            if (ScheduledModeChange is not null)
            {
                PPU.Mode = (Mode)ScheduledModeChange;
                ScheduledModeChange = null;

                if (PPU.Mode == Mode.OAMSearch)
                {
                    PPU.OAM.Locked = true;
                    PPU.VRAM.Locked = false;
                }
                if (PPU.Mode == Mode.Transfer)
                {
                    PPU.OAM.Locked = true;
                    PPU.VRAM.Locked = true;
                }

                if (PPU.Mode == Mode.HBlank)
                {
                    PPU.OAM.Locked = false;
                    PPU.VRAM.Locked = false;
                }
                if (PPU.Mode == Mode.VBlank)
                {
                    PPU.OAM.Locked = false;
                    PPU.VRAM.Locked = false;

                    //According to TCAGBD the OAM flag is also triggering on this
                    if (PPU.Enable_VBlankInterrupt || PPU.Enable_OAM_Interrupt)
                        PPU.EnableLCDCStatusInterrupt();

                    PPU.EnableVBlankInterrupt();
                    fs.Draw();
                }
            }

            //We should be handling this during the transition from HBlank to OAMSearch
            if (PPU.Mode == Mode.OAMSearch || PPU.Mode == Mode.VBlank)
            {
                //We only want to increment the line register if we aren't on the very first line
                if (fs.Position != 0 || PPU.Mode == Mode.VBlank)
                    PPU.LY++;

                if (PPU.LY == PPU.LYC) PPU.LYCInterrupt = true;

                if (PPU.LY == 154)
                {
                    PPU.LY = 0;
                    fetcher.FrameFinished();
                    ScheduledModeChange = Mode.OAMSearch;
                    return;
                }
            }

            if (PPU.Mode == Mode.HBlank)
            {
                if (PPU.Enable_HBlankInterrupt)
                    PPU.EnableLCDCStatusInterrupt();
                TimeUntilWhichToPause += 376 - TotalTimeSpentInStage3;

                ScheduledModeChange = PPU.LY == 143 ? Mode.VBlank : Mode.OAMSearch;
                return;
            }
            else if (PPU.Mode == Mode.OAMSearch)
            {
                if (PPU.Enable_OAM_Interrupt)
                    PPU.EnableLCDCStatusInterrupt();
                fetcher.SpriteAttributes = PPU.OAM.SpritesOnLine(PPU.LY, PPU.SpriteHeight);
                TimeUntilWhichToPause += 80;
                ScheduledModeChange = Mode.Transfer;
                return;
            }
            else if (PPU.Mode == Mode.VBlank)
            {
                TimeUntilWhichToPause += TicksPerScanline;
                return;
            }
            else if (PPU.Mode == Mode.Transfer)
            {
                if (PPU.LY != 0 && Stage3TickCount == 0)
                {
                    Stage3TickCount += 4;
                    TimeUntilWhichToPause += 4;
                    return;
                }

                var cycles = fetcher.Fetch();

                //We should probably be doing this in a more clean way since it just needs
                //to handle on every cycle
                for (int i = 0; i < cycles && PixelsSentToLCD < 160; i++)
                {
                    AttemptToPushAPixel();
                }

                Stage3TickCount += cycles;
                TimeUntilWhichToPause += cycles;

                if (PixelsSentToLCD == 160)
                {
                    ResetLineSpecificState();
                }
                return;
            }
        }

        //Reusable buffer
        readonly byte[] output = new byte[DisplayWidth];
        private void ResetLineSpecificState()
        {
            ScheduledModeChange = Mode.HBlank;

            for (int i = 0; i < output.Length; i++)
                output[i] = ShadeToGray(background[i]);
            fs.Write(output);
            fetcher.LineFinished();
            PixelsPopped = 0;
            PixelsSentToLCD = 0;
            TotalTimeSpentInStage3 = Stage3TickCount;
            Stage3TickCount = 0;
        }

        private void AttemptToPushAPixel()
        {
            var pix = fetcher.RenderPixel();
            if (pix != Shade.Empty)
            {
                PixelsPopped++;
                fetcher.scanlineX++;
                if (PixelsPopped > (PPU.SCX & 7))
                    background[PixelsSentToLCD++] = (Shade)pix;
                bool windowStart = PixelsSentToLCD == PPU.WX - 7 && PPU.LY >= PPU.WY && PPU.WindowDisplayEnable;
                if (windowStart)
                {
                    fetcher.FetcherStep = 0;
                    fetcher.BGFIFO.Clear();
                }
            }
        }

        private readonly Shade[] background = new Shade[DisplayWidth];
        public static byte ShadeToGray(Shade s) => s switch
        {
            Shade.White => 0xff,
            Shade.LightGray => 0xc0,
            Shade.DarkGray => 0x40,
            Shade.Black => 0,
            _ => throw new Exception(),
        };
    }
}