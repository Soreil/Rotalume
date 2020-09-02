using System;
namespace generator
{
    public partial class Decoder
    {
        readonly public Registers Registers;
        readonly public Storage Storage;
        public Action NOP()
        {
            return () => { };
        }
        public Action LD((WideRegister, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                var arg = Storage.Fetch(p1.Item1);
                if (!p0.Item2.Immediate)
                {
                    var HL = Registers.Get(p0.Item1);
                    Storage.Write(HL, (byte)arg);
                }
                else
                {
                    Registers.Set(p0.Item1, (ushort)arg);
                }
            };
        }
        public Action LD((WideRegister, Traits) p0, (Register, Traits) p1)
        {
            return () =>
            {
                var address = Registers.Get(p0.Item1);
                var value = Registers.Get(p1.Item1);

                Storage.Write(address, value);

                switch (p0.Item2.Postfix)
                {
                    default:
                        break;
                    case Postfix.increment:
                        Registers.Set(p0.Item1, (ushort)(address + 1));
                        break;
                    case Postfix.decrement:
                        Registers.Set(p0.Item1, (ushort)(address - 1));
                        break;
                }
            };
        }
        public Action INC((WideRegister, Traits) p0)
        {
            return () =>
            {
                //Wide registers do not use flags for INC and DEC
                if (p0.Item2.Immediate)
                    Registers.Set(p0.Item1, (ushort)(Registers.Get(p0.Item1) + 1));
                else
                {
                    var addr = Registers.Get(p0.Item1);
                    var before = Storage.Read(addr);
                    var arg = (byte)(before + 1);
                    Storage.Write(addr, arg);

                    Registers.Set(Flag.Z, arg == 0);
                    Registers.Mark(Flag.NN);
                    Registers.Set(Flag.H, before.IsHalfCarryAdd(arg));
                }
            };
        }
        public Action INC((Register, Traits) p0)
        {
            return () => { Registers.Set(p0.Item1, (byte)(Registers.Get(p0.Item1) + 1)); };
        }
        public Action DEC((Register, Traits) p0)
        {
            return () => { Registers.Set(p0.Item1, (byte)(Registers.Get(p0.Item1) - 1)); };
        }
        public Action LD((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                var arg = Storage.Fetch(p1.Item1);

                Registers.Set(p0.Item1, (byte)arg);

            };
        }
        public Action RLCA()
        {
            return () => { };
        }
        public Action LD((DMGInteger, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action ADD((WideRegister, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action LD((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action DEC((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RRCA()
        {
            return () => { };
        }
        public Action STOP()
        {
            return () => { };
        }
        public Action RLA()
        {
            return () => { };
        }
        public Action JR((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action RRA()
        {
            return () => { };
        }
        public Action JR((Flag, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action DAA()
        {
            return () => { };
        }
        public Action CPL()
        {
            return () => { };
        }
        public Action SCF()
        {
            return () => { };
        }
        public Action CCF()
        {
            return () => { };
        }
        public Action LD((Register, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action HALT()
        {
            return () => { };
        }
        public Action ADD((Register, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action ADD((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action ADC((Register, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action ADC((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action SUB((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SUB((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SBC((Register, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action SBC((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action AND((Register, Traits) p0)
        {
            return () => { };
        }
        public Action AND((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action XOR((Register, Traits) p0)
        {
            return () => { };
        }
        public Action XOR((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action OR((Register, Traits) p0)
        {
            return () => { };
        }
        public Action OR((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action CP((Register, Traits) p0)
        {
            return () => { };
        }
        public Action CP((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RET((Flag, Traits) p0)
        {
            return () => { };
        }
        public Action POP((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action JP((Flag, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action JP((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action CALL((Flag, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action PUSH((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action ADD((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action RST((byte, Traits) p0)
        {
            return () => { };
        }
        public Action RET()
        {
            return () => { };
        }
        public Action PREFIX()
        {
            return () => { };
        }
        public Action CALL((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action ADC((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action ILLEGAL_D3()
        {
            return () => { };
        }
        public Action SUB((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action RETI()
        {
            return () => { };
        }
        public Action ILLEGAL_DB()
        {
            return () => { };
        }
        public Action ILLEGAL_DD()
        {
            return () => { };
        }
        public Action SBC((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action LDH((DMGInteger, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action ILLEGAL_E3()
        {
            return () => { };
        }
        public Action ILLEGAL_E4()
        {
            return () => { };
        }
        public Action AND((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action ADD((WideRegister, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action JP((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action LD((DMGInteger, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action ILLEGAL_EB()
        {
            return () => { };
        }
        public Action ILLEGAL_EC()
        {
            return () => { };
        }
        public Action ILLEGAL_ED()
        {
            return () => { };
        }
        public Action XOR((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action LDH((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action DI()
        {
            return () => { };
        }
        public Action ILLEGAL_F4()
        {
            return () => { };
        }
        public Action OR((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action LD((WideRegister, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action EI()
        {
            return () => { };
        }
        public Action ILLEGAL_FC()
        {
            return () => { };
        }
        public Action ILLEGAL_FD()
        {
            return () => { };
        }
        public Action CP((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action RLC((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RLC((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RRC((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RRC((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RL((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RL((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RR((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RR((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SLA((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SLA((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SRA((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SRA((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SWAP((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SWAP((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SRL((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SRL((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action BIT((byte, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action BIT((byte, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action RES((byte, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action RES((byte, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action SET((byte, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action SET((byte, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }


    }
}