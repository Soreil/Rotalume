using System;

namespace generator
{
    public struct Operand
    {
        public string Name;
        public int? Size;
        public bool Increment;
        public bool Decrement;
        public bool Pointer;

        public Operand(string name, bool pointer)
        {
            Name = name;
            Size = null;
            Increment = false;
            Decrement = false;
            Pointer = pointer;
        }
    }

    public class Register
    {
        public Func<byte> Read;
        public Action<byte> Write;
    }

    public class WideRegister
    {
        private ushort value = 0;
        public Register Low;
        public Register High;

        public Func<ushort> Read;
        public Action<ushort> Write;

        public WideRegister()
        {
            Read = () => value;
            Write = (x) => value = x;

            Low = new Register
            {
                Read = () => (byte)(value & 0x00ff),
                Write = (x) => value = (ushort)((value & 0xff00) + x)
            };

            High = new Register
            {
                Read = () => (byte)((value & 0xff00) >> 8),
                Write = (x) => value = (ushort)((value & 0x00ff) + (x << 8))
            };
        }
    }

    public class Storage
    {
        public WideRegister AF;
        public Register A;
        public Register F;

        public Storage()
        {
            AF = new WideRegister();
            A = AF.High;
            F = AF.Low;
        }
    }
}
