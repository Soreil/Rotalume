using System;
using System.Collections.Concurrent;

namespace emulator
{
    public class Keypad
    {
        //0x3 sets the top selection bits for buttons and dpad
        private byte keypadFlags = 0x30;
        private readonly ConcurrentDictionary<JoypadKey, bool> Pressed;

        public Keypad(ConcurrentDictionary<JoypadKey, bool> pressed, IGameController? controller)
        {
            Pressed = pressed;
            Controller = controller;
        }

        private byte UpdateJoypadPresses()
        {
            var selectButtons = !keypadFlags.GetBit(5);
            var selectArrows = !keypadFlags.GetBit(4);

            byte joypad = 0xf;
            if (!selectButtons && !selectArrows)
            {
                return (byte)((joypad & 0xf) | 0xc0);
            }

            if (selectArrows)
            {
                if (Pressed[JoypadKey.Right])
                {
                    joypad.SetBit(0, false);
                }

                if (Pressed[JoypadKey.Left])
                {
                    joypad.SetBit(1, false);
                }

                if (Pressed[JoypadKey.Up])
                {
                    joypad.SetBit(2, false);
                }

                if (Pressed[JoypadKey.Down])
                {
                    joypad.SetBit(3, false);
                }
            }
            if (selectButtons)
            {

                if (Pressed[JoypadKey.B])
                {
                    joypad.SetBit(0, false);
                }

                if (Pressed[JoypadKey.A])
                {
                    joypad.SetBit(1, false);
                }

                if (Pressed[JoypadKey.Select])
                {
                    joypad.SetBit(2, false);
                }

                if (Pressed[JoypadKey.Start])
                {
                    joypad.SetBit(3, false);
                }
            }

            return (byte)((joypad & 0xf) | 0xc0);
        }

        internal (Action<byte> Write, Func<byte> Read) HookUpKeypad() => (Write: x => keypadFlags = (byte)(x & 0xf0), Read: () => UpdateJoypadPresses());

        private bool Rumbling = false;
        readonly IGameController? Controller;

        internal void ToggleRumble(object? sender, System.EventArgs e)
        {
            if (!Rumbling)
                Controller?.Vibrate(1, 1);
            else Controller?.Vibrate(0, 0);
            Rumbling = !Rumbling;
        }
    }
}