using generator;

namespace Tests
{
    public record PPU
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
        ushort TileMapDisplaySelect
        {
            get
            {
                if (LCDC.GetBit(6)) return 0x9C00;
                else return 0x9800;
            }
        }

        ushort BGAndWindowTileDataSelect
        {
            get
            {
                if (LCDC.GetBit(4)) return 0x8000;
                else return 0x8800;
            }
        }
        ushort BGTileMapDisplaySelect
        {
            get
            {
                if (LCDC.GetBit(3)) return 0x9c00;
                else return 0x9800;
            }
        }

        int SpriteHeight
        {
            get
            {
                if (LCDC.GetBit(2)) return 16;
                else return 8;
            }
        }

        private bool LCDEnable() => LCDC.GetBit(7);
        private bool WindowDisplayEnable() => LCDC.GetBit(5);
        private bool OBJDisplayEnable() => LCDC.GetBit(1);
        private bool BGOrWindowDisplayOrPriority() => LCDC.GetBit(0);

        byte STAT; //FF41


    }
}
