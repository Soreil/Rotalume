using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class NestedDMACall : Exception
{
    public NestedDMACall()
    {
    }

    public NestedDMACall(string? message) : base(message)
    {
    }

    public NestedDMACall(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NestedDMACall(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}