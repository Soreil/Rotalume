namespace emulator
{
    public class ControlRegister
    {
        public delegate void Write(byte b);
        public delegate byte Read();

        public Write[] Writer;
        public Read[] Reader;
        public int Start;
        public int Size;

        public ControlRegister(ushort startAddress, int size)
        {
            Writer = new Write[size];
            Reader = new Read[size];
            Start = startAddress;
            Size = size;
        }

        public byte this[int at]
        {
            get => Reader[at - Start]();
            set => Writer[at - Start](value);
        }

        public bool ContainsWriter(int at) => Writer[at - Start] != null;
        public bool ContainsReader(int at) => Reader[at - Start] != null;
    }
}