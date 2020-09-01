using System;

namespace generator
{
    public class Registers
    {
        private readonly WideRegisterData AF;
        private readonly RegisterData A;
        private readonly RegisterData F;
        private readonly WideRegisterData BC;
        private readonly RegisterData B;
        private readonly RegisterData C;
        private readonly WideRegisterData DE;
        private readonly RegisterData D;
        private readonly RegisterData E;
        private readonly WideRegisterData HL;
        private readonly RegisterData H;
        private readonly RegisterData L;

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
            //WideRegister.SP => SP.Read(),
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
                default: throw new NotImplementedException();
            }
        }
    }
}
