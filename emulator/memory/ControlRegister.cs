using System;

namespace emulator
{
    public class ControlRegister
    {

        public Action<byte>[] Writer;
        public Func<byte>[] Reader;
        public int Start;
        public int Size;

        public ControlRegister(ushort startAddress, int size)
        {
            Writer = new Action<byte>[size];
            Reader = new Func<byte>[size];
            Start = startAddress;
            Size = size;

            for (int i = 0; i < size; i++) Writer[i] = (x) => { };
        }
        public byte this[int at]
        {
            get => Reader[at - Start]();
            set => Writer[at - Start](value);
        }
    }
}