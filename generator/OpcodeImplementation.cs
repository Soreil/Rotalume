using System;
namespace generator
{
    public partial class Decoder
    {
        public Action NOP()
        {
            return () => { };
        }
        public Action LD((WideRegister, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action LD((WideRegister, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action INC((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action INC((Register, bool) p0)
        {
            return () => { };
        }
        public Action DEC((Register, bool) p0)
        {
            return () => { };
        }
        public Action LD((Register, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action RLCA()
        {
            return () => { };
        }
        public Action LD((DMGInteger, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action ADD((WideRegister, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action LD((Register, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action DEC((WideRegister, bool) p0)
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
        public Action JR((DMGInteger, bool) p0)
        {
            return () => { };
        }
        public Action RRA()
        {
            return () => { };
        }
        public Action JR((Flag, bool) p0, (DMGInteger, bool) p1)
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
        public Action LD((Register, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action HALT()
        {
            return () => { };
        }
        public Action ADD((Register, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action ADD((Register, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action ADC((Register, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action ADC((Register, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action SUB((Register, bool) p0)
        {
            return () => { };
        }
        public Action SUB((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action SBC((Register, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action SBC((Register, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action AND((Register, bool) p0)
        {
            return () => { };
        }
        public Action AND((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action XOR((Register, bool) p0)
        {
            return () => { };
        }
        public Action XOR((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action OR((Register, bool) p0)
        {
            return () => { };
        }
        public Action OR((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action CP((Register, bool) p0)
        {
            return () => { };
        }
        public Action CP((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action RET((Flag, bool) p0)
        {
            return () => { };
        }
        public Action POP((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action JP((Flag, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action JP((DMGInteger, bool) p0)
        {
            return () => { };
        }
        public Action CALL((Flag, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action PUSH((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action ADD((Register, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action RST((byte, bool) p0)
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
        public Action CALL((DMGInteger, bool) p0)
        {
            return () => { };
        }
        public Action ADC((Register, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action ILLEGAL_D3()
        {
            return () => { };
        }
        public Action SUB((DMGInteger, bool) p0)
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
        public Action SBC((Register, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action LDH((DMGInteger, bool) p0, (Register, bool) p1)
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
        public Action AND((DMGInteger, bool) p0)
        {
            return () => { };
        }
        public Action ADD((WideRegister, bool) p0, (DMGInteger, bool) p1)
        {
            return () => { };
        }
        public Action JP((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action LD((DMGInteger, bool) p0, (Register, bool) p1)
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
        public Action XOR((DMGInteger, bool) p0)
        {
            return () => { };
        }
        public Action LDH((Register, bool) p0, (DMGInteger, bool) p1)
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
        public Action OR((DMGInteger, bool) p0)
        {
            return () => { };
        }
        public Action LD((WideRegister, bool) p0, (WideRegister, bool) p1)
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
        public Action CP((DMGInteger, bool) p0)
        {
            return () => { };
        }
        public Action RLC((Register, bool) p0)
        {
            return () => { };
        }
        public Action RLC((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action RRC((Register, bool) p0)
        {
            return () => { };
        }
        public Action RRC((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action RL((Register, bool) p0)
        {
            return () => { };
        }
        public Action RL((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action RR((Register, bool) p0)
        {
            return () => { };
        }
        public Action RR((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action SLA((Register, bool) p0)
        {
            return () => { };
        }
        public Action SLA((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action SRA((Register, bool) p0)
        {
            return () => { };
        }
        public Action SRA((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action SWAP((Register, bool) p0)
        {
            return () => { };
        }
        public Action SWAP((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action SRL((Register, bool) p0)
        {
            return () => { };
        }
        public Action SRL((WideRegister, bool) p0)
        {
            return () => { };
        }
        public Action BIT((byte, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action BIT((byte, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action RES((byte, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action RES((byte, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }
        public Action SET((byte, bool) p0, (Register, bool) p1)
        {
            return () => { };
        }
        public Action SET((byte, bool) p0, (WideRegister, bool) p1)
        {
            return () => { };
        }


    }
}