using System;
using System.Collections.Generic;

namespace generator
{
    public class Decoder
    {
        public Dictionary<Unprefixed, Action> MakeTableUnprefixed()
        {
            Dictionary<Unprefixed, Action> m = new Dictionary<Unprefixed, Action>();
            m[Unprefixed.NOP] = NOP();
            m[Unprefixed.LD_BC_d16] = LD((WideRegister.BC, true), (DMGInteger.d16, true));
            m[Unprefixed.LD_AT_BC_A] = LD((WideRegister.BC, false), (Register.A, true));
            m[Unprefixed.INC_BC] = INC((WideRegister.BC, true));
            m[Unprefixed.INC_B] = INC((Register.B, true));
            m[Unprefixed.DEC_B] = DEC((Register.B, true));
            m[Unprefixed.LD_B_d8] = LD((Register.B, true), (DMGInteger.d8, true));
            m[Unprefixed.RLCA] = RLCA();
            m[Unprefixed.LD_AT_a16_SP] = LD((DMGInteger.a16, false), (WideRegister.SP, true));
            m[Unprefixed.ADD_HL_BC] = ADD((WideRegister.HL, true), (WideRegister.BC, true));
            m[Unprefixed.LD_A_AT_BC] = LD((Register.A, true), (WideRegister.BC, false));
            m[Unprefixed.DEC_BC] = DEC((WideRegister.BC, true));
            m[Unprefixed.INC_C] = INC((Register.C, true));
            m[Unprefixed.DEC_C] = DEC((Register.C, true));
            m[Unprefixed.LD_C_d8] = LD((Register.C, true), (DMGInteger.d8, true));
            m[Unprefixed.RRCA] = RRCA();
            m[Unprefixed.STOP] = STOP();
            m[Unprefixed.LD_DE_d16] = LD((WideRegister.DE, true), (DMGInteger.d16, true));
            m[Unprefixed.LD_AT_DE_A] = LD((WideRegister.DE, false), (Register.A, true));
            m[Unprefixed.INC_DE] = INC((WideRegister.DE, true));
            m[Unprefixed.INC_D] = INC((Register.D, true));
            m[Unprefixed.DEC_D] = DEC((Register.D, true));
            m[Unprefixed.LD_D_d8] = LD((Register.D, true), (DMGInteger.d8, true));
            m[Unprefixed.RLA] = RLA();
            m[Unprefixed.JR_r8] = JR((DMGInteger.r8, true));
            m[Unprefixed.ADD_HL_DE] = ADD((WideRegister.HL, true), (WideRegister.DE, true));
            m[Unprefixed.LD_A_AT_DE] = LD((Register.A, true), (WideRegister.DE, false));
            m[Unprefixed.DEC_DE] = DEC((WideRegister.DE, true));
            m[Unprefixed.INC_E] = INC((Register.E, true));
            m[Unprefixed.DEC_E] = DEC((Register.E, true));
            m[Unprefixed.LD_E_d8] = LD((Register.E, true), (DMGInteger.d8, true));
            m[Unprefixed.RRA] = RRA();
            m[Unprefixed.JR_NZ_r8] = JR((Flag.NZ, true), (DMGInteger.r8, true));
            m[Unprefixed.LD_HL_d16] = LD((WideRegister.HL, true), (DMGInteger.d16, true));
            m[Unprefixed.LDI_AT_HL_A] = LD((WideRegister.HL, false), (Register.A, true));
            m[Unprefixed.INC_HL] = INC((WideRegister.HL, true));
            m[Unprefixed.INC_H] = INC((Register.H, true));
            m[Unprefixed.DEC_H] = DEC((Register.H, true));
            m[Unprefixed.LD_H_d8] = LD((Register.H, true), (DMGInteger.d8, true));
            m[Unprefixed.DAA] = DAA();
            m[Unprefixed.JR_Zero_r8] = JR((Flag.Z, true), (DMGInteger.r8, true));
            m[Unprefixed.ADD_HL_HL] = ADD((WideRegister.HL, true), (WideRegister.HL, true));
            m[Unprefixed.LD_AI_AT_HL] = LD((Register.A, true), (WideRegister.HL, false));
            m[Unprefixed.DEC_HL] = DEC((WideRegister.HL, true));
            m[Unprefixed.INC_L] = INC((Register.L, true));
            m[Unprefixed.DEC_L] = DEC((Register.L, true));
            m[Unprefixed.LD_L_d8] = LD((Register.L, true), (DMGInteger.d8, true));
            m[Unprefixed.CPL] = CPL();
            m[Unprefixed.JR_NC_r8] = JR((Flag.NC, true), (DMGInteger.r8, true));
            m[Unprefixed.LD_SP_d16] = LD((WideRegister.SP, true), (DMGInteger.d16, true));
            m[Unprefixed.LDD_AT_HL_A] = LD((WideRegister.HL, false), (Register.A, true));
            m[Unprefixed.INC_SP] = INC((WideRegister.SP, true));
            m[Unprefixed.INC_AT_HL] = INC((WideRegister.HL, false));
            m[Unprefixed.DEC_AT_HL] = DEC((WideRegister.HL, false));
            m[Unprefixed.LD_AT_HL_d8] = LD((WideRegister.HL, false), (DMGInteger.d8, true));
            m[Unprefixed.SCF] = SCF();
            m[Unprefixed.JR_Carry_r8] = JR((Flag.C, true), (DMGInteger.r8, true));
            m[Unprefixed.ADD_HL_SP] = ADD((WideRegister.HL, true), (WideRegister.SP, true));
            m[Unprefixed.LD_AD_AT_HL] = LD((Register.A, true), (WideRegister.HL, false));
            m[Unprefixed.DEC_SP] = DEC((WideRegister.SP, true));
            m[Unprefixed.INC_A] = INC((Register.A, true));
            m[Unprefixed.DEC_A] = DEC((Register.A, true));
            m[Unprefixed.LD_A_d8] = LD((Register.A, true), (DMGInteger.d8, true));
            m[Unprefixed.CCF] = CCF();
            m[Unprefixed.LD_B_B] = LD((Register.B, true), (Register.B, true));
            m[Unprefixed.LD_B_C] = LD((Register.B, true), (Register.C, true));
            m[Unprefixed.LD_B_D] = LD((Register.B, true), (Register.D, true));
            m[Unprefixed.LD_B_E] = LD((Register.B, true), (Register.E, true));
            m[Unprefixed.LD_B_H] = LD((Register.B, true), (Register.H, true));
            m[Unprefixed.LD_B_L] = LD((Register.B, true), (Register.L, true));
            m[Unprefixed.LD_B_AT_HL] = LD((Register.B, true), (WideRegister.HL, false));
            m[Unprefixed.LD_B_A] = LD((Register.B, true), (Register.A, true));
            m[Unprefixed.LD_C_B] = LD((Register.C, true), (Register.B, true));
            m[Unprefixed.LD_C_C] = LD((Register.C, true), (Register.C, true));
            m[Unprefixed.LD_C_D] = LD((Register.C, true), (Register.D, true));
            m[Unprefixed.LD_C_E] = LD((Register.C, true), (Register.E, true));
            m[Unprefixed.LD_C_H] = LD((Register.C, true), (Register.H, true));
            m[Unprefixed.LD_C_L] = LD((Register.C, true), (Register.L, true));
            m[Unprefixed.LD_C_AT_HL] = LD((Register.C, true), (WideRegister.HL, false));
            m[Unprefixed.LD_C_A] = LD((Register.C, true), (Register.A, true));
            m[Unprefixed.LD_D_B] = LD((Register.D, true), (Register.B, true));
            m[Unprefixed.LD_D_C] = LD((Register.D, true), (Register.C, true));
            m[Unprefixed.LD_D_D] = LD((Register.D, true), (Register.D, true));
            m[Unprefixed.LD_D_E] = LD((Register.D, true), (Register.E, true));
            m[Unprefixed.LD_D_H] = LD((Register.D, true), (Register.H, true));
            m[Unprefixed.LD_D_L] = LD((Register.D, true), (Register.L, true));
            m[Unprefixed.LD_D_AT_HL] = LD((Register.D, true), (WideRegister.HL, false));
            m[Unprefixed.LD_D_A] = LD((Register.D, true), (Register.A, true));
            m[Unprefixed.LD_E_B] = LD((Register.E, true), (Register.B, true));
            m[Unprefixed.LD_E_C] = LD((Register.E, true), (Register.C, true));
            m[Unprefixed.LD_E_D] = LD((Register.E, true), (Register.D, true));
            m[Unprefixed.LD_E_E] = LD((Register.E, true), (Register.E, true));
            m[Unprefixed.LD_E_H] = LD((Register.E, true), (Register.H, true));
            m[Unprefixed.LD_E_L] = LD((Register.E, true), (Register.L, true));
            m[Unprefixed.LD_E_AT_HL] = LD((Register.E, true), (WideRegister.HL, false));
            m[Unprefixed.LD_E_A] = LD((Register.E, true), (Register.A, true));
            m[Unprefixed.LD_H_B] = LD((Register.H, true), (Register.B, true));
            m[Unprefixed.LD_H_C] = LD((Register.H, true), (Register.C, true));
            m[Unprefixed.LD_H_D] = LD((Register.H, true), (Register.D, true));
            m[Unprefixed.LD_H_E] = LD((Register.H, true), (Register.E, true));
            m[Unprefixed.LD_H_H] = LD((Register.H, true), (Register.H, true));
            m[Unprefixed.LD_H_L] = LD((Register.H, true), (Register.L, true));
            m[Unprefixed.LD_H_AT_HL] = LD((Register.H, true), (WideRegister.HL, false));
            m[Unprefixed.LD_H_A] = LD((Register.H, true), (Register.A, true));
            m[Unprefixed.LD_L_B] = LD((Register.L, true), (Register.B, true));
            m[Unprefixed.LD_L_C] = LD((Register.L, true), (Register.C, true));
            m[Unprefixed.LD_L_D] = LD((Register.L, true), (Register.D, true));
            m[Unprefixed.LD_L_E] = LD((Register.L, true), (Register.E, true));
            m[Unprefixed.LD_L_H] = LD((Register.L, true), (Register.H, true));
            m[Unprefixed.LD_L_L] = LD((Register.L, true), (Register.L, true));
            m[Unprefixed.LD_L_AT_HL] = LD((Register.L, true), (WideRegister.HL, false));
            m[Unprefixed.LD_L_A] = LD((Register.L, true), (Register.A, true));
            m[Unprefixed.LD_AT_HL_B] = LD((WideRegister.HL, false), (Register.B, true));
            m[Unprefixed.LD_AT_HL_C] = LD((WideRegister.HL, false), (Register.C, true));
            m[Unprefixed.LD_AT_HL_D] = LD((WideRegister.HL, false), (Register.D, true));
            m[Unprefixed.LD_AT_HL_E] = LD((WideRegister.HL, false), (Register.E, true));
            m[Unprefixed.LD_AT_HL_H] = LD((WideRegister.HL, false), (Register.H, true));
            m[Unprefixed.LD_AT_HL_L] = LD((WideRegister.HL, false), (Register.L, true));
            m[Unprefixed.HALT] = HALT();
            m[Unprefixed.LD_AT_HL_A] = LD((WideRegister.HL, false), (Register.A, true));
            m[Unprefixed.LD_A_B] = LD((Register.A, true), (Register.B, true));
            m[Unprefixed.LD_A_C] = LD((Register.A, true), (Register.C, true));
            m[Unprefixed.LD_A_D] = LD((Register.A, true), (Register.D, true));
            m[Unprefixed.LD_A_E] = LD((Register.A, true), (Register.E, true));
            m[Unprefixed.LD_A_H] = LD((Register.A, true), (Register.H, true));
            m[Unprefixed.LD_A_L] = LD((Register.A, true), (Register.L, true));
            m[Unprefixed.LD_A_AT_HL] = LD((Register.A, true), (WideRegister.HL, false));
            m[Unprefixed.LD_A_A] = LD((Register.A, true), (Register.A, true));
            m[Unprefixed.ADD_A_B] = ADD((Register.A, true), (Register.B, true));
            m[Unprefixed.ADD_A_C] = ADD((Register.A, true), (Register.C, true));
            m[Unprefixed.ADD_A_D] = ADD((Register.A, true), (Register.D, true));
            m[Unprefixed.ADD_A_E] = ADD((Register.A, true), (Register.E, true));
            m[Unprefixed.ADD_A_H] = ADD((Register.A, true), (Register.H, true));
            m[Unprefixed.ADD_A_L] = ADD((Register.A, true), (Register.L, true));
            m[Unprefixed.ADD_A_AT_HL] = ADD((Register.A, true), (WideRegister.HL, false));
            m[Unprefixed.ADD_A_A] = ADD((Register.A, true), (Register.A, true));
            m[Unprefixed.ADC_A_B] = ADC((Register.A, true), (Register.B, true));
            m[Unprefixed.ADC_A_C] = ADC((Register.A, true), (Register.C, true));
            m[Unprefixed.ADC_A_D] = ADC((Register.A, true), (Register.D, true));
            m[Unprefixed.ADC_A_E] = ADC((Register.A, true), (Register.E, true));
            m[Unprefixed.ADC_A_H] = ADC((Register.A, true), (Register.H, true));
            m[Unprefixed.ADC_A_L] = ADC((Register.A, true), (Register.L, true));
            m[Unprefixed.ADC_A_AT_HL] = ADC((Register.A, true), (WideRegister.HL, false));
            m[Unprefixed.ADC_A_A] = ADC((Register.A, true), (Register.A, true));
            m[Unprefixed.SUB_B] = SUB((Register.B, true));
            m[Unprefixed.SUB_C] = SUB((Register.C, true));
            m[Unprefixed.SUB_D] = SUB((Register.D, true));
            m[Unprefixed.SUB_E] = SUB((Register.E, true));
            m[Unprefixed.SUB_H] = SUB((Register.H, true));
            m[Unprefixed.SUB_L] = SUB((Register.L, true));
            m[Unprefixed.SUB_AT_HL] = SUB((WideRegister.HL, false));
            m[Unprefixed.SUB_A] = SUB((Register.A, true));
            m[Unprefixed.SBC_A_B] = SBC((Register.A, true), (Register.B, true));
            m[Unprefixed.SBC_A_C] = SBC((Register.A, true), (Register.C, true));
            m[Unprefixed.SBC_A_D] = SBC((Register.A, true), (Register.D, true));
            m[Unprefixed.SBC_A_E] = SBC((Register.A, true), (Register.E, true));
            m[Unprefixed.SBC_A_H] = SBC((Register.A, true), (Register.H, true));
            m[Unprefixed.SBC_A_L] = SBC((Register.A, true), (Register.L, true));
            m[Unprefixed.SBC_A_AT_HL] = SBC((Register.A, true), (WideRegister.HL, false));
            m[Unprefixed.SBC_A_A] = SBC((Register.A, true), (Register.A, true));
            m[Unprefixed.AND_B] = AND((Register.B, true));
            m[Unprefixed.AND_C] = AND((Register.C, true));
            m[Unprefixed.AND_D] = AND((Register.D, true));
            m[Unprefixed.AND_E] = AND((Register.E, true));
            m[Unprefixed.AND_H] = AND((Register.H, true));
            m[Unprefixed.AND_L] = AND((Register.L, true));
            m[Unprefixed.AND_AT_HL] = AND((WideRegister.HL, false));
            m[Unprefixed.AND_A] = AND((Register.A, true));
            m[Unprefixed.XOR_B] = XOR((Register.B, true));
            m[Unprefixed.XOR_C] = XOR((Register.C, true));
            m[Unprefixed.XOR_D] = XOR((Register.D, true));
            m[Unprefixed.XOR_E] = XOR((Register.E, true));
            m[Unprefixed.XOR_H] = XOR((Register.H, true));
            m[Unprefixed.XOR_L] = XOR((Register.L, true));
            m[Unprefixed.XOR_AT_HL] = XOR((WideRegister.HL, false));
            m[Unprefixed.XOR_A] = XOR((Register.A, true));
            m[Unprefixed.OR_B] = OR((Register.B, true));
            m[Unprefixed.OR_C] = OR((Register.C, true));
            m[Unprefixed.OR_D] = OR((Register.D, true));
            m[Unprefixed.OR_E] = OR((Register.E, true));
            m[Unprefixed.OR_H] = OR((Register.H, true));
            m[Unprefixed.OR_L] = OR((Register.L, true));
            m[Unprefixed.OR_AT_HL] = OR((WideRegister.HL, false));
            m[Unprefixed.OR_A] = OR((Register.A, true));
            m[Unprefixed.CP_B] = CP((Register.B, true));
            m[Unprefixed.CP_C] = CP((Register.C, true));
            m[Unprefixed.CP_D] = CP((Register.D, true));
            m[Unprefixed.CP_E] = CP((Register.E, true));
            m[Unprefixed.CP_H] = CP((Register.H, true));
            m[Unprefixed.CP_L] = CP((Register.L, true));
            m[Unprefixed.CP_AT_HL] = CP((WideRegister.HL, false));
            m[Unprefixed.CP_A] = CP((Register.A, true));
            m[Unprefixed.RET_NZ] = RET((Flag.NZ, true));
            m[Unprefixed.POP_BC] = POP((WideRegister.BC, true));
            m[Unprefixed.JP_NZ_a16] = JP((Flag.NZ, true), (DMGInteger.a16, true));
            m[Unprefixed.JP_a16] = JP((DMGInteger.a16, true));
            m[Unprefixed.CALL_NZ_a16] = CALL((Flag.NZ, true), (DMGInteger.a16, true));
            m[Unprefixed.PUSH_BC] = PUSH((WideRegister.BC, true));
            m[Unprefixed.ADD_A_d8] = ADD((Register.A, true), (DMGInteger.d8, true));
            m[Unprefixed.RST_00H] = RST((0x0, true));
            m[Unprefixed.RET_Zero] = RET((Flag.Z, true));
            m[Unprefixed.RET] = RET();
            m[Unprefixed.JP_Zero_a16] = JP((Flag.Z, true), (DMGInteger.a16, true));
            m[Unprefixed.PREFIX] = PREFIX();
            m[Unprefixed.CALL_Zero_a16] = CALL((Flag.Z, true), (DMGInteger.a16, true));
            m[Unprefixed.CALL_a16] = CALL((DMGInteger.a16, true));
            m[Unprefixed.ADC_A_d8] = ADC((Register.A, true), (DMGInteger.d8, true));
            m[Unprefixed.RST_08H] = RST((0x8, true));
            m[Unprefixed.RET_NC] = RET((Flag.NC, true));
            m[Unprefixed.POP_DE] = POP((WideRegister.DE, true));
            m[Unprefixed.JP_NC_a16] = JP((Flag.NC, true), (DMGInteger.a16, true));
            m[Unprefixed.ILLEGAL_D3] = ILLEGAL_D3();
            m[Unprefixed.CALL_NC_a16] = CALL((Flag.NC, true), (DMGInteger.a16, true));
            m[Unprefixed.PUSH_DE] = PUSH((WideRegister.DE, true));
            m[Unprefixed.SUB_d8] = SUB((DMGInteger.d8, true));
            m[Unprefixed.RST_10H] = RST((0x10, true));
            m[Unprefixed.RET_Carry] = RET((Flag.C, true));
            m[Unprefixed.RETI] = RETI();
            m[Unprefixed.JP_Carry_a16] = JP((Flag.C, true), (DMGInteger.a16, true));
            m[Unprefixed.ILLEGAL_DB] = ILLEGAL_DB();
            m[Unprefixed.CALL_Carry_a16] = CALL((Flag.C, true), (DMGInteger.a16, true));
            m[Unprefixed.ILLEGAL_DD] = ILLEGAL_DD();
            m[Unprefixed.SBC_A_d8] = SBC((Register.A, true), (DMGInteger.d8, true));
            m[Unprefixed.RST_18H] = RST((0x18, true));
            m[Unprefixed.LDH_AT_a8_A] = LDH((DMGInteger.a8, false), (Register.A, true));
            m[Unprefixed.POP_HL] = POP((WideRegister.HL, true));
            m[Unprefixed.LD_AT_C_A] = LD((Register.C, false), (Register.A, true));
            m[Unprefixed.ILLEGAL_E3] = ILLEGAL_E3();
            m[Unprefixed.ILLEGAL_E4] = ILLEGAL_E4();
            m[Unprefixed.PUSH_HL] = PUSH((WideRegister.HL, true));
            m[Unprefixed.AND_d8] = AND((DMGInteger.d8, true));
            m[Unprefixed.RST_20H] = RST((0x20, true));
            m[Unprefixed.ADD_SP_r8] = ADD((WideRegister.SP, true), (DMGInteger.r8, true));
            m[Unprefixed.JP_HL] = JP((WideRegister.HL, true));
            m[Unprefixed.LD_AT_a16_A] = LD((DMGInteger.a16, false), (Register.A, true));
            m[Unprefixed.ILLEGAL_EB] = ILLEGAL_EB();
            m[Unprefixed.ILLEGAL_EC] = ILLEGAL_EC();
            m[Unprefixed.ILLEGAL_ED] = ILLEGAL_ED();
            m[Unprefixed.XOR_d8] = XOR((DMGInteger.d8, true));
            m[Unprefixed.RST_28H] = RST((0x28, true));
            m[Unprefixed.LDH_A_AT_a8] = LDH((Register.A, true), (DMGInteger.a8, false));
            m[Unprefixed.POP_AF] = POP((WideRegister.AF, true));
            m[Unprefixed.LD_A_AT_C] = LD((Register.A, true), (Register.C, false));
            m[Unprefixed.DI] = DI();
            m[Unprefixed.ILLEGAL_F4] = ILLEGAL_F4();
            m[Unprefixed.PUSH_AF] = PUSH((BC: WideRegister.AF, true));
            m[Unprefixed.OR_d8] = OR((DMGInteger.d8, true));
            m[Unprefixed.RST_30H] = RST((0x30, true));
            m[Unprefixed.LD_HLI_SP] = LD((WideRegister.HL, true), (WideRegister.SP, true));
            m[Unprefixed.LD_SP_HL] = LD((WideRegister.SP, true), (WideRegister.HL, true));
            m[Unprefixed.LD_A_AT_a16] = LD((Register.A, true), (DMGInteger.a16, false));
            m[Unprefixed.EI] = EI();
            m[Unprefixed.ILLEGAL_FC] = ILLEGAL_FC();
            m[Unprefixed.ILLEGAL_FD] = ILLEGAL_FD();
            m[Unprefixed.CP_d8] = CP((DMGInteger.d8, true));
            m[Unprefixed.RST_38H] = RST((0x38, true));
            return m;
        }

