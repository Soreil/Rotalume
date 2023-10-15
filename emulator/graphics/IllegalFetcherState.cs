using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class IllegalFetcherState : Exception
{
    public IllegalFetcherState()
    {
    }

    public IllegalFetcherState(string? message) : base(message)
    {
    }

    public IllegalFetcherState(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}