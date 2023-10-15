namespace emulator.graphics;

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

}