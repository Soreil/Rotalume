using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emulator.cpu
{
    public static class Constants
    {
        public const int Frequency = 1 << 22;
        public const int TicksPerInstructionStep = 4;
    }
}
