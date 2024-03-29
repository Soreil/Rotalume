﻿namespace emulator.memory;
[Serializable]
internal class UnexpectedSize : Exception
{
    public UnexpectedSize()
    {
    }

    public UnexpectedSize(string? message) : base(message)
    {
    }

    public UnexpectedSize(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}