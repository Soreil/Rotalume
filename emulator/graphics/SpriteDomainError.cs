using System.Runtime.Serialization;

namespace emulator;

[Serializable]
internal class SpriteDomainError : Exception
{
    public SpriteDomainError()
    {
    }

    public SpriteDomainError(string? message) : base(message)
    {
    }

    public SpriteDomainError(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}