namespace emulator
{
    public record Operand
    {
        public string Name;
        public int? Size;
        public bool Pointer;
        public Postfix Postfix;

        public Operand(string name, bool pointer)
        {
            Name = name;
            Size = null;
            Postfix = Postfix.unchanged;
            Pointer = pointer;
        }
    }
}