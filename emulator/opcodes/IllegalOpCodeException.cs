﻿using System.Runtime.Serialization;

namespace emulator;

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

    protected IllegalOpCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}