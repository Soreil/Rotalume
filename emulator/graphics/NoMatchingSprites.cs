using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class NoMatchingSprites : Exception
{
    public NoMatchingSprites()
    {
    }

    public NoMatchingSprites(string? message) : base(message)
    {
    }

    public NoMatchingSprites(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NoMatchingSprites(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}