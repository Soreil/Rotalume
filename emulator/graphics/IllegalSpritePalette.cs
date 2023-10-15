namespace emulator.graphics;

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

}