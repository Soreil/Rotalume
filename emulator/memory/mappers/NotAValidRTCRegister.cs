namespace emulator.memory.mappers;
[Serializable]
internal class NotAValidRTCRegister : Exception
{
    public NotAValidRTCRegister()
    {
    }

    public NotAValidRTCRegister(string? message) : base(message)
    {
    }

    public NotAValidRTCRegister(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}