namespace emulator;

public class FIFO<T>
{
    //Whether or not the FIFO actually holds 16 or 8 items isn't entirely clear to me.
    private const int Capacity = 16;
    private const int mask = Capacity - 1;
    private int Position;
    public int Count;
    private readonly T[] buffer = new T[Capacity];
    public void Clear()
    {
        Count = 0;
        Position = 0;
    }

    public void Push(T p) => buffer[(Position + Count++) & mask] = p;

    public T Pop()
    {
        Count--;
        return buffer[Position++ & mask];
    }
    public void Replace(int at, T p) => buffer[(Position + at) & mask] = p;

    public T At(int at) => buffer[(Position + at) & mask];
}
