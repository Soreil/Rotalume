namespace generator
{
    public delegate void ControlRegisterWrite(byte b);
    public delegate byte ControlRegisterRead();
    public class ControlRegisters
    {
        public ControlRegisterWrite[] WriteHandlers = new ControlRegisterWrite[0x80];
        public ControlRegisterRead[] ReadHandlers = new ControlRegisterRead[0x80];

        public byte this[int at]
        {
            get => ReadHandlers[at & 0xff]();
            set => WriteHandlers[at & 0xff](value);
        }

        public bool ContainsWriter(int at) => WriteHandlers[at & 0xff] != null;
        public bool ContainsReader(int at) => ReadHandlers[at & 0xff] != null;
    }
}