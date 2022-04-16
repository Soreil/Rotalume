using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class IllegalDMAAdress : Exception
{
    public IllegalDMAAdress()
    {
    }

    public IllegalDMAAdress(string? message) : base(message)
    {
    }

    public IllegalDMAAdress(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected IllegalDMAAdress(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}