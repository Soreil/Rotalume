
using emulator.extensions;
using emulator.opcodes;

using Microsoft.Extensions.Logging;

namespace emulator.graphics;


public class PPUState
{
    public required long Clock { get; init; }

    //These flags are what LCDC register bits are set to, not the actual LCDC register value
    public required bool LCDEnable { get; init; }
    public required bool WindowTileMapSelect { get; init; }
    public required bool WindowDisplayEnable { get; init; }
    public required bool BGAndWindowTileDataSelectFlag { get; init; }
    public required bool BGTileMapDisplaySelectFlag { get; init; }
    public required bool DoubleHeightSprites { get; init; }
    public required bool OBJDisplayEnable { get; init; }
    public required bool BGDisplayEnable { get; init; }

    //Not going to bother unpacking that STAT register, these are just interrupt flags
    public required byte STAT { get; init; }

    public required byte SCY { get; init; } //FF42
    public required byte SCX { get; init; } //FF43
            
    public required byte LY { get; init; } //FF44
    public required byte LYC { get; init; } //FF45

    public required byte BGP { get; init; } //FF47
    public required byte OBP0 { get; init; } //FF48
    public required byte OBP1 { get; init; } //FF49

    public required byte WY { get; init; } //FF4A
    public required byte WX { get; init; } //FF4B

}

public class PPU
{
    private long Clock;
    private readonly IFrameSink Writer;
    public PPU(IFrameSink frameSink, InterruptRegisters interruptRegisters, OAM OAM, VRAM VRAM, ILogger<PPU> logger)
    {
        Writer = frameSink;
        this.OAM = OAM;
        this.VRAM = VRAM;
        this.logger = logger;
        VBlankInterrupt += interruptRegisters.EnableVBlankInterrupt;
        STATInterrupt += interruptRegisters.EnableLCDSTATInterrupt;
    }

    private readonly OAM OAM;
    private readonly VRAM VRAM;
    private readonly ILogger<PPU> logger;

    private bool LCDEnable { get; set; }
    private bool WindowTileMapSelect { get; set; }
    public ushort TileMapDisplaySelect => (ushort)(WindowTileMapSelect ? VRAM.TileMap1Start : VRAM.TileMap0Start);
    public bool WindowDisplayEnable { get; private set; }
    private bool BGAndWindowTileDataSelectFlag { get; set; }
    public ushort BGAndWindowTileDataSelect => (ushort)(BGAndWindowTileDataSelectFlag ? VRAM.TileBlock0Start : VRAM.TileBlock2Start);
    private bool BGTileMapDisplaySelectFlag { get; set; }
    public ushort BGTileMapDisplaySelect => (ushort)(BGTileMapDisplaySelectFlag ? VRAM.TileMap1Start : VRAM.TileMap0Start);
    private bool DoubleHeightSprites { get; set; }
    public int SpriteHeight => DoubleHeightSprites ? 16 : 8;
    public bool OBJDisplayEnable { get; private set; }
    public bool BGDisplayEnable { get; private set; }
    private bool PreviousBGDisplayEnable;
    private long BGEnableEffectiveClock;
    // LCDC readback changes immediately; pixel output retains bit 0 for one dot.
    internal bool BGEnabledForOutput => Clock < BGEnableEffectiveClock ? PreviousBGDisplayEnable : BGDisplayEnable;


