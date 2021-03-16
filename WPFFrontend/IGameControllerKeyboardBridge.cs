using emulator;

using System;
using System.Windows.Input;

namespace WPFFrontend
{
    internal class IGameControllerKeyboardBridge : IGameController
    {
        public bool IsAPressed => keyboard[Key.X];
        public bool IsSelectPressed => keyboard[Key.LeftShift] || keyboard[Key.RightShift];
        public bool IsBPressed => keyboard[Key.Z];
        public bool IsDPadDownPressed => keyboard[Key.Down];
        public bool IsDPadLeftPressed => keyboard[Key.Left];
        public bool IsDPadRightPressed => keyboard[Key.Right];
        public bool IsDPadUpPressed => keyboard[Key.Up];
        public bool IsStartPressed => keyboard[Key.Enter];
        public void Vibrate(double leftMotor, double rightMotor) { }

        private readonly KeyBoardWithInterruptHandler keyboard;

        public void AddEventHandler(EventHandler<EventArgs> e) => keyboard.KeyWentDown += e;
        public void RemoveEventHandler(EventHandler<EventArgs> e) => keyboard.KeyWentDown -= e;

        public IGameControllerKeyboardBridge(KeyBoardWithInterruptHandler keyboard) => this.keyboard = keyboard;
    }
}