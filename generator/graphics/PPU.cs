using System;

namespace generator
{
    public class PPU
    {
        public readonly Func<int> Clock;
        public PPU(Func<int> clock)
        {
            Clock = clock;
        }

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

        public Mode Mode
        {
            get => (Mode)(STAT & 0x03);
            set => STAT = (byte)(STAT & 0xFC | (int)value & 0x3);
        }

        public bool LYCInterrupt
        {
            get => STAT.GetBit(2);
            set => STAT.SetBit(2, value);
        }

        private Renderer Renderer;
        public void Do()
        {
            if (Renderer is not null)
                Renderer.Render();
        }
        public void SetLCDC(byte b)
        {
            LCDC = b;
            if (ScreenJustTurnedOn)
                Renderer = new Renderer(this);
            else if (!LCDEnable && Renderer is not null)
                Renderer = null;
        }
        private bool ScreenJustTurnedOn => LCDEnable && Renderer is null;
    }
}
