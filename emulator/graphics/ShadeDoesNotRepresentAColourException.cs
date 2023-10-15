namespace emulator.graphics;

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