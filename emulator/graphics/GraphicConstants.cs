using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emulator.graphics
{
    public static class Constants
    {
        public const int ScreenWidth = 160;
        public const int ScreenHeight = 144;
        public const int SpriteWidth = 8;
        public const int SpriteHeight = 8;
        public const int DoubleSpriteHeight = SpriteHeight * 2;
         
        public const int OAMSearchDuration = 80;
        public const int ScanLineRemainderAfterOAMSearch = 376;
        public const int ScanlineDuration = ScanLineRemainderAfterOAMSearch + OAMSearchDuration;
        public const int VBlankLineCount = 10;
        public const int ScanLinesPerFrame = ScreenHeight + VBlankLineCount;
        public const int TicksPerFrame = ScanLinesPerFrame * ScanlineDuration;
        public const int BitsPerSpriteTile = 16;
    }
}
