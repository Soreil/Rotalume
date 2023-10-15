using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class IllegalInterruptState : Exception
{
    public IllegalInterruptState()
    {
    }

    public IllegalInterruptState(string? message) : base(message)
    {
    }

    public IllegalInterruptState(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}