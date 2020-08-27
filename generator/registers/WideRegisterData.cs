using System;

namespace generator
{
    public class WideRegisterData
    {
        private ushort value;
        public RegisterData Low;
        public RegisterData High;

        public Func<ushort> Read;
        public Action<ushort> Write;

        public WideRegisterData()
        {
            Read = () => value;
            Write = (x) => value = x;

            Low = new RegisterData
            {
                Read = () => (byte)(value & 0x00ff),
                Write = (x) => value = (ushort)((value & 0xff00) + x)
            };

            High = new RegisterData
            {
                Read = () => (byte)((value & 0xff00) >> 8),
                Write = (x) => value = (ushort)((value & 0x00ff) + (x << 8))
            };
        }
    }
}
