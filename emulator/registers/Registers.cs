using System;

namespace emulator
{
    public class Registers
    {
        private static byte Low(ushort s) => (byte)(s & 0x00ff);
        private static byte High(ushort s) => (byte)((s & 0xff00) >> 8);
        private static ushort SetLow(ushort s, byte b) => (ushort)((s & 0xff00) | b);
        private static ushort SetHigh(ushort s, byte b) => (ushort)((s & 0x00ff) | (b << 8));

        private ushort _AF;
        public ushort AF { get => _AF; set => _AF = (ushort)(value & 0xFFF0); }
        public byte A
        {
            get => High(AF);
            set => AF = SetHigh(AF, value);
        }
        public byte F
        {
            get => Low(AF);
            set => AF = SetLow(AF, value);
        }
        public ushort BC;
        public byte B
        {
            get => High(BC);
            set => BC = SetHigh(BC, value);
        }
        public byte C
        {
            get => Low(BC);
            set => BC = SetLow(BC, value);
        }
        public ushort DE;
        public byte D
        {
            get => High(DE);
            set => DE = SetHigh(DE, value);
        }
        public byte E
        {
            get => Low(DE);
            set => DE = SetLow(DE, value);
        }
        public ushort HL;
        public byte H
        {
            get => High(HL);
            set => HL = SetHigh(HL, value);
        }
        public byte L
        {
            get => Low(HL);
            set => HL = SetLow(HL, value);
        }
        public ushort SP;

        public Registers() { }

        public Registers(ushort aF, ushort bC, ushort dE, ushort hL, ushort sP)
        {
            AF = aF;
            BC = bC;
            DE = dE;
            HL = hL;
            SP = sP;
        }

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
