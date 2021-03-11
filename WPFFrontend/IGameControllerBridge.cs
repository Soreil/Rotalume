using emulator;

using J2i.Net.XInputWrapper;

using System;

namespace WPFFrontend
{
    internal class IGameControllerBridge : IGameController
    {
        public bool IsAPressed => XboxController.IsAPressed;
        public bool IsBackPressed => XboxController.IsBackPressed;
        public bool IsBPressed => XboxController.IsBPressed;
        public bool IsConnected => XboxController.IsConnected;
        public bool IsDPadDownPressed => XboxController.IsDPadDownPressed;
        public bool IsDPadLeftPressed => XboxController.IsDPadLeftPressed;
        public bool IsDPadRightPressed => XboxController.IsDPadRightPressed;
        public bool IsDPadUpPressed => XboxController.IsDPadUpPressed;
        public bool IsLeftShoulderPressed => XboxController.IsLeftShoulderPressed;
        public bool IsLeftStickPressed => XboxController.IsLeftStickPressed;
        public bool IsRightShoulderPressed => XboxController.IsRightShoulderPressed;
        public bool IsRightStickPressed => XboxController.IsRightStickPressed;
        public bool IsStartPressed => XboxController.IsStartPressed;
        public bool IsXPressed => XboxController.IsXPressed;
        public bool IsYPressed => XboxController.IsYPressed;
        public (int X, int Y) LeftThumbStick => (XboxController.LeftThumbStick.X, XboxController.LeftThumbStick.Y);
        public int LeftTrigger => XboxController.LeftTrigger;
        public (int X, int Y) RightThumbStick => (XboxController.RightThumbStick.X, XboxController.RightThumbStick.Y);
        public int RightTrigger => XboxController.RightTrigger;

        public void UpdateState() => XboxController.UpdateState();
        public void Vibrate(double leftMotor, double rightMotor) => XboxController.Vibrate(leftMotor, rightMotor);
        public void Vibrate(double leftMotor, double rightMotor, TimeSpan length) => XboxController.Vibrate(leftMotor, rightMotor, length);

        private readonly XboxController XboxController;
        public IGameControllerBridge(XboxController xboxcontroller) => XboxController = xboxcontroller;
    }
}