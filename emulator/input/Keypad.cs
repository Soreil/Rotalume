using System;

namespace emulator
{
    public class Keypad
    {
        //0x3 sets the top selection bits for buttons and dpad
        private byte keypadFlags = 0x30;

        public Keypad(InputDevices input) => Input = input;

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
                if (Input[JoypadKey.Right])
                {
                    joypad.SetBit(0, false);
                }

                if (Input[JoypadKey.Left])
                {
                    joypad.SetBit(1, false);
                }

                if (Input[JoypadKey.Up])
                {
                    joypad.SetBit(2, false);
                }

                if (Input[JoypadKey.Down])
                {
                    joypad.SetBit(3, false);
                }
            }
            if (selectButtons)
            {

                if (Input[JoypadKey.A])
                {
                    joypad.SetBit(0, false);
                }

                if (Input[JoypadKey.B])
                {
                    joypad.SetBit(1, false);
                }

                if (Input[JoypadKey.Select])
                {
                    joypad.SetBit(2, false);
                }

                if (Input[JoypadKey.Start])
                {
                    joypad.SetBit(3, false);
                }
            }

            return (byte)((joypad & 0xf) | 0xc0);
        }

        internal (Action<byte> Write, Func<byte> Read) HookUpKeypad() => (Write: x => keypadFlags = (byte)(x & 0xf0), Read: () => UpdateJoypadPresses());

        private bool Rumbling = false;

        public InputDevices Input { get; }

        internal void ToggleRumble(object? sender, EventArgs e)
        {
            if (!Rumbling)
                Input.Vibrate(1, 1);
            else Input.Vibrate(0, 0);
            Rumbling = !Rumbling;
        }
    }
}