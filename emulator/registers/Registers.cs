using System;
using System.Runtime.InteropServices;

namespace emulator
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UnionRegister
    {
        [FieldOffset(0)] public ushort Wide;
        [FieldOffset(0)] public byte Low;
        [FieldOffset(1)] public byte High;
    }
    public class Registers
    {
        private UnionRegister _AF;
        public ushort AF { get => _AF.Wide; set => _AF.Wide = (ushort)(value & 0xFFF0); }
        public byte A
        {
            get => _AF.High;
            set => _AF.High = value;
        }
        public byte F
        {
            get => _AF.Low;
            set => _AF.Low = value;
        }

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

        public bool Get(Flag f)
        {
            var FReg = F;
            return f switch
            {
                Flag.Z => FReg.GetBit(7),
                Flag.NZ => !FReg.GetBit(7),
                Flag.N => FReg.GetBit(6),
                Flag.NN => !FReg.GetBit(6),
                Flag.H => FReg.GetBit(5),
                Flag.NH => !FReg.GetBit(5),
                Flag.C => FReg.GetBit(4),
                Flag.NC => !FReg.GetBit(4),
                _ => throw new NotImplementedException(),
            };
        }

        public byte Get(Register r) => r switch
        {
            Register.A => A,
            Register.B => B,
            Register.C => C,
            Register.D => D,
            Register.E => E,
            Register.F => F,
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

        public void Mark(Flag f)
        {
            var FReg = F;
            switch (f)
            {
                case Flag.Z:
                FReg = FReg.SetBit(7);
                break;
                case Flag.NZ:
                FReg = FReg.ClearBit(7);
                break;
                case Flag.N:
                FReg = FReg.SetBit(6);
                break;
                case Flag.NN:
                FReg = FReg.ClearBit(6);
                break;
                case Flag.H:
                FReg = FReg.SetBit(5);
                break;
                case Flag.NH:
                FReg = FReg.ClearBit(5);
                break;
                case Flag.C:
                FReg = FReg.SetBit(4);
                break;
                case Flag.NC:
                FReg = FReg.ClearBit(4);
                break;
            }
            F = FReg;
        }
        public void Set(Flag f, bool b)
        {
            var FReg = F;
            FReg = f switch
            {
                Flag.Z => FReg.SetBit(7, b),
                Flag.N => FReg.SetBit(6, b),
                Flag.H => FReg.SetBit(5, b),
                Flag.C => FReg.SetBit(4, b),
                _ => throw new Exception("Flag argument can only be a flag name, not a state"),
            };
            F = FReg;
        }

        public void Set(Register r, byte v)
        {
            switch (r)
            {
                case Register.A: A = v; break;
                case Register.B: B = v; break;
                case Register.C: C = v; break;
                case Register.D: D = v; break;
                case Register.E: E = v; break;
                case Register.F: F = v; break;
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
}
