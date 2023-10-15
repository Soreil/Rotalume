using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class ShadeDoesNotRepresentAColourException : Exception
{
    public ShadeDoesNotRepresentAColourException()
    {
    }

    public ShadeDoesNotRepresentAColourException(string? message) : base(message)
    {
    }

    public ShadeDoesNotRepresentAColourException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}