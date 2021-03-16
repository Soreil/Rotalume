using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace WPFFrontend
{
    public class KeyBoardWithInterruptHandler
    {
        public event EventHandler<EventArgs>? KeyWentDown;

        public bool A { get; internal set; }
        public bool Select { get; internal set; }
        public bool B { get; internal set; }
        public bool DpadDown { get; internal set; }
        public bool DpadLeft { get; internal set; }
        public bool DpadRight { get; internal set; }
        public bool DpadUp { get; internal set; }
        public bool Start { get; internal set; }

        private readonly Dictionary<Key, emulator.JoypadKey> _mappedKeys;
        public KeyBoardWithInterruptHandler(Dictionary<Key, emulator.JoypadKey> mappedKeys) => _mappedKeys = mappedKeys;

        public void Down(object? sender, KeyEventArgs e)
        {
            if (!_mappedKeys.ContainsKey(e.Key)) return;

            switch (_mappedKeys[e.Key])
            {
                case emulator.JoypadKey.A:
                A = true; break;
                case emulator.JoypadKey.B:
                B = true; break;
                case emulator.JoypadKey.Select:
                Select = true; break;
                case emulator.JoypadKey.Start:
                Start = true; break;
                case emulator.JoypadKey.Up:
                DpadUp = true; break;
                case emulator.JoypadKey.Down:
                DpadDown = true; break;
                case emulator.JoypadKey.Left:
                DpadLeft = true; break;
                case emulator.JoypadKey.Right:
                DpadRight = true; break;
            }

            OnAnyKeyDown(EventArgs.Empty);
        }

        public void Up(object? sender, KeyEventArgs e)
        {
            if (!_mappedKeys.ContainsKey(e.Key)) return;

            switch (_mappedKeys[e.Key])
            {
                case emulator.JoypadKey.A:
                A = false; break;
                case emulator.JoypadKey.B:
                B = false; break;
                case emulator.JoypadKey.Select:
                Select = false; break;
                case emulator.JoypadKey.Start:
                Start = false; break;
                case emulator.JoypadKey.Up:
                DpadUp = false; break;
                case emulator.JoypadKey.Down:
                DpadDown = false; break;
                case emulator.JoypadKey.Left:
                DpadLeft = false; break;
                case emulator.JoypadKey.Right:
                DpadRight = false; break;
            }
        }

        protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
    }
}