﻿using System;

namespace emulator
{
    public partial class CPU
    {
        private readonly Action[] StdOps;
        private readonly Action[] CbOps;

        public bool IME;
        private HaltState Halted = HaltState.off;

        private bool InterruptEnableScheduled;
        private readonly Func<bool> GetKeyboardInterrupt;
        public Action Op(Unprefixed op) => StdOps[(int)op];

        public Action Op(Cbprefixed op) => CbOps[(int)op];

        public CPU(Func<bool> getKeyboardInterrupt, MMU memory)
        {
            GetKeyboardInterrupt = getKeyboardInterrupt;
            Memory = memory;

            StdOps = new Action[0x100];
            StdOps[(int)Unprefixed.NOP] = NOP(4);
            StdOps[(int)Unprefixed.LD_AT_DE_A] = LD((WideRegister.DE, new Traits(false, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_AT_BC_A] = LD((WideRegister.BC, new Traits(false, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.INC_BC] = INC(WideRegister.BC, 8);
            StdOps[(int)Unprefixed.INC_B] = INC(Register.B, 4);
            StdOps[(int)Unprefixed.DEC_B] = DEC(Register.B, 4);
            StdOps[(int)Unprefixed.LD_B_d8] = LD(Register.B, DMGInteger.d8, 8);
            StdOps[(int)Unprefixed.RLCA] = RLCA(4);
            StdOps[(int)Unprefixed.LD_AT_a16_SP] = WriteSPToMem(20);
            StdOps[(int)Unprefixed.ADD_HL_BC] = ADD(WideRegister.BC, 8);
            StdOps[(int)Unprefixed.LD_A_AT_BC] = LD(Register.A, (WideRegister.BC, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.DEC_BC] = DEC(WideRegister.BC, 8);
            StdOps[(int)Unprefixed.INC_C] = INC(Register.C, 4);
            StdOps[(int)Unprefixed.DEC_C] = DEC(Register.C, 4);
            StdOps[(int)Unprefixed.LD_C_d8] = LD(Register.C, DMGInteger.d8, 8);
            StdOps[(int)Unprefixed.RRCA] = RRCA(4);
            StdOps[(int)Unprefixed.STOP] = STOP(4);
            StdOps[(int)Unprefixed.LD_DE_d16] = LD_D16(WideRegister.DE, 12);
            StdOps[(int)Unprefixed.LD_BC_d16] = LD_D16(WideRegister.BC, 12);
            StdOps[(int)Unprefixed.INC_DE] = INC(WideRegister.DE, 8);
            StdOps[(int)Unprefixed.INC_D] = INC(Register.D, 4);
            StdOps[(int)Unprefixed.DEC_D] = DEC(Register.D, 4);
            StdOps[(int)Unprefixed.LD_D_d8] = LD(Register.D, DMGInteger.d8, 8);
            StdOps[(int)Unprefixed.RLA] = RLA(4);
            StdOps[(int)Unprefixed.JR_r8] = JR(12);
            StdOps[(int)Unprefixed.ADD_HL_DE] = ADD(WideRegister.DE, 8);
            StdOps[(int)Unprefixed.LD_A_AT_DE] = LD(Register.A, (WideRegister.DE, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.DEC_DE] = DEC(WideRegister.DE, 8);
            StdOps[(int)Unprefixed.INC_E] = INC(Register.E, 4);
            StdOps[(int)Unprefixed.DEC_E] = DEC(Register.E, 4);
            StdOps[(int)Unprefixed.LD_E_d8] = LD(Register.E, DMGInteger.d8, 8);
            StdOps[(int)Unprefixed.RRA] = RRA(4);
            StdOps[(int)Unprefixed.JR_NZ_r8] = JR(Flag.NZ, 12, 8);
            StdOps[(int)Unprefixed.LD_HL_d16] = LD_D16(WideRegister.HL, 12);
            StdOps[(int)Unprefixed.LDI_AT_HL_A] = LD((WideRegister.HL, new Traits(false, Postfix.increment)), (Register.A, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.INC_HL] = INC(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.INC_H] = INC(Register.H, 4);
            StdOps[(int)Unprefixed.DEC_H] = DEC(Register.H, 4);
            StdOps[(int)Unprefixed.LD_H_d8] = LD(Register.H, DMGInteger.d8, 8);
            StdOps[(int)Unprefixed.DAA] = DAA(4);
            StdOps[(int)Unprefixed.JR_Zero_r8] = JR(Flag.Z, 12, 8);
            StdOps[(int)Unprefixed.ADD_HL_HL] = ADD(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.LD_AI_AT_HL] = LD(Register.A, (WideRegister.HL, new Traits(false, Postfix.increment)), 8);
            StdOps[(int)Unprefixed.DEC_HL] = DEC(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.INC_L] = INC(Register.L, 4);
            StdOps[(int)Unprefixed.DEC_L] = DEC(Register.L, 4);
            StdOps[(int)Unprefixed.LD_L_d8] = LD(Register.L, DMGInteger.d8, 8);
            StdOps[(int)Unprefixed.CPL] = CPL(4);
            StdOps[(int)Unprefixed.JR_NC_r8] = JR(Flag.NC, 12, 8);
            StdOps[(int)Unprefixed.LD_SP_d16] = LD_D16(WideRegister.SP, 12);
            StdOps[(int)Unprefixed.LDD_AT_HL_A] = LD((WideRegister.HL, new Traits(false, Postfix.decrement)), (Register.A, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.INC_SP] = INC(WideRegister.SP, 8);
            StdOps[(int)Unprefixed.INC_AT_HL] = INC(Register.HL, 12);
            StdOps[(int)Unprefixed.DEC_AT_HL] = DEC(Register.HL, 12);
            StdOps[(int)Unprefixed.LD_AT_HL_d8] = LD(Register.HL, DMGInteger.d8, 12);
            StdOps[(int)Unprefixed.SCF] = SCF(4);
            StdOps[(int)Unprefixed.JR_Carry_r8] = JR(Flag.C, 12, 8);
            StdOps[(int)Unprefixed.ADD_HL_SP] = ADD(WideRegister.SP, 8);
            StdOps[(int)Unprefixed.LD_AD_AT_HL] = LD(Register.A, (WideRegister.HL, new Traits(false, Postfix.decrement)), 8);
            StdOps[(int)Unprefixed.DEC_SP] = DEC(WideRegister.SP, 8);
            StdOps[(int)Unprefixed.INC_A] = INC(Register.A, 4);
            StdOps[(int)Unprefixed.DEC_A] = DEC(Register.A, 4);
            StdOps[(int)Unprefixed.LD_A_d8] = LD(Register.A, DMGInteger.d8, 8);
            StdOps[(int)Unprefixed.CCF] = CCF(4);
            StdOps[(int)Unprefixed.LD_B_B] = LD((Register.B, new Traits(true, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_B_C] = LD((Register.B, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_B_D] = LD((Register.B, new Traits(true, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_B_E] = LD((Register.B, new Traits(true, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_B_H] = LD((Register.B, new Traits(true, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_B_L] = LD((Register.B, new Traits(true, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_B_AT_HL] = LD(Register.B, (WideRegister.HL, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_B_A] = LD((Register.B, new Traits(true, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_C_B] = LD((Register.C, new Traits(true, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_C_C] = LD((Register.C, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_C_D] = LD((Register.C, new Traits(true, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_C_E] = LD((Register.C, new Traits(true, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_C_H] = LD((Register.C, new Traits(true, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_C_L] = LD((Register.C, new Traits(true, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_C_AT_HL] = LD(Register.C, (WideRegister.HL, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_C_A] = LD((Register.C, new Traits(true, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_D_B] = LD((Register.D, new Traits(true, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_D_C] = LD((Register.D, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_D_D] = LD((Register.D, new Traits(true, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_D_E] = LD((Register.D, new Traits(true, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_D_H] = LD((Register.D, new Traits(true, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_D_L] = LD((Register.D, new Traits(true, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_D_AT_HL] = LD(Register.D, (WideRegister.HL, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_D_A] = LD((Register.D, new Traits(true, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_E_B] = LD((Register.E, new Traits(true, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_E_C] = LD((Register.E, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_E_D] = LD((Register.E, new Traits(true, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_E_E] = LD((Register.E, new Traits(true, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_E_H] = LD((Register.E, new Traits(true, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_E_L] = LD((Register.E, new Traits(true, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_E_AT_HL] = LD(Register.E, (WideRegister.HL, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_E_A] = LD((Register.E, new Traits(true, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_H_B] = LD((Register.H, new Traits(true, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_H_C] = LD((Register.H, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_H_D] = LD((Register.H, new Traits(true, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_H_E] = LD((Register.H, new Traits(true, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_H_H] = LD((Register.H, new Traits(true, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_H_L] = LD((Register.H, new Traits(true, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_H_AT_HL] = LD(Register.H, (WideRegister.HL, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_H_A] = LD((Register.H, new Traits(true, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_L_B] = LD((Register.L, new Traits(true, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_L_C] = LD((Register.L, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_L_D] = LD((Register.L, new Traits(true, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_L_E] = LD((Register.L, new Traits(true, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_L_H] = LD((Register.L, new Traits(true, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_L_L] = LD((Register.L, new Traits(true, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_L_AT_HL] = LD(Register.L, (WideRegister.HL, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_L_A] = LD((Register.L, new Traits(true, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_AT_HL_B] = LD((WideRegister.HL, new Traits(false, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_AT_HL_C] = LD((WideRegister.HL, new Traits(false, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_AT_HL_D] = LD((WideRegister.HL, new Traits(false, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_AT_HL_E] = LD((WideRegister.HL, new Traits(false, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_AT_HL_H] = LD((WideRegister.HL, new Traits(false, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_AT_HL_L] = LD((WideRegister.HL, new Traits(false, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.HALT] = HALT(4);
            StdOps[(int)Unprefixed.LD_AT_HL_A] = LD((WideRegister.HL, new Traits(false, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_A_B] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.B, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_A_C] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_A_D] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.D, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_A_E] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.E, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_A_H] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.H, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_A_L] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.L, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.LD_A_AT_HL] = LD(Register.A, (WideRegister.HL, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.LD_A_A] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 4);
            StdOps[(int)Unprefixed.ADD_A_B] = ADD(Register.A, Register.B, 4);
            StdOps[(int)Unprefixed.ADD_A_C] = ADD(Register.A, Register.C, 4);
            StdOps[(int)Unprefixed.ADD_A_D] = ADD(Register.A, Register.D, 4);
            StdOps[(int)Unprefixed.ADD_A_E] = ADD(Register.A, Register.E, 4);
            StdOps[(int)Unprefixed.ADD_A_H] = ADD(Register.A, Register.H, 4);
            StdOps[(int)Unprefixed.ADD_A_L] = ADD(Register.A, Register.L, 4);
            StdOps[(int)Unprefixed.ADD_A_AT_HL] = ADD(Register.A, WideRegister.HL, 8);
            StdOps[(int)Unprefixed.ADD_A_A] = ADD(Register.A, Register.A, 4);
            StdOps[(int)Unprefixed.ADC_A_B] = ADC(Register.B, 4);
            StdOps[(int)Unprefixed.ADC_A_C] = ADC(Register.C, 4);
            StdOps[(int)Unprefixed.ADC_A_D] = ADC(Register.D, 4);
            StdOps[(int)Unprefixed.ADC_A_E] = ADC(Register.E, 4);
            StdOps[(int)Unprefixed.ADC_A_H] = ADC(Register.H, 4);
            StdOps[(int)Unprefixed.ADC_A_L] = ADC(Register.L, 4);
            StdOps[(int)Unprefixed.ADC_A_AT_HL] = ADC(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.ADC_A_A] = ADC(Register.A, 4);
            StdOps[(int)Unprefixed.SUB_B] = SUB(Register.B, 4);
            StdOps[(int)Unprefixed.SUB_C] = SUB(Register.C, 4);
            StdOps[(int)Unprefixed.SUB_D] = SUB(Register.D, 4);
            StdOps[(int)Unprefixed.SUB_E] = SUB(Register.E, 4);
            StdOps[(int)Unprefixed.SUB_H] = SUB(Register.H, 4);
            StdOps[(int)Unprefixed.SUB_L] = SUB(Register.L, 4);
            StdOps[(int)Unprefixed.SUB_AT_HL] = SUB(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.SUB_A] = SUB(Register.A, 4);
            StdOps[(int)Unprefixed.SBC_A_B] = SBC(Register.B, 4);
            StdOps[(int)Unprefixed.SBC_A_C] = SBC(Register.C, 4);
            StdOps[(int)Unprefixed.SBC_A_D] = SBC(Register.D, 4);
            StdOps[(int)Unprefixed.SBC_A_E] = SBC(Register.E, 4);
            StdOps[(int)Unprefixed.SBC_A_H] = SBC(Register.H, 4);
            StdOps[(int)Unprefixed.SBC_A_L] = SBC(Register.L, 4);
            StdOps[(int)Unprefixed.SBC_A_AT_HL] = SBC(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.SBC_A_A] = SBC(Register.A, 4);
            StdOps[(int)Unprefixed.AND_B] = AND(Register.B, 4);
            StdOps[(int)Unprefixed.AND_C] = AND(Register.C, 4);
            StdOps[(int)Unprefixed.AND_D] = AND(Register.D, 4);
            StdOps[(int)Unprefixed.AND_E] = AND(Register.E, 4);
            StdOps[(int)Unprefixed.AND_H] = AND(Register.H, 4);
            StdOps[(int)Unprefixed.AND_L] = AND(Register.L, 4);
            StdOps[(int)Unprefixed.AND_AT_HL] = AND(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.AND_A] = AND(Register.A, 4);
            StdOps[(int)Unprefixed.XOR_B] = XOR(Register.B, 4);
            StdOps[(int)Unprefixed.XOR_C] = XOR(Register.C, 4);
            StdOps[(int)Unprefixed.XOR_D] = XOR(Register.D, 4);
            StdOps[(int)Unprefixed.XOR_E] = XOR(Register.E, 4);
            StdOps[(int)Unprefixed.XOR_H] = XOR(Register.H, 4);
            StdOps[(int)Unprefixed.XOR_L] = XOR(Register.L, 4);
            StdOps[(int)Unprefixed.XOR_AT_HL] = XOR(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.XOR_A] = XOR(Register.A, 4);
            StdOps[(int)Unprefixed.OR_B] = OR(Register.B, 4);
            StdOps[(int)Unprefixed.OR_C] = OR(Register.C, 4);
            StdOps[(int)Unprefixed.OR_D] = OR(Register.D, 4);
            StdOps[(int)Unprefixed.OR_E] = OR(Register.E, 4);
            StdOps[(int)Unprefixed.OR_H] = OR(Register.H, 4);
            StdOps[(int)Unprefixed.OR_L] = OR(Register.L, 4);
            StdOps[(int)Unprefixed.OR_AT_HL] = OR(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.OR_A] = OR(Register.A, 4);
            StdOps[(int)Unprefixed.CP_B] = CP(Register.B, 4);
            StdOps[(int)Unprefixed.CP_C] = CP(Register.C, 4);
            StdOps[(int)Unprefixed.CP_D] = CP(Register.D, 4);
            StdOps[(int)Unprefixed.CP_E] = CP(Register.E, 4);
            StdOps[(int)Unprefixed.CP_H] = CP(Register.H, 4);
            StdOps[(int)Unprefixed.CP_L] = CP(Register.L, 4);
            StdOps[(int)Unprefixed.CP_AT_HL] = CP(WideRegister.HL, 8);
            StdOps[(int)Unprefixed.CP_A] = CP(Register.A, 4);
            StdOps[(int)Unprefixed.RET_NZ] = RET(Flag.NZ, 20, 8);
            StdOps[(int)Unprefixed.POP_BC] = POP(WideRegister.BC, 12);
            StdOps[(int)Unprefixed.JP_NZ_a16] = JP_A16(Flag.NZ, 16, 12);
            StdOps[(int)Unprefixed.JP_a16] = JP(DMGInteger.a16, 16);
            StdOps[(int)Unprefixed.CALL_NZ_a16] = CALL_A16(Flag.NZ, 24, 12);
            StdOps[(int)Unprefixed.PUSH_BC] = PUSH(WideRegister.BC, 16);
            StdOps[(int)Unprefixed.ADD_A_d8] = ADD_A_d8(8);
            StdOps[(int)Unprefixed.RST_00H] = RST(0x00, 16);
            StdOps[(int)Unprefixed.RET_Zero] = RET(Flag.Z, 20, 8);
            StdOps[(int)Unprefixed.RET] = RET(16);
            StdOps[(int)Unprefixed.JP_Zero_a16] = JP_A16(Flag.Z, 16, 12);
            StdOps[(int)Unprefixed.PREFIX] = PREFIX(4);
            StdOps[(int)Unprefixed.CALL_Zero_a16] = CALL_A16(Flag.Z, 24, 12);
            StdOps[(int)Unprefixed.CALL_a16] = CALL_a16(24);
            StdOps[(int)Unprefixed.ADC_A_d8] = ADC(8);
            StdOps[(int)Unprefixed.RST_08H] = RST(0x08, 16);
            StdOps[(int)Unprefixed.RET_NC] = RET(Flag.NC, 20, 8);
            StdOps[(int)Unprefixed.POP_DE] = POP(WideRegister.DE, 12);
            StdOps[(int)Unprefixed.JP_NC_a16] = JP_A16(Flag.NC, 16, 12);
            StdOps[(int)Unprefixed.ILLEGAL_D3] = ILLEGAL_D3(4);
            StdOps[(int)Unprefixed.CALL_NC_a16] = CALL_A16(Flag.NC, 24, 12);
            StdOps[(int)Unprefixed.PUSH_DE] = PUSH(WideRegister.DE, 16);
            StdOps[(int)Unprefixed.SUB_d8] = SUB(8);
            StdOps[(int)Unprefixed.RST_10H] = RST(0x10, 16);
            StdOps[(int)Unprefixed.RET_Carry] = RET(Flag.C, 20, 8);
            StdOps[(int)Unprefixed.RETI] = RETI(16);
            StdOps[(int)Unprefixed.JP_Carry_a16] = JP_A16(Flag.C, 16, 12);
            StdOps[(int)Unprefixed.ILLEGAL_DB] = ILLEGAL_DB(4);
            StdOps[(int)Unprefixed.CALL_Carry_a16] = CALL_A16(Flag.C, 24, 12);
            StdOps[(int)Unprefixed.ILLEGAL_DD] = ILLEGAL_DD(4);
            StdOps[(int)Unprefixed.SBC_A_d8] = SBC(8);
            StdOps[(int)Unprefixed.RST_18H] = RST(0x18, 16);
            StdOps[(int)Unprefixed.LDH_AT_a8_A] = LDH(12);
            StdOps[(int)Unprefixed.POP_HL] = POP(WideRegister.HL, 12);
            StdOps[(int)Unprefixed.LD_AT_C_A] = LD((Register.C, new Traits(false, Postfix.unchanged)), (Register.A, new Traits(true, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.ILLEGAL_E3] = ILLEGAL_E3(4);
            StdOps[(int)Unprefixed.ILLEGAL_E4] = ILLEGAL_E4(4);
            StdOps[(int)Unprefixed.PUSH_HL] = PUSH(WideRegister.HL, 16);
            StdOps[(int)Unprefixed.AND_d8] = AND(8);
            StdOps[(int)Unprefixed.RST_20H] = RST(0x20, 16);
            StdOps[(int)Unprefixed.ADD_SP_r8] = ADD_SP_R8(16);
            StdOps[(int)Unprefixed.JP_HL] = JP(4);
            StdOps[(int)Unprefixed.LD_AT_a16_A] = LD_AT_a16_A(16);
            StdOps[(int)Unprefixed.ILLEGAL_EB] = ILLEGAL_EB(4);
            StdOps[(int)Unprefixed.ILLEGAL_EC] = ILLEGAL_EC(4);
            StdOps[(int)Unprefixed.ILLEGAL_ED] = ILLEGAL_ED(4);
            StdOps[(int)Unprefixed.XOR_d8] = XOR(8);
            StdOps[(int)Unprefixed.RST_28H] = RST(0x28, 16);
            StdOps[(int)Unprefixed.LDH_A_AT_a8] = LDH_A_AT_a8(12);
            StdOps[(int)Unprefixed.POP_AF] = POP(WideRegister.AF, 12);
            StdOps[(int)Unprefixed.LD_A_AT_C] = LD((Register.A, new Traits(true, Postfix.unchanged)), (Register.C, new Traits(false, Postfix.unchanged)), 8);
            StdOps[(int)Unprefixed.DI] = DI(4);
            StdOps[(int)Unprefixed.ILLEGAL_F4] = ILLEGAL_F4(4);
            StdOps[(int)Unprefixed.PUSH_AF] = PUSH(WideRegister.AF, 16);
            StdOps[(int)Unprefixed.OR_d8] = OR(8);
            StdOps[(int)Unprefixed.RST_30H] = RST(0x30, 16);
            StdOps[(int)Unprefixed.LD_HL_SP_i8] = LD_HL_SP_i8(12);
            StdOps[(int)Unprefixed.LD_SP_HL] = LD_SP_HL(8);
            StdOps[(int)Unprefixed.LD_A_AT_a16] = LD(Register.A, DMGInteger.a16, 16);
            StdOps[(int)Unprefixed.EI] = EI(4);
            StdOps[(int)Unprefixed.ILLEGAL_FC] = ILLEGAL_FC(4);
            StdOps[(int)Unprefixed.ILLEGAL_FD] = ILLEGAL_FD(4);
            StdOps[(int)Unprefixed.CP_d8] = CP(8);
            StdOps[(int)Unprefixed.RST_38H] = RST(0x38, 16);

            CbOps = new Action[0x100];
            CbOps[(int)Cbprefixed.RLC_B] = RLC(Register.B, 8);
            CbOps[(int)Cbprefixed.RLC_C] = RLC(Register.C, 8);
            CbOps[(int)Cbprefixed.RLC_D] = RLC(Register.D, 8);
            CbOps[(int)Cbprefixed.RLC_E] = RLC(Register.E, 8);
            CbOps[(int)Cbprefixed.RLC_H] = RLC(Register.H, 8);
            CbOps[(int)Cbprefixed.RLC_L] = RLC(Register.L, 8);
            CbOps[(int)Cbprefixed.RLC_AT_HL] = RLC(Register.HL, 16);
            CbOps[(int)Cbprefixed.RLC_A] = RLC(Register.A, 8);
            CbOps[(int)Cbprefixed.RRC_B] = RRC(Register.B, 8);
            CbOps[(int)Cbprefixed.RRC_C] = RRC(Register.C, 8);
            CbOps[(int)Cbprefixed.RRC_D] = RRC(Register.D, 8);
            CbOps[(int)Cbprefixed.RRC_E] = RRC(Register.E, 8);
            CbOps[(int)Cbprefixed.RRC_H] = RRC(Register.H, 8);
            CbOps[(int)Cbprefixed.RRC_L] = RRC(Register.L, 8);
            CbOps[(int)Cbprefixed.RRC_AT_HL] = RRC(Register.HL, 16);
            CbOps[(int)Cbprefixed.RRC_A] = RRC(Register.A, 8);
            CbOps[(int)Cbprefixed.RL_B] = RL(Register.B, 8);
            CbOps[(int)Cbprefixed.RL_C] = RL(Register.C, 8);
            CbOps[(int)Cbprefixed.RL_D] = RL(Register.D, 8);
            CbOps[(int)Cbprefixed.RL_E] = RL(Register.E, 8);
            CbOps[(int)Cbprefixed.RL_H] = RL(Register.H, 8);
            CbOps[(int)Cbprefixed.RL_L] = RL(Register.L, 8);
            CbOps[(int)Cbprefixed.RL_AT_HL] = RL(Register.HL, 16);
            CbOps[(int)Cbprefixed.RL_A] = RL(Register.A, 8);
            CbOps[(int)Cbprefixed.RR_B] = RR(Register.B, 8);
            CbOps[(int)Cbprefixed.RR_C] = RR(Register.C, 8);
            CbOps[(int)Cbprefixed.RR_D] = RR(Register.D, 8);
            CbOps[(int)Cbprefixed.RR_E] = RR(Register.E, 8);
            CbOps[(int)Cbprefixed.RR_H] = RR(Register.H, 8);
            CbOps[(int)Cbprefixed.RR_L] = RR(Register.L, 8);
            CbOps[(int)Cbprefixed.RR_AT_HL] = RR(Register.HL, 16);
            CbOps[(int)Cbprefixed.RR_A] = RR(Register.A, 8);
            CbOps[(int)Cbprefixed.SLA_B] = SLA(Register.B, 8);
            CbOps[(int)Cbprefixed.SLA_C] = SLA(Register.C, 8);
            CbOps[(int)Cbprefixed.SLA_D] = SLA(Register.D, 8);
            CbOps[(int)Cbprefixed.SLA_E] = SLA(Register.E, 8);
            CbOps[(int)Cbprefixed.SLA_H] = SLA(Register.H, 8);
            CbOps[(int)Cbprefixed.SLA_L] = SLA(Register.L, 8);
            CbOps[(int)Cbprefixed.SLA_AT_HL] = SLA(Register.HL, 16);
            CbOps[(int)Cbprefixed.SLA_A] = SLA(Register.A, 8);
            CbOps[(int)Cbprefixed.SRA_B] = SRA(Register.B, 8);
            CbOps[(int)Cbprefixed.SRA_C] = SRA(Register.C, 8);
            CbOps[(int)Cbprefixed.SRA_D] = SRA(Register.D, 8);
            CbOps[(int)Cbprefixed.SRA_E] = SRA(Register.E, 8);
            CbOps[(int)Cbprefixed.SRA_H] = SRA(Register.H, 8);
            CbOps[(int)Cbprefixed.SRA_L] = SRA(Register.L, 8);
            CbOps[(int)Cbprefixed.SRA_AT_HL] = SRA(Register.HL, 16);
            CbOps[(int)Cbprefixed.SRA_A] = SRA(Register.A, 8);
            CbOps[(int)Cbprefixed.SWAP_B] = SWAP(Register.B, 8);
            CbOps[(int)Cbprefixed.SWAP_C] = SWAP(Register.C, 8);
            CbOps[(int)Cbprefixed.SWAP_D] = SWAP(Register.D, 8);
            CbOps[(int)Cbprefixed.SWAP_E] = SWAP(Register.E, 8);
            CbOps[(int)Cbprefixed.SWAP_H] = SWAP(Register.H, 8);
            CbOps[(int)Cbprefixed.SWAP_L] = SWAP(Register.L, 8);
            CbOps[(int)Cbprefixed.SWAP_AT_HL] = SWAP(Register.HL, 16);
            CbOps[(int)Cbprefixed.SWAP_A] = SWAP(Register.A, 8);
            CbOps[(int)Cbprefixed.SRL_B] = SRL(Register.B, 8);
            CbOps[(int)Cbprefixed.SRL_C] = SRL(Register.C, 8);
            CbOps[(int)Cbprefixed.SRL_D] = SRL(Register.D, 8);
            CbOps[(int)Cbprefixed.SRL_E] = SRL(Register.E, 8);
            CbOps[(int)Cbprefixed.SRL_H] = SRL(Register.H, 8);
            CbOps[(int)Cbprefixed.SRL_L] = SRL(Register.L, 8);
            CbOps[(int)Cbprefixed.SRL_AT_HL] = SRL(Register.HL, 16);
            CbOps[(int)Cbprefixed.SRL_A] = SRL(Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_0_B] = BIT(0, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_0_C] = BIT(0, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_0_D] = BIT(0, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_0_E] = BIT(0, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_0_H] = BIT(0, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_0_L] = BIT(0, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_0_AT_HL] = BIT(0, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_0_A] = BIT(0, Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_1_B] = BIT(1, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_1_C] = BIT(1, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_1_D] = BIT(1, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_1_E] = BIT(1, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_1_H] = BIT(1, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_1_L] = BIT(1, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_1_AT_HL] = BIT(1, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_1_A] = BIT(1, Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_2_B] = BIT(2, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_2_C] = BIT(2, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_2_D] = BIT(2, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_2_E] = BIT(2, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_2_H] = BIT(2, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_2_L] = BIT(2, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_2_AT_HL] = BIT(2, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_2_A] = BIT(2, Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_3_B] = BIT(3, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_3_C] = BIT(3, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_3_D] = BIT(3, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_3_E] = BIT(3, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_3_H] = BIT(3, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_3_L] = BIT(3, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_3_AT_HL] = BIT(3, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_3_A] = BIT(3, Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_4_B] = BIT(4, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_4_C] = BIT(4, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_4_D] = BIT(4, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_4_E] = BIT(4, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_4_H] = BIT(4, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_4_L] = BIT(4, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_4_AT_HL] = BIT(4, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_4_A] = BIT(4, Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_5_B] = BIT(5, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_5_C] = BIT(5, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_5_D] = BIT(5, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_5_E] = BIT(5, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_5_H] = BIT(5, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_5_L] = BIT(5, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_5_AT_HL] = BIT(5, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_5_A] = BIT(5, Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_6_B] = BIT(6, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_6_C] = BIT(6, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_6_D] = BIT(6, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_6_E] = BIT(6, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_6_H] = BIT(6, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_6_L] = BIT(6, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_6_AT_HL] = BIT(6, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_6_A] = BIT(6, Register.A, 8);
            CbOps[(int)Cbprefixed.BIT_7_B] = BIT(7, Register.B, 8);
            CbOps[(int)Cbprefixed.BIT_7_C] = BIT(7, Register.C, 8);
            CbOps[(int)Cbprefixed.BIT_7_D] = BIT(7, Register.D, 8);
            CbOps[(int)Cbprefixed.BIT_7_E] = BIT(7, Register.E, 8);
            CbOps[(int)Cbprefixed.BIT_7_H] = BIT(7, Register.H, 8);
            CbOps[(int)Cbprefixed.BIT_7_L] = BIT(7, Register.L, 8);
            CbOps[(int)Cbprefixed.BIT_7_AT_HL] = BIT(7, Register.HL, 12);
            CbOps[(int)Cbprefixed.BIT_7_A] = BIT(7, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_0_B] = RES(0, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_0_C] = RES(0, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_0_D] = RES(0, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_0_E] = RES(0, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_0_H] = RES(0, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_0_L] = RES(0, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_0_AT_HL] = RES(0, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_0_A] = RES(0, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_1_B] = RES(1, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_1_C] = RES(1, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_1_D] = RES(1, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_1_E] = RES(1, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_1_H] = RES(1, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_1_L] = RES(1, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_1_AT_HL] = RES(1, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_1_A] = RES(1, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_2_B] = RES(2, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_2_C] = RES(2, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_2_D] = RES(2, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_2_E] = RES(2, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_2_H] = RES(2, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_2_L] = RES(2, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_2_AT_HL] = RES(2, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_2_A] = RES(2, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_3_B] = RES(3, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_3_C] = RES(3, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_3_D] = RES(3, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_3_E] = RES(3, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_3_H] = RES(3, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_3_L] = RES(3, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_3_AT_HL] = RES(3, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_3_A] = RES(3, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_4_B] = RES(4, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_4_C] = RES(4, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_4_D] = RES(4, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_4_E] = RES(4, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_4_H] = RES(4, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_4_L] = RES(4, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_4_AT_HL] = RES(4, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_4_A] = RES(4, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_5_B] = RES(5, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_5_C] = RES(5, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_5_D] = RES(5, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_5_E] = RES(5, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_5_H] = RES(5, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_5_L] = RES(5, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_5_AT_HL] = RES(5, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_5_A] = RES(5, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_6_B] = RES(6, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_6_C] = RES(6, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_6_D] = RES(6, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_6_E] = RES(6, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_6_H] = RES(6, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_6_L] = RES(6, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_6_AT_HL] = RES(6, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_6_A] = RES(6, Register.A, 8);
            CbOps[(int)Cbprefixed.RES_7_B] = RES(7, Register.B, 8);
            CbOps[(int)Cbprefixed.RES_7_C] = RES(7, Register.C, 8);
            CbOps[(int)Cbprefixed.RES_7_D] = RES(7, Register.D, 8);
            CbOps[(int)Cbprefixed.RES_7_E] = RES(7, Register.E, 8);
            CbOps[(int)Cbprefixed.RES_7_H] = RES(7, Register.H, 8);
            CbOps[(int)Cbprefixed.RES_7_L] = RES(7, Register.L, 8);
            CbOps[(int)Cbprefixed.RES_7_AT_HL] = RES(7, Register.HL, 16);
            CbOps[(int)Cbprefixed.RES_7_A] = RES(7, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_0_B] = SET(0, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_0_C] = SET(0, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_0_D] = SET(0, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_0_E] = SET(0, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_0_H] = SET(0, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_0_L] = SET(0, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_0_AT_HL] = SET(0, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_0_A] = SET(0, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_1_B] = SET(1, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_1_C] = SET(1, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_1_D] = SET(1, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_1_E] = SET(1, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_1_H] = SET(1, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_1_L] = SET(1, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_1_AT_HL] = SET(1, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_1_A] = SET(1, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_2_B] = SET(2, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_2_C] = SET(2, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_2_D] = SET(2, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_2_E] = SET(2, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_2_H] = SET(2, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_2_L] = SET(2, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_2_AT_HL] = SET(2, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_2_A] = SET(2, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_3_B] = SET(3, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_3_C] = SET(3, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_3_D] = SET(3, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_3_E] = SET(3, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_3_H] = SET(3, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_3_L] = SET(3, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_3_AT_HL] = SET(3, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_3_A] = SET(3, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_4_B] = SET(4, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_4_C] = SET(4, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_4_D] = SET(4, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_4_E] = SET(4, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_4_H] = SET(4, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_4_L] = SET(4, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_4_AT_HL] = SET(4, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_4_A] = SET(4, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_5_B] = SET(5, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_5_C] = SET(5, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_5_D] = SET(5, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_5_E] = SET(5, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_5_H] = SET(5, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_5_L] = SET(5, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_5_AT_HL] = SET(5, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_5_A] = SET(5, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_6_B] = SET(6, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_6_C] = SET(6, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_6_D] = SET(6, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_6_E] = SET(6, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_6_H] = SET(6, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_6_L] = SET(6, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_6_AT_HL] = SET(6, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_6_A] = SET(6, Register.A, 8);
            CbOps[(int)Cbprefixed.SET_7_B] = SET(7, Register.B, 8);
            CbOps[(int)Cbprefixed.SET_7_C] = SET(7, Register.C, 8);
            CbOps[(int)Cbprefixed.SET_7_D] = SET(7, Register.D, 8);
            CbOps[(int)Cbprefixed.SET_7_E] = SET(7, Register.E, 8);
            CbOps[(int)Cbprefixed.SET_7_H] = SET(7, Register.H, 8);
            CbOps[(int)Cbprefixed.SET_7_L] = SET(7, Register.L, 8);
            CbOps[(int)Cbprefixed.SET_7_AT_HL] = SET(7, Register.HL, 16);
            CbOps[(int)Cbprefixed.SET_7_A] = SET(7, Register.A, 8);

            Registers = new Registers();

            enableInterruptsDelayed = () => InterruptEnableScheduled = true;
            enableInterrupts = () => IME = true;
            disableInterrupts = () => IME = false;
            halt = () => Halted = IME
                    ? HaltState.normal
                    : (InterruptFireRegister & InterruptControlRegister & 0x1f) == 0 ? HaltState.normalIME0 : HaltState.haltbug;
        }

        internal (Action<byte> Write, Func<byte> Read) HookUpCPU() => (x => InterruptFireRegister = x,
            () => InterruptFireRegister);
        internal byte ReadOp() => Memory[PC++];

        public ushort PC = 0;
        private byte ReadHaltBug()
        {
            Halted = HaltState.off;
            return Memory[PC];
        }
        internal void DoNextOP()
        {
            if (Halted != HaltState.off)
            {
                if (Halted != HaltState.haltbug)
                {
                    return;
                }
            }

            var op = Halted == HaltState.haltbug ? ReadHaltBug() : Memory[PC++];
            if (op != 0xcb)
            {
                Op((Unprefixed)op)();
            }
            else
            {
                var CBop = Memory[PC++]; //Because of the CB prefix we encountered in the previous case we already skipped the extra byte of a cb instruction here
                Op((Cbprefixed)CBop)();
            }
        }
        internal void Tick()
        {
            if (TicksWeAreWaitingFor == 0)
            {
                DoNextOP();
                //We really should have the GUI thread somehow do this logic but polling like this should work
                if (!InterruptFireRegister.GetBit(4) && GetKeyboardInterrupt())
                {
                    InterruptFireRegister = InterruptFireRegister.SetBit(4);
                }

                DoInterrupt();
                if (InterruptEnableScheduled)
                {
                    IME = true;
                    InterruptEnableScheduled = false;
                }
            }
            else
            {
                TicksWeAreWaitingFor--;
            }
        }
    }
}