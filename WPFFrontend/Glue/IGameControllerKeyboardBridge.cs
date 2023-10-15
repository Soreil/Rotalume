using emulator.input;

namespace WPFFrontend.Glue;

internal class IGameControllerKeyboardBridge(KeyBoardWithInterruptHandler keyboard) : IGameController
{
    public bool IsAPressed => keyboard.A;
    public bool IsSelectPressed => keyboard.Select;
    public bool IsBPressed => keyboard.B;
    public bool IsDPadDownPressed => keyboard.DpadDown;
    public bool IsDPadLeftPressed => keyboard.DpadLeft;
    public bool IsDPadRightPressed => keyboard.DpadRight;
    public bool IsDPadUpPressed => keyboard.DpadUp;
    public bool IsStartPressed => keyboard.Start;
    public void Vibrate(double leftMotor, double rightMotor) { }

    public void AddEventHandler(EventHandler<EventArgs> e) => keyboard.KeyWentDown += e;
    public void RemoveEventHandler(EventHandler<EventArgs> e) => keyboard.KeyWentDown -= e;
}
