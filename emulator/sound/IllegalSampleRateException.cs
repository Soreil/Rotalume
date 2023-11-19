namespace emulator;

[Serializable]
internal class IllegalSampleRateException : Exception
{
    public IllegalSampleRateException()
    {
    }

    public IllegalSampleRateException(string? message) : base(message)
    {
    }

    public IllegalSampleRateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}