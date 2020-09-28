namespace generator
{
    public delegate void ControlRegisterWrite(byte b);
    public delegate byte ControlRegisterRead();
    public class ControlRegisters
    {
        public ControlRegisterWrite[] WriteHandlers;
        public ControlRegisterRead[] ReadHandlers;
        public int Start;
        public ControlRegisters(ushort startAddress, int size)
        {
            WriteHandlers = new ControlRegisterWrite[size];
            ReadHandlers = new ControlRegisterRead[size];
            Start = startAddress;
        }

        public byte this[int at]
        {
            get => ReadHandlers[at - Start]();
            set => WriteHandlers[at - Start](value);
        }

        public bool ContainsWriter(int at) => WriteHandlers[at - Start] != null;
        public bool ContainsReader(int at) => ReadHandlers[at - Start] != null;
    }
}