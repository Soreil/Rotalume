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
                    joypad = joypad.SetBit(0, false);
                }

                if (Pressed[JoypadKey.Left])
                {
                    joypad = joypad.SetBit(1, false);
                }

                if (Pressed[JoypadKey.Up])
                {
                    joypad = joypad.SetBit(2, false);
                }

                if (Pressed[JoypadKey.Down])
                {
                    joypad = joypad.SetBit(3, false);
                }
            }
            if (selectButtons)
            {

                if (Pressed[JoypadKey.B])
                {
                    joypad = joypad.SetBit(0, false);
                }

                if (Pressed[JoypadKey.A])
                {
                    joypad = joypad.SetBit(1, false);
                }

                if (Pressed[JoypadKey.Select])
                {
                    joypad = joypad.SetBit(2, false);
                }

                if (Pressed[JoypadKey.Start])
                {
                    joypad = joypad.SetBit(3, false);
                }
            }

            return (byte)((joypad & 0xf) | 0xc0);
        }

        internal void HookUpKeypad(ControlRegister controlRegisters)
        {
            controlRegisters.Writer[0] = x => keypadFlags = (byte)(x & 0xf0);
            controlRegisters.Reader[0] = () => UpdateJoypadPresses();
        }

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