using System;

namespace emulator
{
    public class FIFO<T>
    {
        public const int capacity = 16;
        public int position = 0;
        public int count = 0;
        private readonly T[] buffer = new T[capacity];
        public void Clear()
        {
            count = 0;
            position = 0;
        }

        public void Push(T p) => buffer[(position + count++) & (capacity - 1)] = p;
        public T Pop()
        {
            
            if (count == 0) throw new Exception("Empty FIFO");

            count--;
            return buffer[position++&(capacity-1)];
        }
        public void Replace(int at, T p) => buffer[(position + at) & (capacity - 1)] = p;
        public T At(int at) => buffer[(position + at) & (capacity - 1)];
    }
}