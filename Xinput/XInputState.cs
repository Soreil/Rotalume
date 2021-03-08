using System.Runtime.InteropServices;

namespace J2i.Net.XInputWrapper
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct XInputState
    {
        [FieldOffset(0)]
        public int PacketNumber;

        [FieldOffset(4)]
        internal XInputGamepad Gamepad;

        public void Copy(XInputState source)
        {
            PacketNumber = source.PacketNumber;
            Gamepad.Copy(source.Gamepad);
        }
    }
}