        private Action LDH((Register A, bool) p1, (DMGInteger a8, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action LDH((DMGInteger a8, bool) p1, (Register A, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action CP((DMGInteger d8, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_FD()
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_FC()
        {
            throw new NotImplementedException();
        }

        private Action EI()
        {
            throw new NotImplementedException();
        }

        private Action LD((WideRegister HL, bool) p1, (WideRegister SP, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action OR((DMGInteger d8, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_F4()
        {
            throw new NotImplementedException();
        }

        private Action DI()
        {
            throw new NotImplementedException();
        }

        private Action XOR((DMGInteger d8, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_ED()
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_EC()
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_EB()
        {
            throw new NotImplementedException();
        }

        private Action LD((DMGInteger a16, bool) p1, (Register A, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action JP((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action ADD((WideRegister SP, bool) p1, (DMGInteger r8, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action AND((DMGInteger d8, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_E4()
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_E3()
        {
            throw new NotImplementedException();
        }

        private Action SBC((Register A, bool) p1, (DMGInteger d8, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_DD()
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_DB()
        {
            throw new NotImplementedException();
        }

        private Action RETI()
        {
            throw new NotImplementedException();
        }

        private Action SUB((DMGInteger d8, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action ILLEGAL_D3()
        {
            throw new NotImplementedException();
        }

        private Action ADC((Register A, bool) p1, (DMGInteger d8, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action RST((int, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action CALL((DMGInteger a16, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action PREFIX()
        {
            throw new NotImplementedException();
        }

        private Action RET()
        {
            throw new NotImplementedException();
        }

        private Action RST(int v1, object h, bool v2)
        {
            throw new NotImplementedException();
        }

        private Action ADD((Register A, bool) p1, (DMGInteger d8, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action PUSH((WideRegister BC, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action CALL((Flag NZ, bool) p1, (DMGInteger a16, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action JP((DMGInteger a16, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action JP((Flag NZ, bool) p1, (DMGInteger a16, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action RET((Flag NZ, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action POP((WideRegister BC, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action CP((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action OR((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action CP((Register C, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action OR((Register B, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action XOR((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action XOR((Register H, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action AND((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action AND((Register H, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SBC((Register A, bool) p1, (WideRegister HL, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action SBC((Register A, bool) p1, (Register B, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action SUB((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SUB((Register D, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action ADC((Register A, bool) p1, (WideRegister HL, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action ADC((Register A, bool) p1, (Register B, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action ADD((Register A, bool) p1, (WideRegister HL, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action ADD((Register A, bool) p1, (Register B, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action HALT()
        {
            throw new NotImplementedException();
        }

        private Action JR((DMGInteger r8, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action LD((Register B, bool) p1, (Register B, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action CCF()
        {
            throw new NotImplementedException();
        }

        private Action SCF()
        {
            throw new NotImplementedException();
        }

        private Action CPL()
        {
            throw new NotImplementedException();
        }

        private Action DAA()
        {
            throw new NotImplementedException();
        }

        private Action JR((Flag NZ, bool) p1, (DMGInteger r8, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action RRA()
        {
            throw new NotImplementedException();
        }

        private Action JR((DMGInteger r8, bool) p, (DMGInteger r8, bool) p1)
        {
            throw new NotImplementedException();
        }

        private Action RLA()
        {
            throw new NotImplementedException();
        }

        private Action STOP()
        {
            throw new NotImplementedException();
        }

        private Action RRCA()
        {
            throw new NotImplementedException();
        }

        private Action DEC((WideRegister BC, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action LD((Register A, bool) p1, (WideRegister BC, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action ADD((WideRegister HL, bool) p1, (WideRegister BC, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action LD((DMGInteger a16, bool) p1, (WideRegister SP, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action RLCA()
        {
            throw new NotImplementedException();
        }

        private Action LD((Register B, bool) p1, (DMGInteger d8, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action DEC((Register B, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action INC((Register B, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action LD((WideRegister BC, bool) p1, (Register A, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action INC((WideRegister BC, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action LD((WideRegister BC, bool) p1, (DMGInteger d16, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action NOP()
        {
            throw new NotImplementedException();
        }

        public Dictionary<Cbprefixed, Action> MakeTableCB()
        {
            Dictionary<Cbprefixed, Action> m = new Dictionary<Cbprefixed, Action>();
            m[Cbprefixed.RLC_B] = RLC((Register.B, true));
            m[Cbprefixed.RLC_C] = RLC((Register.C, true));
            m[Cbprefixed.RLC_D] = RLC((Register.D, true));
            m[Cbprefixed.RLC_E] = RLC((Register.E, true));
            m[Cbprefixed.RLC_H] = RLC((Register.H, true));
            m[Cbprefixed.RLC_L] = RLC((Register.L, true));
            m[Cbprefixed.RLC_AT_HL] = RLC((WideRegister.HL, false));
            m[Cbprefixed.RLC_A] = RLC((Register.A, true));
            m[Cbprefixed.RRC_B] = RRC((Register.B, true));
            m[Cbprefixed.RRC_C] = RRC((Register.C, true));
            m[Cbprefixed.RRC_D] = RRC((Register.D, true));
            m[Cbprefixed.RRC_E] = RRC((Register.E, true));
            m[Cbprefixed.RRC_H] = RRC((Register.H, true));
            m[Cbprefixed.RRC_L] = RRC((Register.L, true));
            m[Cbprefixed.RRC_AT_HL] = RRC((WideRegister.HL, false));
            m[Cbprefixed.RRC_A] = RRC((Register.A, true));
            m[Cbprefixed.RL_B] = RL((Register.B, true));
            m[Cbprefixed.RL_C] = RL((Register.C, true));
            m[Cbprefixed.RL_D] = RL((Register.D, true));
            m[Cbprefixed.RL_E] = RL((Register.E, true));
            m[Cbprefixed.RL_H] = RL((Register.H, true));
            m[Cbprefixed.RL_L] = RL((Register.L, true));
            m[Cbprefixed.RL_AT_HL] = RL((WideRegister.HL, false));
            m[Cbprefixed.RL_A] = RL((Register.A, true));
            m[Cbprefixed.RR_B] = RR((Register.B, true));
            m[Cbprefixed.RR_C] = RR((Register.C, true));
            m[Cbprefixed.RR_D] = RR((Register.D, true));
            m[Cbprefixed.RR_E] = RR((Register.E, true));
            m[Cbprefixed.RR_H] = RR((Register.H, true));
            m[Cbprefixed.RR_L] = RR((Register.L, true));
            m[Cbprefixed.RR_AT_HL] = RR((WideRegister.HL, false));
            m[Cbprefixed.RR_A] = RR((Register.A, true));
            m[Cbprefixed.SLA_B] = SLA((Register.B, true));
            m[Cbprefixed.SLA_C] = SLA((Register.C, true));
            m[Cbprefixed.SLA_D] = SLA((Register.D, true));
            m[Cbprefixed.SLA_E] = SLA((Register.E, true));
            m[Cbprefixed.SLA_H] = SLA((Register.H, true));
            m[Cbprefixed.SLA_L] = SLA((Register.L, true));
            m[Cbprefixed.SLA_AT_HL] = SLA((WideRegister.HL, false));
            m[Cbprefixed.SLA_A] = SLA((Register.A, true));
            m[Cbprefixed.SRA_B] = SRA((Register.B, true));
            m[Cbprefixed.SRA_C] = SRA((Register.C, true));
            m[Cbprefixed.SRA_D] = SRA((Register.D, true));
            m[Cbprefixed.SRA_E] = SRA((Register.E, true));
            m[Cbprefixed.SRA_H] = SRA((Register.H, true));
            m[Cbprefixed.SRA_L] = SRA((Register.L, true));
            m[Cbprefixed.SRA_AT_HL] = SRA((WideRegister.HL, false));
            m[Cbprefixed.SRA_A] = SRA((Register.A, true));
            m[Cbprefixed.SWAP_B] = SWAP((Register.B, true));
            m[Cbprefixed.SWAP_C] = SWAP((Register.C, true));
            m[Cbprefixed.SWAP_D] = SWAP((Register.D, true));
            m[Cbprefixed.SWAP_E] = SWAP((Register.E, true));
            m[Cbprefixed.SWAP_H] = SWAP((Register.H, true));
            m[Cbprefixed.SWAP_L] = SWAP((Register.L, true));
            m[Cbprefixed.SWAP_AT_HL] = SWAP((WideRegister.HL, false));
            m[Cbprefixed.SWAP_A] = SWAP((Register.A, true));
            m[Cbprefixed.SRL_B] = SRL((Register.B, true));
            m[Cbprefixed.SRL_C] = SRL((Register.C, true));
            m[Cbprefixed.SRL_D] = SRL((Register.D, true));
            m[Cbprefixed.SRL_E] = SRL((Register.E, true));
            m[Cbprefixed.SRL_H] = SRL((Register.H, true));
            m[Cbprefixed.SRL_L] = SRL((Register.L, true));
            m[Cbprefixed.SRL_AT_HL] = SRL((WideRegister.HL, false));
            m[Cbprefixed.SRL_A] = SRL((Register.A, true));
            m[Cbprefixed.BIT_0_B] = BIT((0, true), (Register.B, true));
            m[Cbprefixed.BIT_0_C] = BIT((0, true), (Register.C, true));
            m[Cbprefixed.BIT_0_D] = BIT((0, true), (Register.D, true));
            m[Cbprefixed.BIT_0_E] = BIT((0, true), (Register.E, true));
            m[Cbprefixed.BIT_0_H] = BIT((0, true), (Register.H, true));
            m[Cbprefixed.BIT_0_L] = BIT((0, true), (Register.L, true));
            m[Cbprefixed.BIT_0_AT_HL] = BIT((0, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_0_A] = BIT((0, true), (Register.A, true));
            m[Cbprefixed.BIT_1_B] = BIT((1, true), (Register.B, true));
            m[Cbprefixed.BIT_1_C] = BIT((1, true), (Register.C, true));
            m[Cbprefixed.BIT_1_D] = BIT((1, true), (Register.D, true));
            m[Cbprefixed.BIT_1_E] = BIT((1, true), (Register.E, true));
            m[Cbprefixed.BIT_1_H] = BIT((1, true), (Register.H, true));
            m[Cbprefixed.BIT_1_L] = BIT((1, true), (Register.L, true));
            m[Cbprefixed.BIT_1_AT_HL] = BIT((1, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_1_A] = BIT((1, true), (Register.A, true));
            m[Cbprefixed.BIT_2_B] = BIT((2, true), (Register.B, true));
            m[Cbprefixed.BIT_2_C] = BIT((2, true), (Register.C, true));
            m[Cbprefixed.BIT_2_D] = BIT((2, true), (Register.D, true));
            m[Cbprefixed.BIT_2_E] = BIT((2, true), (Register.E, true));
            m[Cbprefixed.BIT_2_H] = BIT((2, true), (Register.H, true));
            m[Cbprefixed.BIT_2_L] = BIT((2, true), (Register.L, true));
            m[Cbprefixed.BIT_2_AT_HL] = BIT((2, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_2_A] = BIT((2, true), (Register.A, true));
            m[Cbprefixed.BIT_3_B] = BIT((3, true), (Register.B, true));
            m[Cbprefixed.BIT_3_C] = BIT((3, true), (Register.C, true));
            m[Cbprefixed.BIT_3_D] = BIT((3, true), (Register.D, true));
            m[Cbprefixed.BIT_3_E] = BIT((3, true), (Register.E, true));
            m[Cbprefixed.BIT_3_H] = BIT((3, true), (Register.H, true));
            m[Cbprefixed.BIT_3_L] = BIT((3, true), (Register.L, true));
            m[Cbprefixed.BIT_3_AT_HL] = BIT((3, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_3_A] = BIT((3, true), (Register.A, true));
            m[Cbprefixed.BIT_4_B] = BIT((4, true), (Register.B, true));
            m[Cbprefixed.BIT_4_C] = BIT((4, true), (Register.C, true));
            m[Cbprefixed.BIT_4_D] = BIT((4, true), (Register.D, true));
            m[Cbprefixed.BIT_4_E] = BIT((4, true), (Register.E, true));
            m[Cbprefixed.BIT_4_H] = BIT((4, true), (Register.H, true));
            m[Cbprefixed.BIT_4_L] = BIT((4, true), (Register.L, true));
            m[Cbprefixed.BIT_4_AT_HL] = BIT((4, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_4_A] = BIT((4, true), (Register.A, true));
            m[Cbprefixed.BIT_5_B] = BIT((5, true), (Register.B, true));
            m[Cbprefixed.BIT_5_C] = BIT((5, true), (Register.C, true));
            m[Cbprefixed.BIT_5_D] = BIT((5, true), (Register.D, true));
            m[Cbprefixed.BIT_5_E] = BIT((5, true), (Register.E, true));
            m[Cbprefixed.BIT_5_H] = BIT((5, true), (Register.H, true));
            m[Cbprefixed.BIT_5_L] = BIT((5, true), (Register.L, true));
            m[Cbprefixed.BIT_5_AT_HL] = BIT((5, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_5_A] = BIT((5, true), (Register.A, true));
            m[Cbprefixed.BIT_6_B] = BIT((6, true), (Register.B, true));
            m[Cbprefixed.BIT_6_C] = BIT((6, true), (Register.C, true));
            m[Cbprefixed.BIT_6_D] = BIT((6, true), (Register.D, true));
            m[Cbprefixed.BIT_6_E] = BIT((6, true), (Register.E, true));
            m[Cbprefixed.BIT_6_H] = BIT((6, true), (Register.H, true));
            m[Cbprefixed.BIT_6_L] = BIT((6, true), (Register.L, true));
            m[Cbprefixed.BIT_6_AT_HL] = BIT((6, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_6_A] = BIT((6, true), (Register.A, true));
            m[Cbprefixed.BIT_7_B] = BIT((7, true), (Register.B, true));
            m[Cbprefixed.BIT_7_C] = BIT((7, true), (Register.C, true));
            m[Cbprefixed.BIT_7_D] = BIT((7, true), (Register.D, true));
            m[Cbprefixed.BIT_7_E] = BIT((7, true), (Register.E, true));
            m[Cbprefixed.BIT_7_H] = BIT((7, true), (Register.H, true));
            m[Cbprefixed.BIT_7_L] = BIT((7, true), (Register.L, true));
            m[Cbprefixed.BIT_7_AT_HL] = BIT((7, true), (WideRegister.HL, false));
            m[Cbprefixed.BIT_7_A] = BIT((7, true), (Register.A, true));
            m[Cbprefixed.RES_0_B] = RES((0, true), (Register.B, true));
            m[Cbprefixed.RES_0_C] = RES((0, true), (Register.C, true));
            m[Cbprefixed.RES_0_D] = RES((0, true), (Register.D, true));
            m[Cbprefixed.RES_0_E] = RES((0, true), (Register.E, true));
            m[Cbprefixed.RES_0_H] = RES((0, true), (Register.H, true));
            m[Cbprefixed.RES_0_L] = RES((0, true), (Register.L, true));
            m[Cbprefixed.RES_0_AT_HL] = RES((0, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_0_A] = RES((0, true), (Register.A, true));
            m[Cbprefixed.RES_1_B] = RES((1, true), (Register.B, true));
            m[Cbprefixed.RES_1_C] = RES((1, true), (Register.C, true));
            m[Cbprefixed.RES_1_D] = RES((1, true), (Register.D, true));
            m[Cbprefixed.RES_1_E] = RES((1, true), (Register.E, true));
            m[Cbprefixed.RES_1_H] = RES((1, true), (Register.H, true));
            m[Cbprefixed.RES_1_L] = RES((1, true), (Register.L, true));
            m[Cbprefixed.RES_1_AT_HL] = RES((1, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_1_A] = RES((1, true), (Register.A, true));
            m[Cbprefixed.RES_2_B] = RES((2, true), (Register.B, true));
            m[Cbprefixed.RES_2_C] = RES((2, true), (Register.C, true));
            m[Cbprefixed.RES_2_D] = RES((2, true), (Register.D, true));
            m[Cbprefixed.RES_2_E] = RES((2, true), (Register.E, true));
            m[Cbprefixed.RES_2_H] = RES((2, true), (Register.H, true));
            m[Cbprefixed.RES_2_L] = RES((2, true), (Register.L, true));
            m[Cbprefixed.RES_2_AT_HL] = RES((2, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_2_A] = RES((2, true), (Register.A, true));
            m[Cbprefixed.RES_3_B] = RES((3, true), (Register.B, true));
            m[Cbprefixed.RES_3_C] = RES((3, true), (Register.C, true));
            m[Cbprefixed.RES_3_D] = RES((3, true), (Register.D, true));
            m[Cbprefixed.RES_3_E] = RES((3, true), (Register.E, true));
            m[Cbprefixed.RES_3_H] = RES((3, true), (Register.H, true));
            m[Cbprefixed.RES_3_L] = RES((3, true), (Register.L, true));
            m[Cbprefixed.RES_3_AT_HL] = RES((3, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_3_A] = RES((3, true), (Register.A, true));
            m[Cbprefixed.RES_4_B] = RES((4, true), (Register.B, true));
            m[Cbprefixed.RES_4_C] = RES((4, true), (Register.C, true));
            m[Cbprefixed.RES_4_D] = RES((4, true), (Register.D, true));
            m[Cbprefixed.RES_4_E] = RES((4, true), (Register.E, true));
            m[Cbprefixed.RES_4_H] = RES((4, true), (Register.H, true));
            m[Cbprefixed.RES_4_L] = RES((4, true), (Register.L, true));
            m[Cbprefixed.RES_4_AT_HL] = RES((4, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_4_A] = RES((4, true), (Register.A, true));
            m[Cbprefixed.RES_5_B] = RES((5, true), (Register.B, true));
            m[Cbprefixed.RES_5_C] = RES((5, true), (Register.C, true));
            m[Cbprefixed.RES_5_D] = RES((5, true), (Register.D, true));
            m[Cbprefixed.RES_5_E] = RES((5, true), (Register.E, true));
            m[Cbprefixed.RES_5_H] = RES((5, true), (Register.H, true));
            m[Cbprefixed.RES_5_L] = RES((5, true), (Register.L, true));
            m[Cbprefixed.RES_5_AT_HL] = RES((5, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_5_A] = RES((5, true), (Register.A, true));
            m[Cbprefixed.RES_6_B] = RES((6, true), (Register.B, true));
            m[Cbprefixed.RES_6_C] = RES((6, true), (Register.C, true));
            m[Cbprefixed.RES_6_D] = RES((6, true), (Register.D, true));
            m[Cbprefixed.RES_6_E] = RES((6, true), (Register.E, true));
            m[Cbprefixed.RES_6_H] = RES((6, true), (Register.H, true));
            m[Cbprefixed.RES_6_L] = RES((6, true), (Register.L, true));
            m[Cbprefixed.RES_6_AT_HL] = RES((6, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_6_A] = RES((6, true), (Register.A, true));
            m[Cbprefixed.RES_7_B] = RES((7, true), (Register.B, true));
            m[Cbprefixed.RES_7_C] = RES((7, true), (Register.C, true));
            m[Cbprefixed.RES_7_D] = RES((7, true), (Register.D, true));
            m[Cbprefixed.RES_7_E] = RES((7, true), (Register.E, true));
            m[Cbprefixed.RES_7_H] = RES((7, true), (Register.H, true));
            m[Cbprefixed.RES_7_L] = RES((7, true), (Register.L, true));
            m[Cbprefixed.RES_7_AT_HL] = RES((7, true), (WideRegister.HL, false));
            m[Cbprefixed.RES_7_A] = RES((7, true), (Register.A, true));
            m[Cbprefixed.SET_0_B] = SET((0, true), (Register.B, true));
            m[Cbprefixed.SET_0_C] = SET((0, true), (Register.C, true));
            m[Cbprefixed.SET_0_D] = SET((0, true), (Register.D, true));
            m[Cbprefixed.SET_0_E] = SET((0, true), (Register.E, true));
            m[Cbprefixed.SET_0_H] = SET((0, true), (Register.H, true));
            m[Cbprefixed.SET_0_L] = SET((0, true), (Register.L, true));
            m[Cbprefixed.SET_0_AT_HL] = SET((0, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_0_A] = SET((0, true), (Register.A, true));
            m[Cbprefixed.SET_1_B] = SET((1, true), (Register.B, true));
            m[Cbprefixed.SET_1_C] = SET((1, true), (Register.C, true));
            m[Cbprefixed.SET_1_D] = SET((1, true), (Register.D, true));
            m[Cbprefixed.SET_1_E] = SET((1, true), (Register.E, true));
            m[Cbprefixed.SET_1_H] = SET((1, true), (Register.H, true));
            m[Cbprefixed.SET_1_L] = SET((1, true), (Register.L, true));
            m[Cbprefixed.SET_1_AT_HL] = SET((1, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_1_A] = SET((1, true), (Register.A, true));
            m[Cbprefixed.SET_2_B] = SET((2, true), (Register.B, true));
            m[Cbprefixed.SET_2_C] = SET((2, true), (Register.C, true));
            m[Cbprefixed.SET_2_D] = SET((2, true), (Register.D, true));
            m[Cbprefixed.SET_2_E] = SET((2, true), (Register.E, true));
            m[Cbprefixed.SET_2_H] = SET((2, true), (Register.H, true));
            m[Cbprefixed.SET_2_L] = SET((2, true), (Register.L, true));
            m[Cbprefixed.SET_2_AT_HL] = SET((2, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_2_A] = SET((2, true), (Register.A, true));
            m[Cbprefixed.SET_3_B] = SET((3, true), (Register.B, true));
            m[Cbprefixed.SET_3_C] = SET((3, true), (Register.C, true));
            m[Cbprefixed.SET_3_D] = SET((3, true), (Register.D, true));
            m[Cbprefixed.SET_3_E] = SET((3, true), (Register.E, true));
            m[Cbprefixed.SET_3_H] = SET((3, true), (Register.H, true));
            m[Cbprefixed.SET_3_L] = SET((3, true), (Register.L, true));
            m[Cbprefixed.SET_3_AT_HL] = SET((3, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_3_A] = SET((3, true), (Register.A, true));
            m[Cbprefixed.SET_4_B] = SET((4, true), (Register.B, true));
            m[Cbprefixed.SET_4_C] = SET((4, true), (Register.C, true));
            m[Cbprefixed.SET_4_D] = SET((4, true), (Register.D, true));
            m[Cbprefixed.SET_4_E] = SET((4, true), (Register.E, true));
            m[Cbprefixed.SET_4_H] = SET((4, true), (Register.H, true));
            m[Cbprefixed.SET_4_L] = SET((4, true), (Register.L, true));
            m[Cbprefixed.SET_4_AT_HL] = SET((4, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_4_A] = SET((4, true), (Register.A, true));
            m[Cbprefixed.SET_5_B] = SET((5, true), (Register.B, true));
            m[Cbprefixed.SET_5_C] = SET((5, true), (Register.C, true));
            m[Cbprefixed.SET_5_D] = SET((5, true), (Register.D, true));
            m[Cbprefixed.SET_5_E] = SET((5, true), (Register.E, true));
            m[Cbprefixed.SET_5_H] = SET((5, true), (Register.H, true));
            m[Cbprefixed.SET_5_L] = SET((5, true), (Register.L, true));
            m[Cbprefixed.SET_5_AT_HL] = SET((5, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_5_A] = SET((5, true), (Register.A, true));
            m[Cbprefixed.SET_6_B] = SET((6, true), (Register.B, true));
            m[Cbprefixed.SET_6_C] = SET((6, true), (Register.C, true));
            m[Cbprefixed.SET_6_D] = SET((6, true), (Register.D, true));
            m[Cbprefixed.SET_6_E] = SET((6, true), (Register.E, true));
            m[Cbprefixed.SET_6_H] = SET((6, true), (Register.H, true));
            m[Cbprefixed.SET_6_L] = SET((6, true), (Register.L, true));
            m[Cbprefixed.SET_6_AT_HL] = SET((6, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_6_A] = SET((6, true), (Register.A, true));
            m[Cbprefixed.SET_7_B] = SET((7, true), (Register.B, true));
            m[Cbprefixed.SET_7_C] = SET((7, true), (Register.C, true));
            m[Cbprefixed.SET_7_D] = SET((7, true), (Register.D, true));
            m[Cbprefixed.SET_7_E] = SET((7, true), (Register.E, true));
            m[Cbprefixed.SET_7_H] = SET((7, true), (Register.H, true));
            m[Cbprefixed.SET_7_L] = SET((7, true), (Register.L, true));
            m[Cbprefixed.SET_7_AT_HL] = SET((7, true), (WideRegister.HL, false));
            m[Cbprefixed.SET_7_A] = SET((7, true), (Register.A, true));
            return m;
        }

        private Action SET((int, bool) p1, (WideRegister HL, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action SET((int, bool) p1, (Register B, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action RES((int, bool) p1, (WideRegister HL, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action RES((int, bool) p1, (Register B, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action BIT((int, bool) p1, (WideRegister HL, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action BIT((int, bool) p1, (Register B, bool) p2)
        {
            throw new NotImplementedException();
        }

        private Action SRL((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SRL((Register C, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SWAP((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SWAP((Register B, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SRA((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SRA((Register C, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SLA((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action SLA((Register B, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RR((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RR((Register B, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RL((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RL((Register B, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RRC((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RRC((Register L, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RLC((WideRegister HL, bool) p)
        {
            throw new NotImplementedException();
        }

        private Action RLC((Register B, bool) p)
        {
            throw new NotImplementedException();
        }
    }
}