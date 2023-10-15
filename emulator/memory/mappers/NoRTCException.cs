using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class NoRTCException : Exception
{
    public NoRTCException()
    {
    }

    public NoRTCException(string? message) : base(message)
    {
    }

    public NoRTCException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}