using emulator;

using J2i.Net.XInputWrapper;

namespace WPFFrontend.Glue;

internal class IGameControllerXboxBridge : IGameController
{
    public bool IsAPressed => XboxController.X.IsBPressed;
    public bool IsSelectPressed => XboxController.X.IsBackPressed;
    public bool IsBPressed => XboxController.X.IsAPressed;
    public bool IsDPadDownPressed => XboxController.X.IsDPadDownPressed;
    public bool IsDPadLeftPressed => XboxController.X.IsDPadLeftPressed;
    public bool IsDPadRightPressed => XboxController.X.IsDPadRightPressed;
    public bool IsDPadUpPressed => XboxController.X.IsDPadUpPressed;
    public bool IsStartPressed => XboxController.X.IsStartPressed;
    public void Vibrate(double leftMotor, double rightMotor) => XboxController.X.Vibrate(leftMotor, rightMotor);

    private readonly XboxControllerWithInterruptHandler XboxController;

    public void AddEventHandler(EventHandler<EventArgs> e) => XboxController.KeyWentDown += e;
    public void RemoveEventHandler(EventHandler<EventArgs> e) => XboxController.KeyWentDown -= e;

    public IGameControllerXboxBridge(XboxControllerWithInterruptHandler xboxcontroller) => XboxController = xboxcontroller;
}
