using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class IllegalSampleRateException : Exception
{
    public IllegalSampleRateException()
    {
    }

    public IllegalSampleRateException(string? message) : base(message)
    {
    }

    public IllegalSampleRateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected IllegalSampleRateException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}