    //FF40 - FF4B, PPU control registers
    //FF40 
    private byte LCDC
    {
        get => (byte)(Convert.ToByte(LCDEnable) << 7 |
            Convert.ToByte(WindowTileMapSelect) << 6 |
            Convert.ToByte(WindowDisplayEnable) << 5 |
            Convert.ToByte(BGAndWindowTileDataSelectFlag) << 4 |
            Convert.ToByte(BGTileMapDisplaySelectFlag) << 3 |
            Convert.ToByte(DoubleHeightSprites) << 2 |
            Convert.ToByte(OBJDisplayEnable) << 1 |
            Convert.ToByte(BGDisplayEnable));
        set
        {
            LCDEnable = value.GetBit(7);
            WindowTileMapSelect = value.GetBit(6);
            WindowDisplayEnable = value.GetBit(5);
            BGAndWindowTileDataSelectFlag = value.GetBit(4);
            BGTileMapDisplaySelectFlag = value.GetBit(3);
            DoubleHeightSprites = value.GetBit(2);
            OBJDisplayEnable = value.GetBit(1);
            if (BGDisplayEnable != value.GetBit(0))
            {
                PreviousBGDisplayEnable = BGEnabledForOutput;
                // Before the first LCD pixel there is no preceding output state
                // to retain, including while the initial object fetch is stalled.
                BGEnableEffectiveClock = Clock +
                    (Mode == Mode.Transfer && Renderer is not null && Renderer.fetcher.PixelsSentToLCD > 0 ? 2 : 1);
            }
            BGDisplayEnable = value.GetBit(0);
            if (ScreenJustTurnedOn)
            {
                logger.LogInformation("Turning on PPU");
                Writer.Resume();
                Renderer = new Renderer(this, OAM, VRAM, Writer, Clock - 4); //We want a new renderer so all the internal state resets including clocking
            }
            else if ((!LCDEnable) && Renderer is not null)
            {
                logger.LogInformation("Turning off PPU");
                Writer.Draw(); //If there is a partially written frame when we delete the old renderer the next
                               //instantiation of renderer will overwrite the end of the buffer because LY starts at 0 despite
                               //there already being data written to the output buffer

                Writer.Pause(); //We are not currently drawing a frame so it would be pointless to measure the duration. 

                Renderer = null; //We want to destroy the old renderer so it can't keep running after requested to turn off
                LY = 0;
                Mode = Mode.HBlank;
                VRAM.Locked = false;
                OAM.Locked = false;
            }
        }
    }

    internal void SetStateWithoutBootrom()
    {
        LCDC = 0x91;

        SCY = 0;
        SCX = 0;
        WY = 0;
        WX = 0;
        LYC = 0;
        LY = 1;

        BGP = 0xfc;
        OBP0 = 0xff;
        OBP1 = 0xff;
    }

    //FF41      
    private byte STAT
    {
        get;
        set => field = (byte)((value & 0x7f) | 0x80);
    } = 0x80;

    private bool Enable_LYC_Compare => STAT.GetBit(6);
    public bool Enable_OAM_Interrupt => STAT.GetBit(5);
    public bool Enable_VBlankInterrupt => STAT.GetBit(4);
    public bool Enable_HBlankInterrupt => STAT.GetBit(3);

    public byte SCY; //FF42
    public byte SCX; //FF43

    public byte LY; //FF44
    public byte LYC; //FF45

    //DMA register is located outside of the PPU for our implementation

    private byte BGP = 0xff; //FF47
    private byte PreviousBGP;
    private long BGPTransitionClock = -1;
    private byte OBP0 = 0xff; //FF48
    private byte OBP1 = 0xff; //FF49

    public byte WY; //FF4A
    public byte WX; //FF4B

    public Shade SpritePalette0(int n) => n switch
    {
        1 => (Shade)((OBP0 & 0xC) >> 2),
        2 => (Shade)((OBP0 & 0x30) >> 4),
        3 => (Shade)((OBP0 & 0xC0) >> 6),
        _ => throw new IndexOutOfRangeException()
    };

    public Shade SpritePalette1(int n) => n switch
    {
        1 => (Shade)((OBP1 & 0xC) >> 2),
        2 => (Shade)((OBP1 & 0x30) >> 4),
        3 => (Shade)((OBP1 & 0xC0) >> 6),
        _ => throw new IndexOutOfRangeException()
    };

    // On the first dot following a DMG BGP write, old and new palette bits overlap.
    // The register itself already reads back the new value.
    private byte BackgroundPalette => Clock == BGPTransitionClock ? (byte)(PreviousBGP | BGP) : BGP;

    public Shade BackgroundColor(int n) => n switch
    {
        0 => (Shade)((BackgroundPalette & 0x3) >> 0),
        1 => (Shade)((BackgroundPalette & 0xC) >> 2),
        2 => (Shade)((BackgroundPalette & 0x30) >> 4),
        3 => (Shade)((BackgroundPalette & 0xC0) >> 6),
        _ => throw new IndexOutOfRangeException()
    };

