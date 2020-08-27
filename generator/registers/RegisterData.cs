using System;

namespace generator
{
    public class RegisterData
    {
        public Func<byte> Read;
        public Action<byte> Write;
    }
}
