namespace emulator
{
    public class FIFO<T>
    {
        private const int Capacity = 16;
        private int Position;
        public int Count { get; private set; }
        private readonly T[] buffer = new T[Capacity];
        public void Clear()
        {
            Count = 0;
            Position = 0;
        }

        public void Push(T p) => buffer[(Position + Count++) & (Capacity - 1)] = p;

        public T Pop()
        {
            Count--;
            return buffer[Position++ & (Capacity - 1)];
        }
        public void Replace(int at, T p) => buffer[(Position + at) & (Capacity - 1)] = p;

        public T At(int at) => buffer[(Position + at) & (Capacity - 1)];
    }
}