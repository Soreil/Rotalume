using System.Runtime.InteropServices;

namespace emulator.registers;

[StructLayout(LayoutKind.Explicit)]
internal struct UnionRegister
{
    [FieldOffset(0)] public ushort Wide;
    [FieldOffset(0)] public byte Low;
    [FieldOffset(1)] public byte High;
}
