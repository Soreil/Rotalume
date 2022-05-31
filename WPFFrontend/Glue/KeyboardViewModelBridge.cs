using WPFFrontend.Platform;

namespace WPFFrontend.Glue;

public static class KeyboardViewModelBridge
{
    public static void Connect(Input input, Views.Screen view)
    {
        view.KeyUp += input.KeyUpHandler;
        view.KeyDown += input.KeyDownHandler;
    }
}
