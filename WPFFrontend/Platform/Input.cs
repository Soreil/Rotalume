
using J2i.Net.XInputWrapper;

using CommunityToolkit.Mvvm.ComponentModel;

using System.Windows.Input;

using WPFFrontend.Glue;
using emulator.input;

namespace WPFFrontend.Platform;

public class Input : ObservableObject
{
    public readonly InputDevices Devices;

    public int SelectedController
    {
        get => Devices.SelectedController;
        set
        {
            if (Devices.SelectedController != value)
            {
                Devices.SelectedController = value;
                OnPropertyChanged(nameof(SelectedController));
            }
        }
    }

    private event KeyEventHandler? KeyDown;
    private event KeyEventHandler? KeyUp;

    public void KeyDownHandler(object? o, KeyEventArgs e) => KeyDown?.Invoke(o, e);
    public void KeyUpHandler(object? o, KeyEventArgs e) => KeyUp?.Invoke(o, e);

    public Input()
    {
        XboxController.UpdateFrequency = 5;
        XboxController.PollerLoop();

        var controllers = new List<XboxControllerWithInterruptHandler>
            {
                new(XboxController.RetrieveController(0)),
                new(XboxController.RetrieveController(1)),
                new(XboxController.RetrieveController(2)),
                new(XboxController.RetrieveController(3))
            };

        var mappedKeys = new Dictionary<Key, JoypadKey>
            {
                { Key.X, JoypadKey.A },
                { Key.LeftShift, JoypadKey.Select },
                { Key.RightShift, JoypadKey.Select },
                { Key.Z, JoypadKey.B },
                { Key.Down, JoypadKey.Down },
                { Key.Left, JoypadKey.Left },
                { Key.Right, JoypadKey.Right },
                { Key.Up, JoypadKey.Up },
                { Key.Enter, JoypadKey.Start }
            };

        var unconnectedKeyboard = new KeyBoardWithInterruptHandler(mappedKeys);

        KeyDown += unconnectedKeyboard.Down;
        KeyUp += unconnectedKeyboard.Up;

        var kb = new IGameControllerKeyboardBridge(unconnectedKeyboard);

        Devices = new InputDevices(kb, controllers.ConvertAll(c => new IGameControllerXboxBridge(c)));
    }
}
