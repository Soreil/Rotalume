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

    public void Push8(ReadOnlySpan<T> p)
    {
        System.Diagnostics.Debug.Assert(p.Length == 8);
        p.CopyTo(buffer.AsSpan((position + Count) & mask));
        Count += 8;
    }

    public T Pop()
    {
        Count--;
        return buffer[position++ & mask];
    }

    public ref T At(int at) => ref buffer[(position + at) & mask];
}
