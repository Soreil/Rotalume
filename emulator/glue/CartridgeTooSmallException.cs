namespace emulator.glue;

[Serializable]
internal class CartridgeTooSmallException : Exception
{
    public CartridgeTooSmallException() : base("The cartridge is too small.")
    {
    }

    public CartridgeTooSmallException(string? message) : base(message)
    {
    }

    public CartridgeTooSmallException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
