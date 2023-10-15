namespace emulator.opcodes;
[Serializable]
internal class IllegalOpCodeException : Exception
{
    public IllegalOpCodeException()
    {
    }

    public IllegalOpCodeException(string? message) : base(message)
    {
    }

    public IllegalOpCodeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}