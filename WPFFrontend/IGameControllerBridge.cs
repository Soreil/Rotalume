using emulator;

using System;

namespace WPFFrontend
{
    internal class IGameControllerBridge : IGameController
    {
        public bool IsAPressed => XboxController.X.IsAPressed;
        public bool IsBackPressed => XboxController.X.IsBackPressed;
        public bool IsBPressed => XboxController.X.IsBPressed;
        public bool IsConnected => XboxController.X.IsConnected;
        public bool IsDPadDownPressed => XboxController.X.IsDPadDownPressed;
        public bool IsDPadLeftPressed => XboxController.X.IsDPadLeftPressed;
        public bool IsDPadRightPressed => XboxController.X.IsDPadRightPressed;
        public bool IsDPadUpPressed => XboxController.X.IsDPadUpPressed;
        public bool IsLeftShoulderPressed => XboxController.X.IsLeftShoulderPressed;
        public bool IsLeftStickPressed => XboxController.X.IsLeftStickPressed;
        public bool IsRightShoulderPressed => XboxController.X.IsRightShoulderPressed;
        public bool IsRightStickPressed => XboxController.X.IsRightStickPressed;
        public bool IsStartPressed => XboxController.X.IsStartPressed;
        public bool IsXPressed => XboxController.X.IsXPressed;
        public bool IsYPressed => XboxController.X.IsYPressed;
        public (int X, int Y) LeftThumbStick => (XboxController.X.LeftThumbStick.X, XboxController.X.LeftThumbStick.Y);
        public int LeftTrigger => XboxController.X.LeftTrigger;
        public (int X, int Y) RightThumbStick => (XboxController.X.RightThumbStick.X, XboxController.X.RightThumbStick.Y);
        public int RightTrigger => XboxController.X.RightTrigger;
        public void Vibrate(double leftMotor, double rightMotor) => XboxController.X.Vibrate(leftMotor, rightMotor);
        public void Vibrate(double leftMotor, double rightMotor, TimeSpan length) => XboxController.X.Vibrate(leftMotor, rightMotor, length);

        private readonly XboxControllerWithInterruptHandler XboxController;

        public void AddEventHandler(EventHandler<EventArgs> e) => XboxController.KeyWentDown += e;
        public void RemoveEventHandler(EventHandler<EventArgs> e) => XboxController.KeyWentDown -= e;

        public IGameControllerBridge(XboxControllerWithInterruptHandler xboxcontroller) => XboxController = xboxcontroller;
    }
}