namespace emulator.graphics;

//PPU Adresses for the DMG
public enum Address : ushort
{
    LCDC = 0xff40,
    STAT = 0xff41,
    SCY = 0xff42,
    SCX = 0xff43,
    LY = 0xff44,
    LYC = 0xff45,
    DMA = 0xff46,
    WY = 0xff4a,
    WX = 0xff4b,
    BGP = 0xff47,
    OBP0 = 0xff48,
    OBP1 = 0xff49,

}
