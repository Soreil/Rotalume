﻿namespace emulator.memory;
[Serializable]
internal class NoBatteryPresentException : Exception
{
    public NoBatteryPresentException()
    {
    }

    public NoBatteryPresentException(string? message) : base(message)
    {
    }

    public NoBatteryPresentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}