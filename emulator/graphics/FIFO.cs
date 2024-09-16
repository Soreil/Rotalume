namespace emulator.graphics;

public class FIFO<T>
{
    //FIXME capacity is actually 8
    private const int capacity = 16;
    private const int mask = capacity - 1;
    private int position;
    public int Count { get; private set; }
    private readonly T[] buffer = new T[capacity];

    public void Clear()
    {
        Count = 0;
        position = 0;
    }

    public void Push(T p) => buffer[(position + Count++) & mask] = p;

    public T Pop()
    {
        Count--;
        return buffer[position++ & mask];
    }
    public void Replace(int at, T p) => buffer[(position + at) & mask] = p;

    public T At(int at) => buffer[(position + at) & mask];
}
