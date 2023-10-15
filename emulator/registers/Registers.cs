using System.Runtime.InteropServices;

namespace emulator.registers;

[StructLayout(LayoutKind.Explicit)]
public struct UnionRegister
{
    [FieldOffset(0)] public ushort Wide;
    [FieldOffset(0)] public byte Low;
    [FieldOffset(1)] public byte High;
}
public class Registers
{
    public ushort AF
    {
        get => (ushort)((A << 8) | MakeF());
        set
        {
            A = (byte)(value >> 8);
            SetF((byte)(value & 0xf0));
        }
    }

    private void SetF(byte value)
    {
        Zero = value.GetBit(7);
        Negative = value.GetBit(6);
        Half = value.GetBit(5);
        Carry = value.GetBit(4);
    }
    private byte MakeF() => (byte)(
        (Convert.ToByte(Zero) << 7) |
        (Convert.ToByte(Negative) << 6) |
        (Convert.ToByte(Half) << 5) |
        (Convert.ToByte(Carry) << 4));

    public byte A;

    private UnionRegister _BC;
    public ushort BC { get => _BC.Wide; set => _BC.Wide = value; }
    public byte B
    {
        get => _BC.High;
        set => _BC.High = value;
    }
    public byte C
    {
        get => _BC.Low;
        set => _BC.Low = value;
    }

    private UnionRegister _DE;
    public ushort DE { get => _DE.Wide; set => _DE.Wide = value; }
    public byte D
    {
        get => _DE.High;
        set => _DE.High = value;
    }
    public byte E
    {
        get => _DE.Low;
        set => _DE.Low = value;
    }

    private UnionRegister _HL;
    public ushort HL { get => _HL.Wide; set => _HL.Wide = value; }
    public byte H
    {
        get => _HL.High;
        set => _HL.High = value;
    }
    public byte L
    {
        get => _HL.Low;
        set => _HL.Low = value;
    }

    public ushort SP;

    public bool Zero;
    public bool Negative;
    public bool Half;
    public bool Carry;

    public byte Get(Register r) => r switch
    {
        Register.A => A,
        Register.B => B,
        Register.C => C,
        Register.D => D,
        Register.E => E,
        Register.H => H,
        Register.L => L,
        _ => throw new NotImplementedException(),
    };

    public ushort Get(WideRegister r) => r switch
    {
        WideRegister.AF => AF,
        WideRegister.BC => BC,
        WideRegister.DE => DE,
        WideRegister.HL => HL,
        WideRegister.SP => SP,
        _ => throw new NotImplementedException(),
    };

    public void Set(Register r, byte v)
    {
        switch (r)
        {
            case Register.A: A = v; break;
            case Register.B: B = v; break;
            case Register.C: C = v; break;
            case Register.D: D = v; break;
            case Register.E: E = v; break;
            case Register.H: H = v; break;
            case Register.L: L = v; break;
            default: throw new NotImplementedException();
        }
    }
    public void Set(WideRegister r, ushort v)
    {
        switch (r)
        {
            case WideRegister.AF: AF = v; break;
            case WideRegister.BC: BC = v; break;
            case WideRegister.DE: DE = v; break;
            case WideRegister.HL: HL = v; break;
            case WideRegister.SP: SP = v; break;
            default: throw new NotImplementedException();
        }
    }
}
