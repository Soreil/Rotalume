namespace emulator.input;
[Serializable]
internal class UnsupportedJoypadButton : Exception
{
    public UnsupportedJoypadButton()
    {
    }

    public UnsupportedJoypadButton(string? message) : base(message)
    {
    }

    public UnsupportedJoypadButton(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}