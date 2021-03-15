using System;
using System.Collections.Generic;

namespace emulator
{
    public enum JoypadKey
    {
        A,
        B,
        Select,
        Start,
        Right,
        Left,
        Up,
        Down
    }

    public class MappedController
    {
        public readonly IGameController controller;

        public bool this[JoypadKey index] => index switch
        {
            JoypadKey.A => controller.IsBPressed,
            JoypadKey.B => controller.IsAPressed,
            JoypadKey.Select => controller.IsBackPressed,
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
    public class InputDevices
    {
        private int _selectedController;

        //On SelectedController change we have to unmap the event handler of the previously selected gamepad
        //and register the event handler of the newly selected gamepad for gamepad interrupt handling.
        public int SelectedController
        {
            get => _selectedController;
            set
            {
                var previous = _selectedController;
                _selectedController = Math.Min(value, ControllerCount);

                //We don't have to remove the previous eventhandler in case it's 0
                if (previous != 0)
                {
                    mappedControllers[previous - 1].controller.RemoveEventHandler(OnUnderLyingChanged);
                }
                if (_selectedController != 0)
                    mappedControllers[_selectedController - 1].controller.AddEventHandler(OnUnderLyingChanged);
            }
        }

        //Event handlers
        private void OnUnderLyingChanged(object? sender, EventArgs e) => OnAnyKeyDown(e);
        protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
        public event EventHandler<EventArgs>? KeyWentDown;

        private readonly List<MappedController> mappedControllers;

        public int ControllerCount => mappedControllers.Count;

        private readonly Dictionary<JoypadKey, bool> Keyboard;

        public InputDevices(Dictionary<JoypadKey, bool> keyboard, List<IGameController> gameControllers)
        {
            Keyboard = keyboard;
            mappedControllers = new();
            foreach (var k in gameControllers) mappedControllers.Add(new MappedController(k));
        }

        public void Vibrate(double leftMotor, double rightMotor)
        {
            if (SelectedController != 0) mappedControllers[SelectedController - 1].Rumble(leftMotor, rightMotor);
        }

        public bool this[JoypadKey index] => SelectedController != 0 ? mappedControllers[SelectedController - 1][index] || Keyboard[index] : Keyboard[index];

    }
}
