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
