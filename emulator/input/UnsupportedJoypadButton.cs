using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class UnsupportedJoypadButton : Exception
{
    public UnsupportedJoypadButton()
    {
    }

    public UnsupportedJoypadButton(string? message) : base(message)
    {
    }

    public UnsupportedJoypadButton(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UnsupportedJoypadButton(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}