    // OAM's first 8-byte row (the first four dots of mode 2) cannot be corrupted.
    internal bool CanCorruptOAM => LCDEnable && Mode == Mode.OAMSearch &&
        Renderer is not null &&
        Clock < Renderer.TimeUntilWhichToPause &&
        Clock >= Renderer.TimeUntilWhichToPause - GraphicConstants.OAMSearchDuration + 4;

    // Each machine cycle scans two sprites, i.e. one eight-byte OAM row.
    internal int OAMScanRow => Renderer is null ? -1 :
        (int)((Clock - Renderer.TimeUntilWhichToPause + GraphicConstants.OAMSearchDuration) / 4);

    public Mode Mode
    {
        get => (Mode)(STAT & 0x03);
        set => STAT = (byte)(STAT & 0xFC | (int)value & 0x3);
    }

    private event EventHandler? STATInterrupt;
    private event EventHandler? VBlankInterrupt;

    public void OnSTATInterrupt() => STATInterrupt?.Invoke(this, EventArgs.Empty);
    public void OnVBlankInterrupt() => VBlankInterrupt?.Invoke(this, EventArgs.Empty);

    public bool LYCInterrupt
    {
        get => STAT.GetBit(2);
        set
        {
            var stat = STAT;
            stat.SetBit(2, value);
            STAT = stat;
            if (Enable_LYC_Compare && value)
            {
                OnSTATInterrupt();
            }
        }
    }

    private Renderer? Renderer;
    public void Tick()
    {
        Clock++;
        if (Renderer is not null)
        {
            while (Clock >= Renderer.TimeUntilWhichToPause)
            {
                Renderer.Render();
            }
        }
    }

    internal PPUState SerializeState()
    {
        return new PPUState
        {
            Clock = Clock,
            LCDEnable = LCDEnable,
            WindowTileMapSelect = WindowTileMapSelect,
            WindowDisplayEnable = WindowDisplayEnable,
            BGAndWindowTileDataSelectFlag = BGAndWindowTileDataSelectFlag,
            BGTileMapDisplaySelectFlag = BGTileMapDisplaySelectFlag,
            DoubleHeightSprites = DoubleHeightSprites,
            OBJDisplayEnable = OBJDisplayEnable,
            BGDisplayEnable = BGDisplayEnable,
            STAT = STAT,
            SCY = SCY,
            SCX = SCX,
            LY = LY,
            LYC = LYC,
            WY = WY,
            WX = WX,
            BGP = BGP,
            OBP0 = OBP0,
            OBP1 = OBP1
        };
    }

    //We could have more calls to SetLCDC for other bits in the LCDC register.
    //The LCDCEnable flag is only interesting at the moment it flips and the renderer null check should mean a recent flip
    private bool ScreenJustTurnedOn => LCDEnable && Renderer is null;

    public byte this[Address addr]
    {
        get => addr switch
        {
            Address.LCDC => LCDC,
            Address.STAT => STAT,
            Address.SCY => SCY,
            Address.SCX => SCX,
            Address.LY => LY,
            Address.LYC => LYC,
            Address.WY => WY,
            Address.WX => WX,
            Address.BGP => BGP,
            Address.OBP0 => OBP0,
            Address.OBP1 => OBP1,
            _ => 0xff
        };


        set
        {
            switch (addr)
            {
                case Address.LCDC: LCDC = value; break;
                case Address.STAT: STAT = (byte)((value & 0xf8) | (STAT & 0x7)); break;
                case Address.SCY: SCY = value; break;
                case Address.SCX: SCX = value; break;
                case Address.LY: LY = value; break;
                case Address.LYC: LYC = value; break;
                case Address.WY: WY = value; break;
                case Address.WX: WX = value; break;
                case Address.BGP:
                PreviousBGP = BGP;
                BGPTransitionClock = Clock + 1;
                BGP = value;
                break;
                case Address.OBP0: OBP0 = value; break;
                case Address.OBP1: OBP1 = value; break;
            }
        }
    }
}
