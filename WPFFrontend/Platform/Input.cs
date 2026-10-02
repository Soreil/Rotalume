
using J2i.Net.XInputWrapper;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Input;

using Windows.System;

using WPFFrontend.Glue;

using emulator.input;

namespace WPFFrontend.Platform;

public class Input : ObservableObject
{
    public readonly InputDevices Devices;


    public int SelectedController
    {
        get => Devices.SelectedController;
        set => SetProperty(
            Devices.SelectedController,
            value,
            Devices,
            static (devices, selected) => devices.SelectedController = selected
            );
    }

    private event KeyEventHandler? KeyDown;
    private event KeyEventHandler? KeyUp;

    public void KeyDownHandler(object o, KeyRoutedEventArgs e) => KeyDown?.Invoke(o, e);
    public void KeyUpHandler(object o, KeyRoutedEventArgs e) => KeyUp?.Invoke(o, e);

    private readonly KeyBoardWithInterruptHandler keyboard;
    public void ReleaseKeys() => keyboard.ReleaseKeys();

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

        var mappedKeys = new Dictionary<VirtualKey, JoypadKey>
            {
                { VirtualKey.X, JoypadKey.A },
                { VirtualKey.Shift, JoypadKey.Select },
                { VirtualKey.LeftShift, JoypadKey.Select },
                { VirtualKey.RightShift, JoypadKey.Select },
                { VirtualKey.Z, JoypadKey.B },
                { VirtualKey.Down, JoypadKey.Down },
                { VirtualKey.Left, JoypadKey.Left },
                { VirtualKey.Right, JoypadKey.Right },
                { VirtualKey.Up, JoypadKey.Up },
                { VirtualKey.Enter, JoypadKey.Start }
            };

        keyboard = new KeyBoardWithInterruptHandler(mappedKeys);

        KeyDown += keyboard.Down;
        KeyUp += keyboard.Up;

        var kb = new IGameControllerKeyboardBridge(keyboard);

        Devices = new InputDevices(kb, controllers.ConvertAll(c => new IGameControllerXboxBridge(c)));
    }
}
