using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class IllegalSpritePalette : Exception
{
    public IllegalSpritePalette()
    {
    }

    public IllegalSpritePalette(string? message) : base(message)
    {
    }

    public IllegalSpritePalette(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected IllegalSpritePalette(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}