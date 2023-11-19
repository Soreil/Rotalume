namespace emulator.graphics;

public class Renderer
{
    private readonly PPU PPU;
    private readonly OAM OAM;
    private readonly VRAM VRAM;
    public long TimeUntilWhichToPause;
    private readonly IFrameSink fs;
    private bool SkippingLYIncrementBecauseStartingLineOne = true;
    public Renderer(PPU ppu, OAM oam, VRAM vram, IFrameSink destination, long offset)
    {
        fs = destination;
        PPU = ppu;
        OAM = oam;
        VRAM = vram;
        ppu.Mode = Mode.OAMSearch;
        fetcher = new PixelFetcher(PPU, VRAM, OAM);
        TimeUntilWhichToPause += offset;
    }

    public PixelFetcher fetcher;
    public int Stage3TickCount;

    public int TotalTimeSpentInStage3 { get; private set; }

    private bool ModeChangeRequested;
    private Mode ModeChangeRequest;
    public void Render()
    {
        //Increment mode and set lock states
        if (ModeChangeRequested)
        {
            PPU.Mode = ModeChangeRequest;
            ModeChangeRequested = false;

            if (PPU.Mode == Mode.OAMSearch)
            {
                OAM.Locked = true;
                VRAM.Locked = false;
            }
            else if (PPU.Mode == Mode.Transfer)
            {
                OAM.Locked = true;
                VRAM.Locked = true;
            }

            else if (PPU.Mode == Mode.HBlank)
            {
                OAM.Locked = false;
                VRAM.Locked = false;
            }
            else
            {
                OAM.Locked = false;
                VRAM.Locked = false;

                //According to TCAGBD the OAM flag is also triggering on this
                if (PPU.Enable_VBlankInterrupt || PPU.Enable_OAM_Interrupt)
                {
                    PPU.OnSTATInterrupt();
                }

                PPU.OnVBlankInterrupt();
                fs.Draw();
            }
        }

        //We should be handling this during the transition from HBlank to OAMSearch
        if (PPU.Mode is Mode.OAMSearch or Mode.VBlank)
        {
            //We only want to increment the line register if we aren't on the very first line
            if (!SkippingLYIncrementBecauseStartingLineOne) PPU.LY++;
            else SkippingLYIncrementBecauseStartingLineOne = false;

            if (PPU.LY == PPU.LYC) PPU.LYCInterrupt = true;

            if (PPU.LY == 154)
            {
                PPU.LY = 0;
                fetcher.FrameFinished();
                ModeChangeRequested = true;
                ModeChangeRequest = Mode.OAMSearch;
                SkippingLYIncrementBecauseStartingLineOne = true;
                return;
            }
        }
        ExecuteModeTick();
    }

    private void ExecuteModeTick()
    {
        switch (PPU.Mode)
        {
            case Mode.HBlank:
            HBlank();
            break;
            case Mode.OAMSearch:
            OAMSearch();
            break;
            case Mode.VBlank:
            VBlank();
            break;
            case Mode.Transfer:
            Transfer();
            break;
        }
    }

    private void Transfer()
    {
        //TODO: Why is this here again? Shouldn't the 4 tick offset only be for LY 0 instead of what we have now?
        if (PPU.LY != 0 && Stage3TickCount == 0)
        {
            Stage3TickCount += 4;
            TimeUntilWhichToPause += 4;
            return;
        }

        fetcher.Fetch();
        fetcher.AttemptToPushAPixel();

        Stage3TickCount++;
        TimeUntilWhichToPause++;

        if (fetcher.PixelsSentToLCD == graphics.GraphicConstants.ScreenWidth)
            ResetLineSpecificState();
    }
    private void VBlank() => TimeUntilWhichToPause += graphics.GraphicConstants.ScanlineDuration;
    private void OAMSearch()
    {
        if (PPU.Enable_OAM_Interrupt)
            PPU.OnSTATInterrupt();

        fetcher.GetSprites();
        TimeUntilWhichToPause += graphics.GraphicConstants.OAMSearchDuration;
        ModeChangeRequested = true;
        ModeChangeRequest = Mode.Transfer;
    }

    private void HBlank()
    {
        if (PPU.Enable_HBlankInterrupt)
            PPU.OnSTATInterrupt();

        TimeUntilWhichToPause += graphics.GraphicConstants.ScanLineRemainderAfterOAMSearch - TotalTimeSpentInStage3;

        ModeChangeRequested = true;
        ModeChangeRequest = PPU.LY == 143 ? Mode.VBlank : Mode.OAMSearch;
        return;
    }

    private void ResetLineSpecificState()
    {
        ModeChangeRequested = true;
        ModeChangeRequest = Mode.HBlank;

        Span<byte> output = stackalloc byte[graphics.GraphicConstants.ScreenWidth];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = ShadeToGray(fetcher.LineShadeBuffer[i]);
        }

        fs.Write(output);
        fetcher.LineFinished();
        TotalTimeSpentInStage3 = Stage3TickCount;
        Stage3TickCount = 0;
    }

    public static byte ShadeToGray(Shade s) => s switch
    {
        Shade.White => 0xff,
        Shade.LightGray => 0xc0,
        Shade.DarkGray => 0x40,
        Shade.Black => 0,
        _ => throw new ShadeDoesNotRepresentAColourException(),
    };
}
