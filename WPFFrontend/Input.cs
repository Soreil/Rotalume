﻿using emulator;

using J2i.Net.XInputWrapper;

using System.Windows.Input;

namespace WPFFrontend;

public class Input
{
    private InputDevices MakeNewInput()
    {
        XboxController.UpdateFrequency = 5;
        XboxController.StartPolling();

        var Controller1 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(0));
        var Controller2 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(1));
        var Controller3 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(2));
        var Controller4 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(3));

        Dictionary<Key, JoypadKey> mappedKeys = new()
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

        var UnconnectedKeyboard = new KeyBoardWithInterruptHandler(mappedKeys);

        KeyDown += new KeyEventHandler(UnconnectedKeyboard.Down);
        KeyUp += new KeyEventHandler(UnconnectedKeyboard.Up);

        var kb = new IGameControllerKeyboardBridge(UnconnectedKeyboard);

        var Controllers = new List<IGameController> { new IGameControllerXboxBridge(Controller1), new IGameControllerXboxBridge(Controller2), new IGameControllerXboxBridge(Controller3), new IGameControllerXboxBridge(Controller4) };
        return new InputDevices(kb, Controllers);
    }

    public readonly InputDevices Devices;

    private event KeyEventHandler? KeyDown;
    private event KeyEventHandler? KeyUp;

    public void KeyDownHandler(object? o, KeyEventArgs e) => KeyDown?.Invoke(o, e);
    public void KeyUpHandler(object? o, KeyEventArgs e) => KeyUp?.Invoke(o, e);

    public Input()
    {
        Devices = MakeNewInput();
    }
}
