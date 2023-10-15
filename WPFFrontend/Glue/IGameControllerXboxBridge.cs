using emulator.input;

using J2i.Net.XInputWrapper;

namespace WPFFrontend.Glue;

internal class IGameControllerXboxBridge(XboxControllerWithInterruptHandler xboxcontroller) : IGameController
{
    public bool IsAPressed => xboxcontroller.X.IsBPressed;
    public bool IsSelectPressed => xboxcontroller.X.IsBackPressed;
    public bool IsBPressed => xboxcontroller.X.IsAPressed;
    public bool IsDPadDownPressed => xboxcontroller.X.IsDPadDownPressed;
    public bool IsDPadLeftPressed => xboxcontroller.X.IsDPadLeftPressed;
    public bool IsDPadRightPressed => xboxcontroller.X.IsDPadRightPressed;
    public bool IsDPadUpPressed => xboxcontroller.X.IsDPadUpPressed;
    public bool IsStartPressed => xboxcontroller.X.IsStartPressed;
    public void Vibrate(double leftMotor, double rightMotor) => xboxcontroller.X.Vibrate(leftMotor, rightMotor);

    public void AddEventHandler(EventHandler<EventArgs> e) => xboxcontroller.KeyWentDown += e;
    public void RemoveEventHandler(EventHandler<EventArgs> e) => xboxcontroller.KeyWentDown -= e;
}
