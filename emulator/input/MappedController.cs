namespace emulator;

public class MappedController
{
    public readonly IGameController controller;

    public bool this[JoypadKey index] => index switch
    {
        JoypadKey.A => controller.IsAPressed,
        JoypadKey.B => controller.IsBPressed,
        JoypadKey.Select => controller.IsSelectPressed,
        JoypadKey.Start => controller.IsStartPressed,
        JoypadKey.Right => controller.IsDPadRightPressed,
        JoypadKey.Left => controller.IsDPadLeftPressed,
        JoypadKey.Up => controller.IsDPadUpPressed,
        JoypadKey.Down => controller.IsDPadDownPressed,
        _ => throw new Exception("Unmapped control")
    };

    public MappedController(IGameController gameController) => controller = gameController;

    public void Rumble(double leftMotor, double rightMotor) => controller.Vibrate(leftMotor, rightMotor);
}
