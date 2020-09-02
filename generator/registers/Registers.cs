using System;

namespace generator
{
    public record Registers
    {
        public WideRegisterData AF;
        public RegisterData A;
        public RegisterData F;
        public WideRegisterData BC;
        public RegisterData B;
        public RegisterData C;
        public WideRegisterData DE;
        public RegisterData D;
        public RegisterData E;
        public WideRegisterData HL;
        public RegisterData H;
        public RegisterData L;
        public WideRegisterData SP;

        public Registers()
        {
            AF = new WideRegisterData();
            A = AF.High;
            F = AF.Low;
            BC = new WideRegisterData();
            B = BC.High;
            C = BC.Low;
            DE = new WideRegisterData();
            D = DE.High;
            E = DE.Low;
            HL = new WideRegisterData();
            H = HL.High;
            L = HL.Low;

            SP = new WideRegisterData();
        }

        public bool Get(Flag f)
        {
            var FReg = F.Read();
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
            Register.A => A.Read(),
            Register.B => B.Read(),
            Register.C => C.Read(),
            Register.D => D.Read(),
            Register.E => E.Read(),
            Register.F => F.Read(),
            Register.H => H.Read(),
            Register.L => L.Read(),
            _ => throw new NotImplementedException(),
        };

        public ushort Get(WideRegister r) => r switch
        {
            WideRegister.AF => AF.Read(),
            WideRegister.BC => BC.Read(),
            WideRegister.DE => DE.Read(),
            WideRegister.HL => HL.Read(),
            //WideRegister.PC => PC.Read(),
            WideRegister.SP => SP.Read(),
            _ => throw new NotImplementedException(),
        };

        public void Mark(Flag f)
        {
            var FReg = F.Read();
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
            F.Write(FReg);
        }
        public void Set(Flag f, bool b)
        {
            var FReg = F.Read();
            switch (f)
            {
                case Flag.Z:
                    FReg = FReg.SetBit(7, b);
                    break;
                case Flag.N:
                    FReg = FReg.SetBit(6, b);
                    break;
                case Flag.H:
                    FReg = FReg.SetBit(5, b);
                    break;
                case Flag.C:
                    FReg = FReg.SetBit(4, b);
                    break;
                default:
                    throw new Exception("Flag argument can only be a flag name, not a state");
            }
            F.Write(FReg);
        }

        public void Set(Register r, byte v)
        {
            switch (r)
            {
                case Register.A: A.Write(v); break;
                case Register.B: B.Write(v); break;
                case Register.C: C.Write(v); break;
                case Register.D: D.Write(v); break;
                case Register.E: E.Write(v); break;
                case Register.F: F.Write(v); break;
                case Register.H: H.Write(v); break;
                case Register.L: L.Write(v); break;
                default: throw new NotImplementedException();
            }
        }
        public void Set(WideRegister r, ushort v)
        {
            switch (r)
            {
                case WideRegister.AF: AF.Write(v); break;
                case WideRegister.BC: BC.Write(v); break;
                case WideRegister.DE: DE.Write(v); break;
                case WideRegister.HL: HL.Write(v); break;
                case WideRegister.SP: SP.Write(v); break;
                default: throw new NotImplementedException();
            }
        }
    }
}
