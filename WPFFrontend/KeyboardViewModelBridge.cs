namespace WPFFrontend;

public static class KeyboardViewModelBridge
{
    public static void Connect(Input input, Screen view)
    {
        view.KeyUp += input.KeyUpHandler;
        view.KeyDown += input.KeyDownHandler;
    }
}
