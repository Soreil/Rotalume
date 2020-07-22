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
}
