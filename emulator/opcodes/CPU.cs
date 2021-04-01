﻿using System;

namespace emulator
{
    public partial class CPU
    {
        private readonly Action[] StdOps;
        private readonly Action[] CbOps;
        private readonly InterruptRegisters ISR;
        public readonly Registers Registers;
        private readonly MMU Memory;
        private readonly ProgramCounter Pc;
        public ushort PC { get => Pc.Value; set => Pc.Value = value; }

        private HaltState Halted = HaltState.off;

        public Action Op(Opcode op) => StdOps[(int)op];

        public Action Op(CBOpcode op) => CbOps[(int)op];

        public CPU(MMU mMU, InterruptRegisters interruptRegisters, ProgramCounter PC)
        {
            Memory = mMU;
            ISR = interruptRegisters;
            Pc = PC;

            StdOps = new Action[0x100];
            StdOps[(int)Opcode.NOP] = NOP(4);
            StdOps[(int)Opcode.LD_AT_DE_A] = LD((WideRegister.DE, Postfix.unchanged), Register.A, 8);
            StdOps[(int)Opcode.LD_AT_BC_A] = LD((WideRegister.BC, Postfix.unchanged), Register.A, 8);
            StdOps[(int)Opcode.INC_BC] = INC(WideRegister.BC, 8);
            StdOps[(int)Opcode.INC_B] = INC(Register.B, 4);
            StdOps[(int)Opcode.DEC_B] = DEC(Register.B, 4);
            StdOps[(int)Opcode.LD_B_d8] = LD_D8(Register.B, 8);
            StdOps[(int)Opcode.RLCA] = RLCA(4);
            StdOps[(int)Opcode.LD_AT_a16_SP] = WriteSPToMem(20);
            StdOps[(int)Opcode.ADD_HL_BC] = ADD(WideRegister.BC, 8);
            StdOps[(int)Opcode.LD_A_AT_BC] = LD(Register.A, (WideRegister.BC, Postfix.unchanged), 8);
            StdOps[(int)Opcode.DEC_BC] = DEC(WideRegister.BC, 8);
            StdOps[(int)Opcode.INC_C] = INC(Register.C, 4);
            StdOps[(int)Opcode.DEC_C] = DEC(Register.C, 4);
            StdOps[(int)Opcode.LD_C_d8] = LD_D8(Register.C, 8);
            StdOps[(int)Opcode.RRCA] = RRCA(4);
            StdOps[(int)Opcode.STOP] = STOP(4);
            StdOps[(int)Opcode.LD_DE_d16] = LD_D16(WideRegister.DE, 12);
            StdOps[(int)Opcode.LD_BC_d16] = LD_D16(WideRegister.BC, 12);
            StdOps[(int)Opcode.INC_DE] = INC(WideRegister.DE, 8);
            StdOps[(int)Opcode.INC_D] = INC(Register.D, 4);
            StdOps[(int)Opcode.DEC_D] = DEC(Register.D, 4);
            StdOps[(int)Opcode.LD_D_d8] = LD_D8(Register.D, 8);
            StdOps[(int)Opcode.RLA] = RLA(4);
            StdOps[(int)Opcode.JR_r8] = JR(12);
            StdOps[(int)Opcode.ADD_HL_DE] = ADD(WideRegister.DE, 8);
            StdOps[(int)Opcode.LD_A_AT_DE] = LD(Register.A, (WideRegister.DE, Postfix.unchanged), 8);
            StdOps[(int)Opcode.DEC_DE] = DEC(WideRegister.DE, 8);
            StdOps[(int)Opcode.INC_E] = INC(Register.E, 4);
            StdOps[(int)Opcode.DEC_E] = DEC(Register.E, 4);
            StdOps[(int)Opcode.LD_E_d8] = LD_D8(Register.E, 8);
            StdOps[(int)Opcode.RRA] = RRA(4);
            StdOps[(int)Opcode.JR_NZ_r8] = JR(Flag.NZ, 12, 8);
            StdOps[(int)Opcode.LD_HL_d16] = LD_D16(WideRegister.HL, 12);
            StdOps[(int)Opcode.LDI_AT_HL_A] = LD((WideRegister.HL, Postfix.increment), Register.A, 8);
            StdOps[(int)Opcode.INC_HL] = INC(WideRegister.HL, 8);
            StdOps[(int)Opcode.INC_H] = INC(Register.H, 4);
            StdOps[(int)Opcode.DEC_H] = DEC(Register.H, 4);
            StdOps[(int)Opcode.LD_H_d8] = LD_D8(Register.H, 8);
            StdOps[(int)Opcode.DAA] = DAA(4);
            StdOps[(int)Opcode.JR_Zero_r8] = JR(Flag.Z, 12, 8);
            StdOps[(int)Opcode.ADD_HL_HL] = ADD(WideRegister.HL, 8);
            StdOps[(int)Opcode.LD_AI_AT_HL] = LD(Register.A, (WideRegister.HL, Postfix.increment), 8);
            StdOps[(int)Opcode.DEC_HL] = DEC(WideRegister.HL, 8);
            StdOps[(int)Opcode.INC_L] = INC(Register.L, 4);
            StdOps[(int)Opcode.DEC_L] = DEC(Register.L, 4);
            StdOps[(int)Opcode.LD_L_d8] = LD_D8(Register.L, 8);
            StdOps[(int)Opcode.CPL] = CPL(4);
            StdOps[(int)Opcode.JR_NC_r8] = JR(Flag.NC, 12, 8);
            StdOps[(int)Opcode.LD_SP_d16] = LD_D16(WideRegister.SP, 12);
            StdOps[(int)Opcode.LDD_AT_HL_A] = LD((WideRegister.HL, Postfix.decrement), Register.A, 8);
            StdOps[(int)Opcode.INC_SP] = INC(WideRegister.SP, 8);
            StdOps[(int)Opcode.INC_AT_HL] = INC(Register.HL, 12);
            StdOps[(int)Opcode.DEC_AT_HL] = DEC(Register.HL, 12);
            StdOps[(int)Opcode.LD_AT_HL_d8] = LD_D8(Register.HL, 12);
            StdOps[(int)Opcode.SCF] = SCF(4);
            StdOps[(int)Opcode.JR_Carry_r8] = JR(Flag.C, 12, 8);
            StdOps[(int)Opcode.ADD_HL_SP] = ADD(WideRegister.SP, 8);
            StdOps[(int)Opcode.LD_AD_AT_HL] = LD(Register.A, (WideRegister.HL, Postfix.decrement), 8);
            StdOps[(int)Opcode.DEC_SP] = DEC(WideRegister.SP, 8);
            StdOps[(int)Opcode.INC_A] = INC(Register.A, 4);
            StdOps[(int)Opcode.DEC_A] = DEC(Register.A, 4);
            StdOps[(int)Opcode.LD_A_d8] = LD_D8(Register.A, 8);
            StdOps[(int)Opcode.CCF] = CCF(4);
            StdOps[(int)Opcode.LD_B_B] = LD(Register.B, Register.B, 4);
            StdOps[(int)Opcode.LD_B_C] = LD(Register.B, Register.C, 4);
            StdOps[(int)Opcode.LD_B_D] = LD(Register.B, Register.D, 4);
            StdOps[(int)Opcode.LD_B_E] = LD(Register.B, Register.E, 4);
            StdOps[(int)Opcode.LD_B_H] = LD(Register.B, Register.H, 4);
            StdOps[(int)Opcode.LD_B_L] = LD(Register.B, Register.L, 4);
            StdOps[(int)Opcode.LD_B_AT_HL] = LD(Register.B, Register.HL, 8);
            StdOps[(int)Opcode.LD_B_A] = LD(Register.B, Register.A, 4);
            StdOps[(int)Opcode.LD_C_B] = LD(Register.C, Register.B, 4);
            StdOps[(int)Opcode.LD_C_C] = LD(Register.C, Register.C, 4);
            StdOps[(int)Opcode.LD_C_D] = LD(Register.C, Register.D, 4);
            StdOps[(int)Opcode.LD_C_E] = LD(Register.C, Register.E, 4);
            StdOps[(int)Opcode.LD_C_H] = LD(Register.C, Register.H, 4);
            StdOps[(int)Opcode.LD_C_L] = LD(Register.C, Register.L, 4);
            StdOps[(int)Opcode.LD_C_AT_HL] = LD(Register.C, Register.HL, 8);
            StdOps[(int)Opcode.LD_C_A] = LD(Register.C, Register.A, 4);
            StdOps[(int)Opcode.LD_D_B] = LD(Register.D, Register.B, 4);
            StdOps[(int)Opcode.LD_D_C] = LD(Register.D, Register.C, 4);
            StdOps[(int)Opcode.LD_D_D] = LD(Register.D, Register.D, 4);
            StdOps[(int)Opcode.LD_D_E] = LD(Register.D, Register.E, 4);
            StdOps[(int)Opcode.LD_D_H] = LD(Register.D, Register.H, 4);
            StdOps[(int)Opcode.LD_D_L] = LD(Register.D, Register.L, 4);
            StdOps[(int)Opcode.LD_D_AT_HL] = LD(Register.D, Register.HL, 8);
            StdOps[(int)Opcode.LD_D_A] = LD(Register.D, Register.A, 4);
            StdOps[(int)Opcode.LD_E_B] = LD(Register.E, Register.B, 4);
            StdOps[(int)Opcode.LD_E_C] = LD(Register.E, Register.C, 4);
            StdOps[(int)Opcode.LD_E_D] = LD(Register.E, Register.D, 4);
            StdOps[(int)Opcode.LD_E_E] = LD(Register.E, Register.E, 4);
            StdOps[(int)Opcode.LD_E_H] = LD(Register.E, Register.H, 4);
            StdOps[(int)Opcode.LD_E_L] = LD(Register.E, Register.L, 4);
            StdOps[(int)Opcode.LD_E_AT_HL] = LD(Register.E, Register.HL, 8);
            StdOps[(int)Opcode.LD_E_A] = LD(Register.E, Register.A, 4);
            StdOps[(int)Opcode.LD_H_B] = LD(Register.H, Register.B, 4);
            StdOps[(int)Opcode.LD_H_C] = LD(Register.H, Register.C, 4);
            StdOps[(int)Opcode.LD_H_D] = LD(Register.H, Register.D, 4);
            StdOps[(int)Opcode.LD_H_E] = LD(Register.H, Register.E, 4);
            StdOps[(int)Opcode.LD_H_H] = LD(Register.H, Register.H, 4);
            StdOps[(int)Opcode.LD_H_L] = LD(Register.H, Register.L, 4);
            StdOps[(int)Opcode.LD_H_AT_HL] = LD(Register.H, Register.HL, 8);
            StdOps[(int)Opcode.LD_H_A] = LD(Register.H, Register.A, 4);
            StdOps[(int)Opcode.LD_L_B] = LD(Register.L, Register.B, 4);
            StdOps[(int)Opcode.LD_L_C] = LD(Register.L, Register.C, 4);
            StdOps[(int)Opcode.LD_L_D] = LD(Register.L, Register.D, 4);
            StdOps[(int)Opcode.LD_L_E] = LD(Register.L, Register.E, 4);
            StdOps[(int)Opcode.LD_L_H] = LD(Register.L, Register.H, 4);
            StdOps[(int)Opcode.LD_L_L] = LD(Register.L, Register.L, 4);
            StdOps[(int)Opcode.LD_L_AT_HL] = LD(Register.L, Register.HL, 8);
            StdOps[(int)Opcode.LD_L_A] = LD(Register.L, Register.A, 4);
            StdOps[(int)Opcode.LD_AT_HL_B] = LD(Register.HL, Register.B, 8);
            StdOps[(int)Opcode.LD_AT_HL_C] = LD(Register.HL, Register.C, 8);
            StdOps[(int)Opcode.LD_AT_HL_D] = LD(Register.HL, Register.D, 8);
            StdOps[(int)Opcode.LD_AT_HL_E] = LD(Register.HL, Register.E, 8);
            StdOps[(int)Opcode.LD_AT_HL_H] = LD(Register.HL, Register.H, 8);
            StdOps[(int)Opcode.LD_AT_HL_L] = LD(Register.HL, Register.L, 8);
            StdOps[(int)Opcode.HALT] = HALT(4);
            StdOps[(int)Opcode.LD_AT_HL_A] = LD(Register.HL, Register.A, 8);
            StdOps[(int)Opcode.LD_A_B] = LD(Register.A, Register.B, 4);
            StdOps[(int)Opcode.LD_A_C] = LD(Register.A, Register.C, 4);
            StdOps[(int)Opcode.LD_A_D] = LD(Register.A, Register.D, 4);
            StdOps[(int)Opcode.LD_A_E] = LD(Register.A, Register.E, 4);
            StdOps[(int)Opcode.LD_A_H] = LD(Register.A, Register.H, 4);
            StdOps[(int)Opcode.LD_A_L] = LD(Register.A, Register.L, 4);
            StdOps[(int)Opcode.LD_A_AT_HL] = LD(Register.A, Register.HL, 8);
            StdOps[(int)Opcode.LD_A_A] = LD(Register.A, Register.A, 4);
            StdOps[(int)Opcode.ADD_A_B] = ADD(Register.B, 4);
            StdOps[(int)Opcode.ADD_A_C] = ADD(Register.C, 4);
            StdOps[(int)Opcode.ADD_A_D] = ADD(Register.D, 4);
            StdOps[(int)Opcode.ADD_A_E] = ADD(Register.E, 4);
            StdOps[(int)Opcode.ADD_A_H] = ADD(Register.H, 4);
            StdOps[(int)Opcode.ADD_A_L] = ADD(Register.L, 4);
            StdOps[(int)Opcode.ADD_A_AT_HL] = ADD(Register.HL, 8);
            StdOps[(int)Opcode.ADD_A_A] = ADD(Register.A, 4);
            StdOps[(int)Opcode.ADC_A_B] = ADC(Register.B, 4);
            StdOps[(int)Opcode.ADC_A_C] = ADC(Register.C, 4);
            StdOps[(int)Opcode.ADC_A_D] = ADC(Register.D, 4);
            StdOps[(int)Opcode.ADC_A_E] = ADC(Register.E, 4);
            StdOps[(int)Opcode.ADC_A_H] = ADC(Register.H, 4);
            StdOps[(int)Opcode.ADC_A_L] = ADC(Register.L, 4);
            StdOps[(int)Opcode.ADC_A_AT_HL] = ADC(Register.HL, 8);
            StdOps[(int)Opcode.ADC_A_A] = ADC(Register.A, 4);
            StdOps[(int)Opcode.SUB_B] = SUB(Register.B, 4);
            StdOps[(int)Opcode.SUB_C] = SUB(Register.C, 4);
            StdOps[(int)Opcode.SUB_D] = SUB(Register.D, 4);
            StdOps[(int)Opcode.SUB_E] = SUB(Register.E, 4);
            StdOps[(int)Opcode.SUB_H] = SUB(Register.H, 4);
            StdOps[(int)Opcode.SUB_L] = SUB(Register.L, 4);
            StdOps[(int)Opcode.SUB_AT_HL] = SUB(Register.HL, 8);
            StdOps[(int)Opcode.SUB_A] = SUB(Register.A, 4);
            StdOps[(int)Opcode.SBC_A_B] = SBC(Register.B, 4);
            StdOps[(int)Opcode.SBC_A_C] = SBC(Register.C, 4);
            StdOps[(int)Opcode.SBC_A_D] = SBC(Register.D, 4);
            StdOps[(int)Opcode.SBC_A_E] = SBC(Register.E, 4);
            StdOps[(int)Opcode.SBC_A_H] = SBC(Register.H, 4);
            StdOps[(int)Opcode.SBC_A_L] = SBC(Register.L, 4);
            StdOps[(int)Opcode.SBC_A_AT_HL] = SBC(Register.HL, 8);
            StdOps[(int)Opcode.SBC_A_A] = SBC(Register.A, 4);
            StdOps[(int)Opcode.AND_B] = AND(Register.B, 4);
            StdOps[(int)Opcode.AND_C] = AND(Register.C, 4);
            StdOps[(int)Opcode.AND_D] = AND(Register.D, 4);
            StdOps[(int)Opcode.AND_E] = AND(Register.E, 4);
            StdOps[(int)Opcode.AND_H] = AND(Register.H, 4);
            StdOps[(int)Opcode.AND_L] = AND(Register.L, 4);
            StdOps[(int)Opcode.AND_AT_HL] = AND(Register.HL, 8);
            StdOps[(int)Opcode.AND_A] = AND(Register.A, 4);
            StdOps[(int)Opcode.XOR_B] = XOR(Register.B, 4);
            StdOps[(int)Opcode.XOR_C] = XOR(Register.C, 4);
            StdOps[(int)Opcode.XOR_D] = XOR(Register.D, 4);
            StdOps[(int)Opcode.XOR_E] = XOR(Register.E, 4);
            StdOps[(int)Opcode.XOR_H] = XOR(Register.H, 4);
            StdOps[(int)Opcode.XOR_L] = XOR(Register.L, 4);
            StdOps[(int)Opcode.XOR_AT_HL] = XOR(Register.HL, 8);
            StdOps[(int)Opcode.XOR_A] = XOR(Register.A, 4);
            StdOps[(int)Opcode.OR_B] = OR(Register.B, 4);
            StdOps[(int)Opcode.OR_C] = OR(Register.C, 4);
            StdOps[(int)Opcode.OR_D] = OR(Register.D, 4);
            StdOps[(int)Opcode.OR_E] = OR(Register.E, 4);
            StdOps[(int)Opcode.OR_H] = OR(Register.H, 4);
            StdOps[(int)Opcode.OR_L] = OR(Register.L, 4);
            StdOps[(int)Opcode.OR_AT_HL] = OR(Register.HL, 8);
            StdOps[(int)Opcode.OR_A] = OR(Register.A, 4);
            StdOps[(int)Opcode.CP_B] = CP(Register.B, 4);
            StdOps[(int)Opcode.CP_C] = CP(Register.C, 4);
            StdOps[(int)Opcode.CP_D] = CP(Register.D, 4);
            StdOps[(int)Opcode.CP_E] = CP(Register.E, 4);
            StdOps[(int)Opcode.CP_H] = CP(Register.H, 4);
            StdOps[(int)Opcode.CP_L] = CP(Register.L, 4);
            StdOps[(int)Opcode.CP_AT_HL] = CP(Register.HL, 8);
            StdOps[(int)Opcode.CP_A] = CP(Register.A, 4);
            StdOps[(int)Opcode.RET_NZ] = RET(Flag.NZ, 20, 8);
            StdOps[(int)Opcode.POP_BC] = POP(WideRegister.BC, 12);
            StdOps[(int)Opcode.JP_NZ_a16] = JP_A16(Flag.NZ, 16, 12);
            StdOps[(int)Opcode.JP_a16] = JP_A16(16);
            StdOps[(int)Opcode.CALL_NZ_a16] = CALL_A16(Flag.NZ, 24, 12);
            StdOps[(int)Opcode.PUSH_BC] = PUSH(WideRegister.BC, 16);
            StdOps[(int)Opcode.ADD_A_d8] = ADD_A_d8(8);
            StdOps[(int)Opcode.RST_00H] = RST(0x00, 16);
            StdOps[(int)Opcode.RET_Zero] = RET(Flag.Z, 20, 8);
            StdOps[(int)Opcode.RET] = RET(16);
            StdOps[(int)Opcode.JP_Zero_a16] = JP_A16(Flag.Z, 16, 12);
            StdOps[(int)Opcode.PREFIX] = PREFIX(4);
            StdOps[(int)Opcode.CALL_Zero_a16] = CALL_A16(Flag.Z, 24, 12);
            StdOps[(int)Opcode.CALL_a16] = CALL_a16(24);
            StdOps[(int)Opcode.ADC_A_d8] = ADC(8);
            StdOps[(int)Opcode.RST_08H] = RST(0x08, 16);
            StdOps[(int)Opcode.RET_NC] = RET(Flag.NC, 20, 8);
            StdOps[(int)Opcode.POP_DE] = POP(WideRegister.DE, 12);
            StdOps[(int)Opcode.JP_NC_a16] = JP_A16(Flag.NC, 16, 12);
            StdOps[(int)Opcode.ILLEGAL_D3] = ILLEGAL_D3(4);
            StdOps[(int)Opcode.CALL_NC_a16] = CALL_A16(Flag.NC, 24, 12);
            StdOps[(int)Opcode.PUSH_DE] = PUSH(WideRegister.DE, 16);
            StdOps[(int)Opcode.SUB_d8] = SUB(8);
            StdOps[(int)Opcode.RST_10H] = RST(0x10, 16);
            StdOps[(int)Opcode.RET_Carry] = RET(Flag.C, 20, 8);
            StdOps[(int)Opcode.RETI] = RETI(16);
            StdOps[(int)Opcode.JP_Carry_a16] = JP_A16(Flag.C, 16, 12);
            StdOps[(int)Opcode.ILLEGAL_DB] = ILLEGAL_DB(4);
            StdOps[(int)Opcode.CALL_Carry_a16] = CALL_A16(Flag.C, 24, 12);
            StdOps[(int)Opcode.ILLEGAL_DD] = ILLEGAL_DD(4);
            StdOps[(int)Opcode.SBC_A_d8] = SBC(8);
            StdOps[(int)Opcode.RST_18H] = RST(0x18, 16);
            StdOps[(int)Opcode.LDH_AT_a8_A] = LDH(12);
            StdOps[(int)Opcode.POP_HL] = POP(WideRegister.HL, 12);
            StdOps[(int)Opcode.LD_AT_C_A] = LD_AT_C_A(8);
            StdOps[(int)Opcode.ILLEGAL_E3] = ILLEGAL_E3(4);
            StdOps[(int)Opcode.ILLEGAL_E4] = ILLEGAL_E4(4);
            StdOps[(int)Opcode.PUSH_HL] = PUSH(WideRegister.HL, 16);
            StdOps[(int)Opcode.AND_d8] = AND(8);
            StdOps[(int)Opcode.RST_20H] = RST(0x20, 16);
            StdOps[(int)Opcode.ADD_SP_r8] = ADD_SP_R8(16);
            StdOps[(int)Opcode.JP_HL] = JP(4);
            StdOps[(int)Opcode.LD_AT_a16_A] = LD_AT_a16_A(16);
            StdOps[(int)Opcode.ILLEGAL_EB] = ILLEGAL_EB(4);
            StdOps[(int)Opcode.ILLEGAL_EC] = ILLEGAL_EC(4);
            StdOps[(int)Opcode.ILLEGAL_ED] = ILLEGAL_ED(4);
            StdOps[(int)Opcode.XOR_d8] = XOR(8);
            StdOps[(int)Opcode.RST_28H] = RST(0x28, 16);
            StdOps[(int)Opcode.LDH_A_AT_a8] = LDH_A_AT_a8(12);
            StdOps[(int)Opcode.POP_AF] = POP(WideRegister.AF, 12);
            StdOps[(int)Opcode.LD_A_AT_C] = LD_A_AT_C(8);
            StdOps[(int)Opcode.DI] = DI(4);
            StdOps[(int)Opcode.ILLEGAL_F4] = ILLEGAL_F4(4);
            StdOps[(int)Opcode.PUSH_AF] = PUSH(WideRegister.AF, 16);
            StdOps[(int)Opcode.OR_d8] = OR(8);
            StdOps[(int)Opcode.RST_30H] = RST(0x30, 16);
            StdOps[(int)Opcode.LD_HL_SP_i8] = LD_HL_SP_i8(12);
            StdOps[(int)Opcode.LD_SP_HL] = LD_SP_HL(8);
            StdOps[(int)Opcode.LD_A_AT_a16] = LD_A16(16);
            StdOps[(int)Opcode.EI] = EI(4);
            StdOps[(int)Opcode.ILLEGAL_FC] = ILLEGAL_FC(4);
            StdOps[(int)Opcode.ILLEGAL_FD] = ILLEGAL_FD(4);
            StdOps[(int)Opcode.CP_d8] = CP(8);
            StdOps[(int)Opcode.RST_38H] = RST(0x38, 16);

            CbOps = new Action[0x100];
            CbOps[(int)CBOpcode.RLC_B] = RLC(Register.B, 8);
            CbOps[(int)CBOpcode.RLC_C] = RLC(Register.C, 8);
            CbOps[(int)CBOpcode.RLC_D] = RLC(Register.D, 8);
            CbOps[(int)CBOpcode.RLC_E] = RLC(Register.E, 8);
            CbOps[(int)CBOpcode.RLC_H] = RLC(Register.H, 8);
            CbOps[(int)CBOpcode.RLC_L] = RLC(Register.L, 8);
            CbOps[(int)CBOpcode.RLC_AT_HL] = RLC(Register.HL, 16);
            CbOps[(int)CBOpcode.RLC_A] = RLC(Register.A, 8);
            CbOps[(int)CBOpcode.RRC_B] = RRC(Register.B, 8);
            CbOps[(int)CBOpcode.RRC_C] = RRC(Register.C, 8);
            CbOps[(int)CBOpcode.RRC_D] = RRC(Register.D, 8);
            CbOps[(int)CBOpcode.RRC_E] = RRC(Register.E, 8);
            CbOps[(int)CBOpcode.RRC_H] = RRC(Register.H, 8);
            CbOps[(int)CBOpcode.RRC_L] = RRC(Register.L, 8);
            CbOps[(int)CBOpcode.RRC_AT_HL] = RRC(Register.HL, 16);
            CbOps[(int)CBOpcode.RRC_A] = RRC(Register.A, 8);
            CbOps[(int)CBOpcode.RL_B] = RL(Register.B, 8);
            CbOps[(int)CBOpcode.RL_C] = RL(Register.C, 8);
            CbOps[(int)CBOpcode.RL_D] = RL(Register.D, 8);
            CbOps[(int)CBOpcode.RL_E] = RL(Register.E, 8);
            CbOps[(int)CBOpcode.RL_H] = RL(Register.H, 8);
            CbOps[(int)CBOpcode.RL_L] = RL(Register.L, 8);
            CbOps[(int)CBOpcode.RL_AT_HL] = RL(Register.HL, 16);
            CbOps[(int)CBOpcode.RL_A] = RL(Register.A, 8);
            CbOps[(int)CBOpcode.RR_B] = RR(Register.B, 8);
            CbOps[(int)CBOpcode.RR_C] = RR(Register.C, 8);
            CbOps[(int)CBOpcode.RR_D] = RR(Register.D, 8);
            CbOps[(int)CBOpcode.RR_E] = RR(Register.E, 8);
            CbOps[(int)CBOpcode.RR_H] = RR(Register.H, 8);
            CbOps[(int)CBOpcode.RR_L] = RR(Register.L, 8);
            CbOps[(int)CBOpcode.RR_AT_HL] = RR(Register.HL, 16);
            CbOps[(int)CBOpcode.RR_A] = RR(Register.A, 8);
            CbOps[(int)CBOpcode.SLA_B] = SLA(Register.B, 8);
            CbOps[(int)CBOpcode.SLA_C] = SLA(Register.C, 8);
            CbOps[(int)CBOpcode.SLA_D] = SLA(Register.D, 8);
            CbOps[(int)CBOpcode.SLA_E] = SLA(Register.E, 8);
            CbOps[(int)CBOpcode.SLA_H] = SLA(Register.H, 8);
            CbOps[(int)CBOpcode.SLA_L] = SLA(Register.L, 8);
            CbOps[(int)CBOpcode.SLA_AT_HL] = SLA(Register.HL, 16);
            CbOps[(int)CBOpcode.SLA_A] = SLA(Register.A, 8);
            CbOps[(int)CBOpcode.SRA_B] = SRA(Register.B, 8);
            CbOps[(int)CBOpcode.SRA_C] = SRA(Register.C, 8);
            CbOps[(int)CBOpcode.SRA_D] = SRA(Register.D, 8);
            CbOps[(int)CBOpcode.SRA_E] = SRA(Register.E, 8);
            CbOps[(int)CBOpcode.SRA_H] = SRA(Register.H, 8);
            CbOps[(int)CBOpcode.SRA_L] = SRA(Register.L, 8);
            CbOps[(int)CBOpcode.SRA_AT_HL] = SRA(Register.HL, 16);
            CbOps[(int)CBOpcode.SRA_A] = SRA(Register.A, 8);
            CbOps[(int)CBOpcode.SWAP_B] = SWAP(Register.B, 8);
            CbOps[(int)CBOpcode.SWAP_C] = SWAP(Register.C, 8);
            CbOps[(int)CBOpcode.SWAP_D] = SWAP(Register.D, 8);
            CbOps[(int)CBOpcode.SWAP_E] = SWAP(Register.E, 8);
            CbOps[(int)CBOpcode.SWAP_H] = SWAP(Register.H, 8);
            CbOps[(int)CBOpcode.SWAP_L] = SWAP(Register.L, 8);
            CbOps[(int)CBOpcode.SWAP_AT_HL] = SWAP(Register.HL, 16);
            CbOps[(int)CBOpcode.SWAP_A] = SWAP(Register.A, 8);
            CbOps[(int)CBOpcode.SRL_B] = SRL(Register.B, 8);
            CbOps[(int)CBOpcode.SRL_C] = SRL(Register.C, 8);
            CbOps[(int)CBOpcode.SRL_D] = SRL(Register.D, 8);
            CbOps[(int)CBOpcode.SRL_E] = SRL(Register.E, 8);
            CbOps[(int)CBOpcode.SRL_H] = SRL(Register.H, 8);
            CbOps[(int)CBOpcode.SRL_L] = SRL(Register.L, 8);
            CbOps[(int)CBOpcode.SRL_AT_HL] = SRL(Register.HL, 16);
            CbOps[(int)CBOpcode.SRL_A] = SRL(Register.A, 8);
            CbOps[(int)CBOpcode.BIT_0_B] = BIT(0, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_0_C] = BIT(0, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_0_D] = BIT(0, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_0_E] = BIT(0, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_0_H] = BIT(0, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_0_L] = BIT(0, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_0_AT_HL] = BIT(0, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_0_A] = BIT(0, Register.A, 8);
            CbOps[(int)CBOpcode.BIT_1_B] = BIT(1, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_1_C] = BIT(1, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_1_D] = BIT(1, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_1_E] = BIT(1, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_1_H] = BIT(1, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_1_L] = BIT(1, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_1_AT_HL] = BIT(1, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_1_A] = BIT(1, Register.A, 8);
            CbOps[(int)CBOpcode.BIT_2_B] = BIT(2, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_2_C] = BIT(2, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_2_D] = BIT(2, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_2_E] = BIT(2, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_2_H] = BIT(2, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_2_L] = BIT(2, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_2_AT_HL] = BIT(2, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_2_A] = BIT(2, Register.A, 8);
            CbOps[(int)CBOpcode.BIT_3_B] = BIT(3, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_3_C] = BIT(3, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_3_D] = BIT(3, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_3_E] = BIT(3, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_3_H] = BIT(3, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_3_L] = BIT(3, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_3_AT_HL] = BIT(3, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_3_A] = BIT(3, Register.A, 8);
            CbOps[(int)CBOpcode.BIT_4_B] = BIT(4, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_4_C] = BIT(4, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_4_D] = BIT(4, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_4_E] = BIT(4, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_4_H] = BIT(4, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_4_L] = BIT(4, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_4_AT_HL] = BIT(4, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_4_A] = BIT(4, Register.A, 8);
            CbOps[(int)CBOpcode.BIT_5_B] = BIT(5, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_5_C] = BIT(5, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_5_D] = BIT(5, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_5_E] = BIT(5, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_5_H] = BIT(5, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_5_L] = BIT(5, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_5_AT_HL] = BIT(5, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_5_A] = BIT(5, Register.A, 8);
            CbOps[(int)CBOpcode.BIT_6_B] = BIT(6, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_6_C] = BIT(6, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_6_D] = BIT(6, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_6_E] = BIT(6, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_6_H] = BIT(6, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_6_L] = BIT(6, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_6_AT_HL] = BIT(6, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_6_A] = BIT(6, Register.A, 8);
            CbOps[(int)CBOpcode.BIT_7_B] = BIT(7, Register.B, 8);
            CbOps[(int)CBOpcode.BIT_7_C] = BIT(7, Register.C, 8);
            CbOps[(int)CBOpcode.BIT_7_D] = BIT(7, Register.D, 8);
            CbOps[(int)CBOpcode.BIT_7_E] = BIT(7, Register.E, 8);
            CbOps[(int)CBOpcode.BIT_7_H] = BIT(7, Register.H, 8);
            CbOps[(int)CBOpcode.BIT_7_L] = BIT(7, Register.L, 8);
            CbOps[(int)CBOpcode.BIT_7_AT_HL] = BIT(7, Register.HL, 12);
            CbOps[(int)CBOpcode.BIT_7_A] = BIT(7, Register.A, 8);
            CbOps[(int)CBOpcode.RES_0_B] = RES(0, Register.B, 8);
            CbOps[(int)CBOpcode.RES_0_C] = RES(0, Register.C, 8);
            CbOps[(int)CBOpcode.RES_0_D] = RES(0, Register.D, 8);
            CbOps[(int)CBOpcode.RES_0_E] = RES(0, Register.E, 8);
            CbOps[(int)CBOpcode.RES_0_H] = RES(0, Register.H, 8);
            CbOps[(int)CBOpcode.RES_0_L] = RES(0, Register.L, 8);
            CbOps[(int)CBOpcode.RES_0_AT_HL] = RES(0, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_0_A] = RES(0, Register.A, 8);
            CbOps[(int)CBOpcode.RES_1_B] = RES(1, Register.B, 8);
            CbOps[(int)CBOpcode.RES_1_C] = RES(1, Register.C, 8);
            CbOps[(int)CBOpcode.RES_1_D] = RES(1, Register.D, 8);
            CbOps[(int)CBOpcode.RES_1_E] = RES(1, Register.E, 8);
            CbOps[(int)CBOpcode.RES_1_H] = RES(1, Register.H, 8);
            CbOps[(int)CBOpcode.RES_1_L] = RES(1, Register.L, 8);
            CbOps[(int)CBOpcode.RES_1_AT_HL] = RES(1, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_1_A] = RES(1, Register.A, 8);
            CbOps[(int)CBOpcode.RES_2_B] = RES(2, Register.B, 8);
            CbOps[(int)CBOpcode.RES_2_C] = RES(2, Register.C, 8);
            CbOps[(int)CBOpcode.RES_2_D] = RES(2, Register.D, 8);
            CbOps[(int)CBOpcode.RES_2_E] = RES(2, Register.E, 8);
            CbOps[(int)CBOpcode.RES_2_H] = RES(2, Register.H, 8);
            CbOps[(int)CBOpcode.RES_2_L] = RES(2, Register.L, 8);
            CbOps[(int)CBOpcode.RES_2_AT_HL] = RES(2, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_2_A] = RES(2, Register.A, 8);
            CbOps[(int)CBOpcode.RES_3_B] = RES(3, Register.B, 8);
            CbOps[(int)CBOpcode.RES_3_C] = RES(3, Register.C, 8);
            CbOps[(int)CBOpcode.RES_3_D] = RES(3, Register.D, 8);
            CbOps[(int)CBOpcode.RES_3_E] = RES(3, Register.E, 8);
            CbOps[(int)CBOpcode.RES_3_H] = RES(3, Register.H, 8);
            CbOps[(int)CBOpcode.RES_3_L] = RES(3, Register.L, 8);
            CbOps[(int)CBOpcode.RES_3_AT_HL] = RES(3, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_3_A] = RES(3, Register.A, 8);
            CbOps[(int)CBOpcode.RES_4_B] = RES(4, Register.B, 8);
            CbOps[(int)CBOpcode.RES_4_C] = RES(4, Register.C, 8);
            CbOps[(int)CBOpcode.RES_4_D] = RES(4, Register.D, 8);
            CbOps[(int)CBOpcode.RES_4_E] = RES(4, Register.E, 8);
            CbOps[(int)CBOpcode.RES_4_H] = RES(4, Register.H, 8);
            CbOps[(int)CBOpcode.RES_4_L] = RES(4, Register.L, 8);
            CbOps[(int)CBOpcode.RES_4_AT_HL] = RES(4, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_4_A] = RES(4, Register.A, 8);
            CbOps[(int)CBOpcode.RES_5_B] = RES(5, Register.B, 8);
            CbOps[(int)CBOpcode.RES_5_C] = RES(5, Register.C, 8);
            CbOps[(int)CBOpcode.RES_5_D] = RES(5, Register.D, 8);
            CbOps[(int)CBOpcode.RES_5_E] = RES(5, Register.E, 8);
            CbOps[(int)CBOpcode.RES_5_H] = RES(5, Register.H, 8);
            CbOps[(int)CBOpcode.RES_5_L] = RES(5, Register.L, 8);
            CbOps[(int)CBOpcode.RES_5_AT_HL] = RES(5, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_5_A] = RES(5, Register.A, 8);
            CbOps[(int)CBOpcode.RES_6_B] = RES(6, Register.B, 8);
            CbOps[(int)CBOpcode.RES_6_C] = RES(6, Register.C, 8);
            CbOps[(int)CBOpcode.RES_6_D] = RES(6, Register.D, 8);
            CbOps[(int)CBOpcode.RES_6_E] = RES(6, Register.E, 8);
            CbOps[(int)CBOpcode.RES_6_H] = RES(6, Register.H, 8);
            CbOps[(int)CBOpcode.RES_6_L] = RES(6, Register.L, 8);
            CbOps[(int)CBOpcode.RES_6_AT_HL] = RES(6, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_6_A] = RES(6, Register.A, 8);
            CbOps[(int)CBOpcode.RES_7_B] = RES(7, Register.B, 8);
            CbOps[(int)CBOpcode.RES_7_C] = RES(7, Register.C, 8);
            CbOps[(int)CBOpcode.RES_7_D] = RES(7, Register.D, 8);
            CbOps[(int)CBOpcode.RES_7_E] = RES(7, Register.E, 8);
            CbOps[(int)CBOpcode.RES_7_H] = RES(7, Register.H, 8);
            CbOps[(int)CBOpcode.RES_7_L] = RES(7, Register.L, 8);
            CbOps[(int)CBOpcode.RES_7_AT_HL] = RES(7, Register.HL, 16);
            CbOps[(int)CBOpcode.RES_7_A] = RES(7, Register.A, 8);
            CbOps[(int)CBOpcode.SET_0_B] = SET(0, Register.B, 8);
            CbOps[(int)CBOpcode.SET_0_C] = SET(0, Register.C, 8);
            CbOps[(int)CBOpcode.SET_0_D] = SET(0, Register.D, 8);
            CbOps[(int)CBOpcode.SET_0_E] = SET(0, Register.E, 8);
            CbOps[(int)CBOpcode.SET_0_H] = SET(0, Register.H, 8);
            CbOps[(int)CBOpcode.SET_0_L] = SET(0, Register.L, 8);
            CbOps[(int)CBOpcode.SET_0_AT_HL] = SET(0, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_0_A] = SET(0, Register.A, 8);
            CbOps[(int)CBOpcode.SET_1_B] = SET(1, Register.B, 8);
            CbOps[(int)CBOpcode.SET_1_C] = SET(1, Register.C, 8);
            CbOps[(int)CBOpcode.SET_1_D] = SET(1, Register.D, 8);
            CbOps[(int)CBOpcode.SET_1_E] = SET(1, Register.E, 8);
            CbOps[(int)CBOpcode.SET_1_H] = SET(1, Register.H, 8);
            CbOps[(int)CBOpcode.SET_1_L] = SET(1, Register.L, 8);
            CbOps[(int)CBOpcode.SET_1_AT_HL] = SET(1, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_1_A] = SET(1, Register.A, 8);
            CbOps[(int)CBOpcode.SET_2_B] = SET(2, Register.B, 8);
            CbOps[(int)CBOpcode.SET_2_C] = SET(2, Register.C, 8);
            CbOps[(int)CBOpcode.SET_2_D] = SET(2, Register.D, 8);
            CbOps[(int)CBOpcode.SET_2_E] = SET(2, Register.E, 8);
            CbOps[(int)CBOpcode.SET_2_H] = SET(2, Register.H, 8);
            CbOps[(int)CBOpcode.SET_2_L] = SET(2, Register.L, 8);
            CbOps[(int)CBOpcode.SET_2_AT_HL] = SET(2, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_2_A] = SET(2, Register.A, 8);
            CbOps[(int)CBOpcode.SET_3_B] = SET(3, Register.B, 8);
            CbOps[(int)CBOpcode.SET_3_C] = SET(3, Register.C, 8);
            CbOps[(int)CBOpcode.SET_3_D] = SET(3, Register.D, 8);
            CbOps[(int)CBOpcode.SET_3_E] = SET(3, Register.E, 8);
            CbOps[(int)CBOpcode.SET_3_H] = SET(3, Register.H, 8);
            CbOps[(int)CBOpcode.SET_3_L] = SET(3, Register.L, 8);
            CbOps[(int)CBOpcode.SET_3_AT_HL] = SET(3, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_3_A] = SET(3, Register.A, 8);
            CbOps[(int)CBOpcode.SET_4_B] = SET(4, Register.B, 8);
            CbOps[(int)CBOpcode.SET_4_C] = SET(4, Register.C, 8);
            CbOps[(int)CBOpcode.SET_4_D] = SET(4, Register.D, 8);
            CbOps[(int)CBOpcode.SET_4_E] = SET(4, Register.E, 8);
            CbOps[(int)CBOpcode.SET_4_H] = SET(4, Register.H, 8);
            CbOps[(int)CBOpcode.SET_4_L] = SET(4, Register.L, 8);
            CbOps[(int)CBOpcode.SET_4_AT_HL] = SET(4, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_4_A] = SET(4, Register.A, 8);
            CbOps[(int)CBOpcode.SET_5_B] = SET(5, Register.B, 8);
            CbOps[(int)CBOpcode.SET_5_C] = SET(5, Register.C, 8);
            CbOps[(int)CBOpcode.SET_5_D] = SET(5, Register.D, 8);
            CbOps[(int)CBOpcode.SET_5_E] = SET(5, Register.E, 8);
            CbOps[(int)CBOpcode.SET_5_H] = SET(5, Register.H, 8);
            CbOps[(int)CBOpcode.SET_5_L] = SET(5, Register.L, 8);
            CbOps[(int)CBOpcode.SET_5_AT_HL] = SET(5, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_5_A] = SET(5, Register.A, 8);
            CbOps[(int)CBOpcode.SET_6_B] = SET(6, Register.B, 8);
            CbOps[(int)CBOpcode.SET_6_C] = SET(6, Register.C, 8);
            CbOps[(int)CBOpcode.SET_6_D] = SET(6, Register.D, 8);
            CbOps[(int)CBOpcode.SET_6_E] = SET(6, Register.E, 8);
            CbOps[(int)CBOpcode.SET_6_H] = SET(6, Register.H, 8);
            CbOps[(int)CBOpcode.SET_6_L] = SET(6, Register.L, 8);
            CbOps[(int)CBOpcode.SET_6_AT_HL] = SET(6, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_6_A] = SET(6, Register.A, 8);
            CbOps[(int)CBOpcode.SET_7_B] = SET(7, Register.B, 8);
            CbOps[(int)CBOpcode.SET_7_C] = SET(7, Register.C, 8);
            CbOps[(int)CBOpcode.SET_7_D] = SET(7, Register.D, 8);
            CbOps[(int)CBOpcode.SET_7_E] = SET(7, Register.E, 8);
            CbOps[(int)CBOpcode.SET_7_H] = SET(7, Register.H, 8);
            CbOps[(int)CBOpcode.SET_7_L] = SET(7, Register.L, 8);
            CbOps[(int)CBOpcode.SET_7_AT_HL] = SET(7, Register.HL, 16);
            CbOps[(int)CBOpcode.SET_7_A] = SET(7, Register.A, 8);

            Registers = new Registers();
        }

        private byte ReadHaltBug()
        {
            Halted = HaltState.off;
            return Memory[Pc.Value];
        }
        internal void DoNextOP()
        {
            if (Halted != HaltState.off)
            {
                if (Halted != HaltState.haltbug)
                {
                    TicksWeAreWaitingFor += 4;
                    return;
                }
            }

            var op = Halted == HaltState.haltbug ? ReadHaltBug() : Memory[Pc.Value++];
            if (op != 0xcb)
            {
                Op((Opcode)op)();
            }
            else
            {
                var CBop = Memory[Pc.Value++]; //Because of the CB prefix we encountered in the previous case we already skipped the extra byte of a cb instruction here
                Op((CBOpcode)CBop)();
            }
        }

        private bool didInterrupt;
        internal void Tick()
        {
            if (TicksWeAreWaitingFor == 0)
            {
                didInterrupt = DoInterrupt();
                if (ISR.InterruptEnableScheduled)
                {
                    ISR.IME = true;
                    ISR.InterruptEnableScheduled = false;
                }
                if (!didInterrupt)
                    DoNextOP();
                didInterrupt = false;
            }
            TicksWeAreWaitingFor--;
        }
    }
}