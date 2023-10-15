using System.Runtime.InteropServices;

namespace J2i.Net.XInputWrapper;

[StructLayout(LayoutKind.Explicit)]
public struct XInputCapabilities
{
    [MarshalAs(UnmanagedType.I1)]
    [FieldOffset(0)]
    private readonly byte Type;

    [MarshalAs(UnmanagedType.I1)]
    [FieldOffset(1)]
    internal byte SubType;

    [MarshalAs(UnmanagedType.I2)]
    [FieldOffset(2)]
    internal short Flags;


    [FieldOffset(4)]
    internal XInputGamepad Gamepad;

    [FieldOffset(16)]
    internal XInputVibration Vibration;
}
