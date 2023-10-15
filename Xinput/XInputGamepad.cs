using System.Runtime.InteropServices;

namespace J2i.Net.XInputWrapper;

[StructLayout(LayoutKind.Explicit)]
internal struct XInputGamepad
{
    [MarshalAs(UnmanagedType.I2)]
    [FieldOffset(0)]
    internal short wButtons;

    [MarshalAs(UnmanagedType.I1)]
    [FieldOffset(2)]
    internal byte bLeftTrigger;

    [MarshalAs(UnmanagedType.I1)]
    [FieldOffset(3)]
    internal byte bRightTrigger;

    [MarshalAs(UnmanagedType.I2)]
    [FieldOffset(4)]
    internal short sThumbLX;

    [MarshalAs(UnmanagedType.I2)]
    [FieldOffset(6)]
    internal short sThumbLY;

    [MarshalAs(UnmanagedType.I2)]
    [FieldOffset(8)]
    internal short sThumbRX;

    [MarshalAs(UnmanagedType.I2)]
    [FieldOffset(10)]
    internal short sThumbRY;

    internal readonly bool IsButtonPressed(int buttonFlags) => (wButtons & buttonFlags) == buttonFlags;

    internal void Copy(XInputGamepad source)
    {
        sThumbLX = source.sThumbLX;
        sThumbLY = source.sThumbLY;
        sThumbRX = source.sThumbRX;
        sThumbRY = source.sThumbRY;
        bLeftTrigger = source.bLeftTrigger;
        bRightTrigger = source.bRightTrigger;
        wButtons = source.wButtons;
    }
}
