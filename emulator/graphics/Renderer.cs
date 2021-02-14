﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace emulator
{
    public class Renderer
    {
        readonly PPU PPU;
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
            ppu.Mode = Mode.OAMSearch;
            fetcher = new PixelFetcher(PPU);
        }

        public List<SpriteAttributes> SpriteAttributes = new();
        public PixelFetcher fetcher;
        public int Stage3TickCount = 0;

        public int PixelsPopped = 0;
        public int PixelsSentToLCD = 0;

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
                    fs.Flush();
                }
            }

            //We should be handling this during the transition from HBlank to OAMSearch
            if (PPU.Mode == Mode.OAMSearch || PPU.Mode == Mode.VBlank)
            {
                //We only want to increment the line register if we aren't on the very first line
                if (fs.Position != 0 || PPU.Mode == Mode.VBlank)
                    PPU.LY++;

                if (PPU.LY == PPU.LYC) PPU.LYCInterrupt = true;
                if (PPU.LYCInterrupt && PPU.Enable_LYC_Compare)
                    PPU.EnableLCDCStatusInterrupt();

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
                TimeUntilWhichToPause += 376 - Stage3TickCount;

                ScheduledModeChange = PPU.LY == 143 ? Mode.VBlank : Mode.OAMSearch;
                return;
            }
            else if (PPU.Mode == Mode.OAMSearch)
            {
                if (PPU.Enable_OAM_Interrupt)
                    PPU.EnableLCDCStatusInterrupt();
                SpriteAttributes = PPU.OAM.SpritesOnLine(PPU.LY, PPU.SpriteHeight);
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
                var count = fetcher.Fetch();
                for (int i = 0; i < count && PixelsSentToLCD < 160; i++)
                {
                    var pix = fetcher.RenderPixel();
                    if (pix != null)
                    {
                        PixelsPopped++;
                        if (PixelsPopped > (PPU.SCX & 7))
                            background[PixelsSentToLCD++] = (Shade)pix;
                    }
                }

                Stage3TickCount += count;
                TimeUntilWhichToPause += count;
                //We have to execute this until the full line is drawn by renderpixel calls
            }

            if (PPU.Mode == Mode.Transfer && PixelsSentToLCD == 160)
            {
                ScheduledModeChange = Mode.HBlank;

                var output = new byte[background.Length];
                for (int i = 0; i < output.Length; i++)
                    output[i] = ShadeToGray(background[i]);
                fs.Write(output);
                fetcher.LineFinished();
                Stage3TickCount = 0;
                PixelsPopped = 0;
                PixelsSentToLCD = 0;

                return;
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