﻿using emulator.memory;
using emulator.registers;

using Microsoft.Extensions.Logging;

namespace emulator.opcodes;
public partial class CPU
{
    private readonly Action[] StdOps;
    private readonly Action[] CbOps;
    private readonly InterruptRegisters ISR;
    public readonly Registers Registers;
    private readonly MMU Memory;
    public ushort PC;
    private readonly ILogger Logger;

    private HaltState Halted = HaltState.off;

    public Action Op(Opcode op) => StdOps[(int)op];

    public Action Op(CBOpcode op) => CbOps[(int)op];

    public CPU(MMU mMU, InterruptRegisters interruptRegisters, ILogger<CPU> logger)
    {
        Memory = mMU;
        ISR = interruptRegisters;
        Logger = logger;

        StdOps = new Action[0x100];
        StdOps[(int)Opcode.NOP] = NOP;
        StdOps[(int)Opcode.LD_AT_DE_A] = LD(WideRegister.DE);
        StdOps[(int)Opcode.LD_AT_BC_A] = LD(WideRegister.BC);
        StdOps[(int)Opcode.INC_BC] = INC(WideRegister.BC);
        StdOps[(int)Opcode.INC_B] = INC(Register.B);
        StdOps[(int)Opcode.DEC_B] = DEC(Register.B);
        StdOps[(int)Opcode.LD_B_d8] = LD_D8(Register.B);
        StdOps[(int)Opcode.RLCA] = RLCA;
        StdOps[(int)Opcode.LD_AT_a16_SP] = WriteSPToMem;
        StdOps[(int)Opcode.ADD_HL_BC] = ADD(WideRegister.BC);
        StdOps[(int)Opcode.LD_A_AT_BC] = LD_AT(WideRegister.BC);
        StdOps[(int)Opcode.DEC_BC] = DEC(WideRegister.BC);
        StdOps[(int)Opcode.INC_C] = INC(Register.C);
        StdOps[(int)Opcode.DEC_C] = DEC(Register.C);
        StdOps[(int)Opcode.LD_C_d8] = LD_D8(Register.C);
        StdOps[(int)Opcode.RRCA] = RRCA;
        StdOps[(int)Opcode.STOP] = STOP;
        StdOps[(int)Opcode.LD_DE_d16] = LD_D16(WideRegister.DE);
        StdOps[(int)Opcode.LD_BC_d16] = LD_D16(WideRegister.BC);
        StdOps[(int)Opcode.INC_DE] = INC(WideRegister.DE);
        StdOps[(int)Opcode.INC_D] = INC(Register.D);
        StdOps[(int)Opcode.DEC_D] = DEC(Register.D);
        StdOps[(int)Opcode.LD_D_d8] = LD_D8(Register.D);
        StdOps[(int)Opcode.RLA] = RLA;
        StdOps[(int)Opcode.JR_r8] = JR;
        StdOps[(int)Opcode.ADD_HL_DE] = ADD(WideRegister.DE);
        StdOps[(int)Opcode.LD_A_AT_DE] = LD_AT(WideRegister.DE);
        StdOps[(int)Opcode.DEC_DE] = DEC(WideRegister.DE);
        StdOps[(int)Opcode.INC_E] = INC(Register.E);
        StdOps[(int)Opcode.DEC_E] = DEC(Register.E);
        StdOps[(int)Opcode.LD_E_d8] = LD_D8(Register.E);
        StdOps[(int)Opcode.RRA] = RRA;
        StdOps[(int)Opcode.JR_NZ_r8] = JR(Flag.NZ);
        StdOps[(int)Opcode.LD_HL_d16] = LD_D16(WideRegister.HL);
        StdOps[(int)Opcode.LDI_AT_HL_A] = LDI;
        StdOps[(int)Opcode.INC_HL] = INC(WideRegister.HL);
        StdOps[(int)Opcode.INC_H] = INC(Register.H);
        StdOps[(int)Opcode.DEC_H] = DEC(Register.H);
        StdOps[(int)Opcode.LD_H_d8] = LD_D8(Register.H);
        StdOps[(int)Opcode.DAA] = DAA;
        StdOps[(int)Opcode.JR_Zero_r8] = JR(Flag.Z);
        StdOps[(int)Opcode.ADD_HL_HL] = ADD(WideRegister.HL);
        StdOps[(int)Opcode.LD_AI_AT_HL] = LDA_HLI;
        StdOps[(int)Opcode.DEC_HL] = DEC(WideRegister.HL);
        StdOps[(int)Opcode.INC_L] = INC(Register.L);
        StdOps[(int)Opcode.DEC_L] = DEC(Register.L);
        StdOps[(int)Opcode.LD_L_d8] = LD_D8(Register.L);
        StdOps[(int)Opcode.CPL] = CPL;
        StdOps[(int)Opcode.JR_NC_r8] = JR(Flag.NC);
        StdOps[(int)Opcode.LD_SP_d16] = LD_D16(WideRegister.SP);
        StdOps[(int)Opcode.LDD_AT_HL_A] = LDD;
        StdOps[(int)Opcode.INC_SP] = INC(WideRegister.SP);
        StdOps[(int)Opcode.INC_AT_HL] = INC(Register.HL);
        StdOps[(int)Opcode.DEC_AT_HL] = DEC(Register.HL);
        StdOps[(int)Opcode.LD_AT_HL_d8] = LD_D8(Register.HL);
        StdOps[(int)Opcode.SCF] = SCF;
        StdOps[(int)Opcode.JR_Carry_r8] = JR(Flag.C);
        StdOps[(int)Opcode.ADD_HL_SP] = ADD(WideRegister.SP);
        StdOps[(int)Opcode.LD_AD_AT_HL] = LDA_HLD;
        StdOps[(int)Opcode.DEC_SP] = DEC(WideRegister.SP);
        StdOps[(int)Opcode.INC_A] = INC(Register.A);
        StdOps[(int)Opcode.DEC_A] = DEC(Register.A);
        StdOps[(int)Opcode.LD_A_d8] = LD_D8(Register.A);
        StdOps[(int)Opcode.CCF] = CCF;
        StdOps[(int)Opcode.LD_B_B] = LD(Register.B, Register.B);
        StdOps[(int)Opcode.LD_B_C] = LD(Register.B, Register.C);
        StdOps[(int)Opcode.LD_B_D] = LD(Register.B, Register.D);
        StdOps[(int)Opcode.LD_B_E] = LD(Register.B, Register.E);
        StdOps[(int)Opcode.LD_B_H] = LD(Register.B, Register.H);
        StdOps[(int)Opcode.LD_B_L] = LD(Register.B, Register.L);
        StdOps[(int)Opcode.LD_B_AT_HL] = LD(Register.B, Register.HL);
        StdOps[(int)Opcode.LD_B_A] = LD(Register.B, Register.A);
        StdOps[(int)Opcode.LD_C_B] = LD(Register.C, Register.B);
        StdOps[(int)Opcode.LD_C_C] = LD(Register.C, Register.C);
        StdOps[(int)Opcode.LD_C_D] = LD(Register.C, Register.D);
        StdOps[(int)Opcode.LD_C_E] = LD(Register.C, Register.E);
        StdOps[(int)Opcode.LD_C_H] = LD(Register.C, Register.H);
        StdOps[(int)Opcode.LD_C_L] = LD(Register.C, Register.L);
        StdOps[(int)Opcode.LD_C_AT_HL] = LD(Register.C, Register.HL);
        StdOps[(int)Opcode.LD_C_A] = LD(Register.C, Register.A);
        StdOps[(int)Opcode.LD_D_B] = LD(Register.D, Register.B);
        StdOps[(int)Opcode.LD_D_C] = LD(Register.D, Register.C);
        StdOps[(int)Opcode.LD_D_D] = LD(Register.D, Register.D);
        StdOps[(int)Opcode.LD_D_E] = LD(Register.D, Register.E);
        StdOps[(int)Opcode.LD_D_H] = LD(Register.D, Register.H);
        StdOps[(int)Opcode.LD_D_L] = LD(Register.D, Register.L);
        StdOps[(int)Opcode.LD_D_AT_HL] = LD(Register.D, Register.HL);
        StdOps[(int)Opcode.LD_D_A] = LD(Register.D, Register.A);
        StdOps[(int)Opcode.LD_E_B] = LD(Register.E, Register.B);
        StdOps[(int)Opcode.LD_E_C] = LD(Register.E, Register.C);
        StdOps[(int)Opcode.LD_E_D] = LD(Register.E, Register.D);
        StdOps[(int)Opcode.LD_E_E] = LD(Register.E, Register.E);
        StdOps[(int)Opcode.LD_E_H] = LD(Register.E, Register.H);
        StdOps[(int)Opcode.LD_E_L] = LD(Register.E, Register.L);
        StdOps[(int)Opcode.LD_E_AT_HL] = LD(Register.E, Register.HL);
        StdOps[(int)Opcode.LD_E_A] = LD(Register.E, Register.A);
        StdOps[(int)Opcode.LD_H_B] = LD(Register.H, Register.B);
        StdOps[(int)Opcode.LD_H_C] = LD(Register.H, Register.C);
        StdOps[(int)Opcode.LD_H_D] = LD(Register.H, Register.D);
        StdOps[(int)Opcode.LD_H_E] = LD(Register.H, Register.E);
        StdOps[(int)Opcode.LD_H_H] = LD(Register.H, Register.H);
        StdOps[(int)Opcode.LD_H_L] = LD(Register.H, Register.L);
        StdOps[(int)Opcode.LD_H_AT_HL] = LD(Register.H, Register.HL);
        StdOps[(int)Opcode.LD_H_A] = LD(Register.H, Register.A);
        StdOps[(int)Opcode.LD_L_B] = LD(Register.L, Register.B);
        StdOps[(int)Opcode.LD_L_C] = LD(Register.L, Register.C);
        StdOps[(int)Opcode.LD_L_D] = LD(Register.L, Register.D);
        StdOps[(int)Opcode.LD_L_E] = LD(Register.L, Register.E);
        StdOps[(int)Opcode.LD_L_H] = LD(Register.L, Register.H);
        StdOps[(int)Opcode.LD_L_L] = LD(Register.L, Register.L);
        StdOps[(int)Opcode.LD_L_AT_HL] = LD(Register.L, Register.HL);
        StdOps[(int)Opcode.LD_L_A] = LD(Register.L, Register.A);
        StdOps[(int)Opcode.LD_AT_HL_B] = LD(Register.HL, Register.B);
        StdOps[(int)Opcode.LD_AT_HL_C] = LD(Register.HL, Register.C);
        StdOps[(int)Opcode.LD_AT_HL_D] = LD(Register.HL, Register.D);
        StdOps[(int)Opcode.LD_AT_HL_E] = LD(Register.HL, Register.E);
        StdOps[(int)Opcode.LD_AT_HL_H] = LD(Register.HL, Register.H);
        StdOps[(int)Opcode.LD_AT_HL_L] = LD(Register.HL, Register.L);
        StdOps[(int)Opcode.HALT] = HALT;
        StdOps[(int)Opcode.LD_AT_HL_A] = LD(Register.HL, Register.A);
        StdOps[(int)Opcode.LD_A_B] = LD(Register.A, Register.B);
        StdOps[(int)Opcode.LD_A_C] = LD(Register.A, Register.C);
        StdOps[(int)Opcode.LD_A_D] = LD(Register.A, Register.D);
        StdOps[(int)Opcode.LD_A_E] = LD(Register.A, Register.E);
        StdOps[(int)Opcode.LD_A_H] = LD(Register.A, Register.H);
        StdOps[(int)Opcode.LD_A_L] = LD(Register.A, Register.L);
        StdOps[(int)Opcode.LD_A_AT_HL] = LD(Register.A, Register.HL);
        StdOps[(int)Opcode.LD_A_A] = LD(Register.A, Register.A);
        StdOps[(int)Opcode.ADD_A_B] = ADD(Register.B);
        StdOps[(int)Opcode.ADD_A_C] = ADD(Register.C);
        StdOps[(int)Opcode.ADD_A_D] = ADD(Register.D);
        StdOps[(int)Opcode.ADD_A_E] = ADD(Register.E);
        StdOps[(int)Opcode.ADD_A_H] = ADD(Register.H);
        StdOps[(int)Opcode.ADD_A_L] = ADD(Register.L);
        StdOps[(int)Opcode.ADD_A_AT_HL] = ADD(Register.HL);
        StdOps[(int)Opcode.ADD_A_A] = ADD(Register.A);
        StdOps[(int)Opcode.ADC_A_B] = ADC(Register.B);
        StdOps[(int)Opcode.ADC_A_C] = ADC(Register.C);
        StdOps[(int)Opcode.ADC_A_D] = ADC(Register.D);
        StdOps[(int)Opcode.ADC_A_E] = ADC(Register.E);
        StdOps[(int)Opcode.ADC_A_H] = ADC(Register.H);
        StdOps[(int)Opcode.ADC_A_L] = ADC(Register.L);
        StdOps[(int)Opcode.ADC_A_AT_HL] = ADC(Register.HL);
        StdOps[(int)Opcode.ADC_A_A] = ADC(Register.A);
        StdOps[(int)Opcode.SUB_B] = SUB(Register.B);
        StdOps[(int)Opcode.SUB_C] = SUB(Register.C);
        StdOps[(int)Opcode.SUB_D] = SUB(Register.D);
        StdOps[(int)Opcode.SUB_E] = SUB(Register.E);
        StdOps[(int)Opcode.SUB_H] = SUB(Register.H);
        StdOps[(int)Opcode.SUB_L] = SUB(Register.L);
        StdOps[(int)Opcode.SUB_AT_HL] = SUB(Register.HL);
        StdOps[(int)Opcode.SUB_A] = SUB(Register.A);
        StdOps[(int)Opcode.SBC_A_B] = SBC(Register.B);
        StdOps[(int)Opcode.SBC_A_C] = SBC(Register.C);
        StdOps[(int)Opcode.SBC_A_D] = SBC(Register.D);
        StdOps[(int)Opcode.SBC_A_E] = SBC(Register.E);
        StdOps[(int)Opcode.SBC_A_H] = SBC(Register.H);
        StdOps[(int)Opcode.SBC_A_L] = SBC(Register.L);
        StdOps[(int)Opcode.SBC_A_AT_HL] = SBC(Register.HL);
        StdOps[(int)Opcode.SBC_A_A] = SBC(Register.A);
        StdOps[(int)Opcode.AND_B] = AND(Register.B);
        StdOps[(int)Opcode.AND_C] = AND(Register.C);
        StdOps[(int)Opcode.AND_D] = AND(Register.D);
        StdOps[(int)Opcode.AND_E] = AND(Register.E);
        StdOps[(int)Opcode.AND_H] = AND(Register.H);
        StdOps[(int)Opcode.AND_L] = AND(Register.L);
        StdOps[(int)Opcode.AND_AT_HL] = AND(Register.HL);
        StdOps[(int)Opcode.AND_A] = AND(Register.A);
        StdOps[(int)Opcode.XOR_B] = XOR(Register.B);
        StdOps[(int)Opcode.XOR_C] = XOR(Register.C);
        StdOps[(int)Opcode.XOR_D] = XOR(Register.D);
        StdOps[(int)Opcode.XOR_E] = XOR(Register.E);
        StdOps[(int)Opcode.XOR_H] = XOR(Register.H);
        StdOps[(int)Opcode.XOR_L] = XOR(Register.L);
        StdOps[(int)Opcode.XOR_AT_HL] = XOR(Register.HL);
        StdOps[(int)Opcode.XOR_A] = XOR(Register.A);
        StdOps[(int)Opcode.OR_B] = OR(Register.B);
        StdOps[(int)Opcode.OR_C] = OR(Register.C);
        StdOps[(int)Opcode.OR_D] = OR(Register.D);
        StdOps[(int)Opcode.OR_E] = OR(Register.E);
        StdOps[(int)Opcode.OR_H] = OR(Register.H);
        StdOps[(int)Opcode.OR_L] = OR(Register.L);
        StdOps[(int)Opcode.OR_AT_HL] = OR(Register.HL);
        StdOps[(int)Opcode.OR_A] = OR(Register.A);
        StdOps[(int)Opcode.CP_B] = CP(Register.B);
        StdOps[(int)Opcode.CP_C] = CP(Register.C);
        StdOps[(int)Opcode.CP_D] = CP(Register.D);
        StdOps[(int)Opcode.CP_E] = CP(Register.E);
        StdOps[(int)Opcode.CP_H] = CP(Register.H);
        StdOps[(int)Opcode.CP_L] = CP(Register.L);
        StdOps[(int)Opcode.CP_AT_HL] = CP(Register.HL);
        StdOps[(int)Opcode.CP_A] = CP(Register.A);
        StdOps[(int)Opcode.RET_NZ] = RET(Flag.NZ);
        StdOps[(int)Opcode.POP_BC] = POP(WideRegister.BC);
        StdOps[(int)Opcode.JP_NZ_a16] = JP_A16(Flag.NZ);
        StdOps[(int)Opcode.JP_a16] = JP_A16;
        StdOps[(int)Opcode.CALL_NZ_a16] = CALL_A16(Flag.NZ);
        StdOps[(int)Opcode.PUSH_BC] = PUSH(WideRegister.BC);
        StdOps[(int)Opcode.ADD_A_d8] = ADD_A_d8;
        StdOps[(int)Opcode.RST_00H] = RST(0x00);
        StdOps[(int)Opcode.RET_Zero] = RET(Flag.Z);
        StdOps[(int)Opcode.RET] = RET;
        StdOps[(int)Opcode.JP_Zero_a16] = JP_A16(Flag.Z);
        StdOps[(int)Opcode.PREFIX] = PREFIX;
        StdOps[(int)Opcode.CALL_Zero_a16] = CALL_A16(Flag.Z);
        StdOps[(int)Opcode.CALL_a16] = CALL_a16;
        StdOps[(int)Opcode.ADC_A_d8] = ADC;
        StdOps[(int)Opcode.RST_08H] = RST(0x08);
        StdOps[(int)Opcode.RET_NC] = RET(Flag.NC);
        StdOps[(int)Opcode.POP_DE] = POP(WideRegister.DE);
        StdOps[(int)Opcode.JP_NC_a16] = JP_A16(Flag.NC);
        StdOps[(int)Opcode.ILLEGAL_D3] = ILLEGAL_D3;
        StdOps[(int)Opcode.CALL_NC_a16] = CALL_A16(Flag.NC);
        StdOps[(int)Opcode.PUSH_DE] = PUSH(WideRegister.DE);
        StdOps[(int)Opcode.SUB_d8] = SUB;
        StdOps[(int)Opcode.RST_10H] = RST(0x10);
        StdOps[(int)Opcode.RET_Carry] = RET(Flag.C);
        StdOps[(int)Opcode.RETI] = RETI;
        StdOps[(int)Opcode.JP_Carry_a16] = JP_A16(Flag.C);
        StdOps[(int)Opcode.ILLEGAL_DB] = ILLEGAL_DB;
        StdOps[(int)Opcode.CALL_Carry_a16] = CALL_A16(Flag.C);
        StdOps[(int)Opcode.ILLEGAL_DD] = ILLEGAL_DD;
        StdOps[(int)Opcode.SBC_A_d8] = SBC;
        StdOps[(int)Opcode.RST_18H] = RST(0x18);
        StdOps[(int)Opcode.LDH_AT_a8_A] = LDH;
        StdOps[(int)Opcode.POP_HL] = POP(WideRegister.HL);
        StdOps[(int)Opcode.LD_AT_C_A] = LD_AT_C_A;
        StdOps[(int)Opcode.ILLEGAL_E3] = ILLEGAL_E3;
        StdOps[(int)Opcode.ILLEGAL_E4] = ILLEGAL_E4;
        StdOps[(int)Opcode.PUSH_HL] = PUSH(WideRegister.HL);
        StdOps[(int)Opcode.AND_d8] = AND;
        StdOps[(int)Opcode.RST_20H] = RST(0x20);
        StdOps[(int)Opcode.ADD_SP_r8] = ADD_SP_R8;
        StdOps[(int)Opcode.JP_HL] = JP;
        StdOps[(int)Opcode.LD_AT_a16_A] = LD_AT_a16_A;
        StdOps[(int)Opcode.ILLEGAL_EB] = ILLEGAL_EB;
        StdOps[(int)Opcode.ILLEGAL_EC] = ILLEGAL_EC;
        StdOps[(int)Opcode.ILLEGAL_ED] = ILLEGAL_ED;
        StdOps[(int)Opcode.XOR_d8] = XOR;
        StdOps[(int)Opcode.RST_28H] = RST(0x28);
        StdOps[(int)Opcode.LDH_A_AT_a8] = LDH_A_AT_a8;
        StdOps[(int)Opcode.POP_AF] = POP(WideRegister.AF);
        StdOps[(int)Opcode.LD_A_AT_C] = LD_A_AT_C;
        StdOps[(int)Opcode.DI] = DI;
        StdOps[(int)Opcode.ILLEGAL_F4] = ILLEGAL_F4;
        StdOps[(int)Opcode.PUSH_AF] = PUSH(WideRegister.AF);
        StdOps[(int)Opcode.OR_d8] = OR;
        StdOps[(int)Opcode.RST_30H] = RST(0x30);
        StdOps[(int)Opcode.LD_HL_SP_i8] = LD_HL_SP_i8;
        StdOps[(int)Opcode.LD_SP_HL] = LD_SP_HL;
        StdOps[(int)Opcode.LD_A_AT_a16] = LD_A16;
        StdOps[(int)Opcode.EI] = EI;
        StdOps[(int)Opcode.ILLEGAL_FC] = ILLEGAL_FC;
        StdOps[(int)Opcode.ILLEGAL_FD] = ILLEGAL_FD;
        StdOps[(int)Opcode.CP_d8] = CP;
        StdOps[(int)Opcode.RST_38H] = RST(0x38);

        CbOps = new Action[0x100];
        CbOps[(int)CBOpcode.RLC_B] = RLC(Register.B);
        CbOps[(int)CBOpcode.RLC_C] = RLC(Register.C);
        CbOps[(int)CBOpcode.RLC_D] = RLC(Register.D);
        CbOps[(int)CBOpcode.RLC_E] = RLC(Register.E);
        CbOps[(int)CBOpcode.RLC_H] = RLC(Register.H);
        CbOps[(int)CBOpcode.RLC_L] = RLC(Register.L);
        CbOps[(int)CBOpcode.RLC_AT_HL] = RLC(Register.HL);
        CbOps[(int)CBOpcode.RLC_A] = RLC(Register.A);
        CbOps[(int)CBOpcode.RRC_B] = RRC(Register.B);
        CbOps[(int)CBOpcode.RRC_C] = RRC(Register.C);
        CbOps[(int)CBOpcode.RRC_D] = RRC(Register.D);
        CbOps[(int)CBOpcode.RRC_E] = RRC(Register.E);
        CbOps[(int)CBOpcode.RRC_H] = RRC(Register.H);
        CbOps[(int)CBOpcode.RRC_L] = RRC(Register.L);
        CbOps[(int)CBOpcode.RRC_AT_HL] = RRC(Register.HL);
        CbOps[(int)CBOpcode.RRC_A] = RRC(Register.A);
        CbOps[(int)CBOpcode.RL_B] = RL(Register.B);
        CbOps[(int)CBOpcode.RL_C] = RL(Register.C);
        CbOps[(int)CBOpcode.RL_D] = RL(Register.D);
        CbOps[(int)CBOpcode.RL_E] = RL(Register.E);
        CbOps[(int)CBOpcode.RL_H] = RL(Register.H);
        CbOps[(int)CBOpcode.RL_L] = RL(Register.L);
        CbOps[(int)CBOpcode.RL_AT_HL] = RL(Register.HL);
        CbOps[(int)CBOpcode.RL_A] = RL(Register.A);
        CbOps[(int)CBOpcode.RR_B] = RR(Register.B);
        CbOps[(int)CBOpcode.RR_C] = RR(Register.C);
        CbOps[(int)CBOpcode.RR_D] = RR(Register.D);
        CbOps[(int)CBOpcode.RR_E] = RR(Register.E);
        CbOps[(int)CBOpcode.RR_H] = RR(Register.H);
        CbOps[(int)CBOpcode.RR_L] = RR(Register.L);
        CbOps[(int)CBOpcode.RR_AT_HL] = RR(Register.HL);
        CbOps[(int)CBOpcode.RR_A] = RR(Register.A);
        CbOps[(int)CBOpcode.SLA_B] = SLA(Register.B);
        CbOps[(int)CBOpcode.SLA_C] = SLA(Register.C);
        CbOps[(int)CBOpcode.SLA_D] = SLA(Register.D);
        CbOps[(int)CBOpcode.SLA_E] = SLA(Register.E);
        CbOps[(int)CBOpcode.SLA_H] = SLA(Register.H);
        CbOps[(int)CBOpcode.SLA_L] = SLA(Register.L);
        CbOps[(int)CBOpcode.SLA_AT_HL] = SLA(Register.HL);
        CbOps[(int)CBOpcode.SLA_A] = SLA(Register.A);
        CbOps[(int)CBOpcode.SRA_B] = SRA(Register.B);
        CbOps[(int)CBOpcode.SRA_C] = SRA(Register.C);
        CbOps[(int)CBOpcode.SRA_D] = SRA(Register.D);
        CbOps[(int)CBOpcode.SRA_E] = SRA(Register.E);
        CbOps[(int)CBOpcode.SRA_H] = SRA(Register.H);
        CbOps[(int)CBOpcode.SRA_L] = SRA(Register.L);
        CbOps[(int)CBOpcode.SRA_AT_HL] = SRA(Register.HL);
        CbOps[(int)CBOpcode.SRA_A] = SRA(Register.A);
        CbOps[(int)CBOpcode.SWAP_B] = SWAP(Register.B);
        CbOps[(int)CBOpcode.SWAP_C] = SWAP(Register.C);
        CbOps[(int)CBOpcode.SWAP_D] = SWAP(Register.D);
        CbOps[(int)CBOpcode.SWAP_E] = SWAP(Register.E);
        CbOps[(int)CBOpcode.SWAP_H] = SWAP(Register.H);
        CbOps[(int)CBOpcode.SWAP_L] = SWAP(Register.L);
        CbOps[(int)CBOpcode.SWAP_AT_HL] = SWAP(Register.HL);
        CbOps[(int)CBOpcode.SWAP_A] = SWAP(Register.A);
        CbOps[(int)CBOpcode.SRL_B] = SRL(Register.B);
        CbOps[(int)CBOpcode.SRL_C] = SRL(Register.C);
        CbOps[(int)CBOpcode.SRL_D] = SRL(Register.D);
        CbOps[(int)CBOpcode.SRL_E] = SRL(Register.E);
        CbOps[(int)CBOpcode.SRL_H] = SRL(Register.H);
        CbOps[(int)CBOpcode.SRL_L] = SRL(Register.L);
        CbOps[(int)CBOpcode.SRL_AT_HL] = SRL(Register.HL);
        CbOps[(int)CBOpcode.SRL_A] = SRL(Register.A);
        CbOps[(int)CBOpcode.BIT_0_B] = BIT(0, Register.B);
        CbOps[(int)CBOpcode.BIT_0_C] = BIT(0, Register.C);
        CbOps[(int)CBOpcode.BIT_0_D] = BIT(0, Register.D);
        CbOps[(int)CBOpcode.BIT_0_E] = BIT(0, Register.E);
        CbOps[(int)CBOpcode.BIT_0_H] = BIT(0, Register.H);
        CbOps[(int)CBOpcode.BIT_0_L] = BIT(0, Register.L);
        CbOps[(int)CBOpcode.BIT_0_AT_HL] = BIT(0, Register.HL);
        CbOps[(int)CBOpcode.BIT_0_A] = BIT(0, Register.A);
        CbOps[(int)CBOpcode.BIT_1_B] = BIT(1, Register.B);
        CbOps[(int)CBOpcode.BIT_1_C] = BIT(1, Register.C);
        CbOps[(int)CBOpcode.BIT_1_D] = BIT(1, Register.D);
        CbOps[(int)CBOpcode.BIT_1_E] = BIT(1, Register.E);
        CbOps[(int)CBOpcode.BIT_1_H] = BIT(1, Register.H);
        CbOps[(int)CBOpcode.BIT_1_L] = BIT(1, Register.L);
        CbOps[(int)CBOpcode.BIT_1_AT_HL] = BIT(1, Register.HL);
        CbOps[(int)CBOpcode.BIT_1_A] = BIT(1, Register.A);
        CbOps[(int)CBOpcode.BIT_2_B] = BIT(2, Register.B);
        CbOps[(int)CBOpcode.BIT_2_C] = BIT(2, Register.C);
        CbOps[(int)CBOpcode.BIT_2_D] = BIT(2, Register.D);
        CbOps[(int)CBOpcode.BIT_2_E] = BIT(2, Register.E);
        CbOps[(int)CBOpcode.BIT_2_H] = BIT(2, Register.H);
        CbOps[(int)CBOpcode.BIT_2_L] = BIT(2, Register.L);
        CbOps[(int)CBOpcode.BIT_2_AT_HL] = BIT(2, Register.HL);
        CbOps[(int)CBOpcode.BIT_2_A] = BIT(2, Register.A);
        CbOps[(int)CBOpcode.BIT_3_B] = BIT(3, Register.B);
        CbOps[(int)CBOpcode.BIT_3_C] = BIT(3, Register.C);
        CbOps[(int)CBOpcode.BIT_3_D] = BIT(3, Register.D);
        CbOps[(int)CBOpcode.BIT_3_E] = BIT(3, Register.E);
        CbOps[(int)CBOpcode.BIT_3_H] = BIT(3, Register.H);
        CbOps[(int)CBOpcode.BIT_3_L] = BIT(3, Register.L);
        CbOps[(int)CBOpcode.BIT_3_AT_HL] = BIT(3, Register.HL);
        CbOps[(int)CBOpcode.BIT_3_A] = BIT(3, Register.A);
        CbOps[(int)CBOpcode.BIT_4_B] = BIT(4, Register.B);
        CbOps[(int)CBOpcode.BIT_4_C] = BIT(4, Register.C);
        CbOps[(int)CBOpcode.BIT_4_D] = BIT(4, Register.D);
        CbOps[(int)CBOpcode.BIT_4_E] = BIT(4, Register.E);
        CbOps[(int)CBOpcode.BIT_4_H] = BIT(4, Register.H);
        CbOps[(int)CBOpcode.BIT_4_L] = BIT(4, Register.L);
        CbOps[(int)CBOpcode.BIT_4_AT_HL] = BIT(4, Register.HL);
        CbOps[(int)CBOpcode.BIT_4_A] = BIT(4, Register.A);
        CbOps[(int)CBOpcode.BIT_5_B] = BIT(5, Register.B);
        CbOps[(int)CBOpcode.BIT_5_C] = BIT(5, Register.C);
        CbOps[(int)CBOpcode.BIT_5_D] = BIT(5, Register.D);
        CbOps[(int)CBOpcode.BIT_5_E] = BIT(5, Register.E);
        CbOps[(int)CBOpcode.BIT_5_H] = BIT(5, Register.H);
        CbOps[(int)CBOpcode.BIT_5_L] = BIT(5, Register.L);
        CbOps[(int)CBOpcode.BIT_5_AT_HL] = BIT(5, Register.HL);
        CbOps[(int)CBOpcode.BIT_5_A] = BIT(5, Register.A);
        CbOps[(int)CBOpcode.BIT_6_B] = BIT(6, Register.B);
        CbOps[(int)CBOpcode.BIT_6_C] = BIT(6, Register.C);
        CbOps[(int)CBOpcode.BIT_6_D] = BIT(6, Register.D);
        CbOps[(int)CBOpcode.BIT_6_E] = BIT(6, Register.E);
        CbOps[(int)CBOpcode.BIT_6_H] = BIT(6, Register.H);
        CbOps[(int)CBOpcode.BIT_6_L] = BIT(6, Register.L);
        CbOps[(int)CBOpcode.BIT_6_AT_HL] = BIT(6, Register.HL);
        CbOps[(int)CBOpcode.BIT_6_A] = BIT(6, Register.A);
        CbOps[(int)CBOpcode.BIT_7_B] = BIT(7, Register.B);
        CbOps[(int)CBOpcode.BIT_7_C] = BIT(7, Register.C);
        CbOps[(int)CBOpcode.BIT_7_D] = BIT(7, Register.D);
        CbOps[(int)CBOpcode.BIT_7_E] = BIT(7, Register.E);
        CbOps[(int)CBOpcode.BIT_7_H] = BIT(7, Register.H);
        CbOps[(int)CBOpcode.BIT_7_L] = BIT(7, Register.L);
        CbOps[(int)CBOpcode.BIT_7_AT_HL] = BIT(7, Register.HL);
        CbOps[(int)CBOpcode.BIT_7_A] = BIT(7, Register.A);
        CbOps[(int)CBOpcode.RES_0_B] = RES(0, Register.B);
        CbOps[(int)CBOpcode.RES_0_C] = RES(0, Register.C);
        CbOps[(int)CBOpcode.RES_0_D] = RES(0, Register.D);
        CbOps[(int)CBOpcode.RES_0_E] = RES(0, Register.E);
        CbOps[(int)CBOpcode.RES_0_H] = RES(0, Register.H);
        CbOps[(int)CBOpcode.RES_0_L] = RES(0, Register.L);
        CbOps[(int)CBOpcode.RES_0_AT_HL] = RES(0, Register.HL);
        CbOps[(int)CBOpcode.RES_0_A] = RES(0, Register.A);
        CbOps[(int)CBOpcode.RES_1_B] = RES(1, Register.B);
        CbOps[(int)CBOpcode.RES_1_C] = RES(1, Register.C);
        CbOps[(int)CBOpcode.RES_1_D] = RES(1, Register.D);
        CbOps[(int)CBOpcode.RES_1_E] = RES(1, Register.E);
        CbOps[(int)CBOpcode.RES_1_H] = RES(1, Register.H);
        CbOps[(int)CBOpcode.RES_1_L] = RES(1, Register.L);
        CbOps[(int)CBOpcode.RES_1_AT_HL] = RES(1, Register.HL);
        CbOps[(int)CBOpcode.RES_1_A] = RES(1, Register.A);
        CbOps[(int)CBOpcode.RES_2_B] = RES(2, Register.B);
        CbOps[(int)CBOpcode.RES_2_C] = RES(2, Register.C);
        CbOps[(int)CBOpcode.RES_2_D] = RES(2, Register.D);
        CbOps[(int)CBOpcode.RES_2_E] = RES(2, Register.E);
        CbOps[(int)CBOpcode.RES_2_H] = RES(2, Register.H);
        CbOps[(int)CBOpcode.RES_2_L] = RES(2, Register.L);
        CbOps[(int)CBOpcode.RES_2_AT_HL] = RES(2, Register.HL);
        CbOps[(int)CBOpcode.RES_2_A] = RES(2, Register.A);
        CbOps[(int)CBOpcode.RES_3_B] = RES(3, Register.B);
        CbOps[(int)CBOpcode.RES_3_C] = RES(3, Register.C);
        CbOps[(int)CBOpcode.RES_3_D] = RES(3, Register.D);
        CbOps[(int)CBOpcode.RES_3_E] = RES(3, Register.E);
        CbOps[(int)CBOpcode.RES_3_H] = RES(3, Register.H);
        CbOps[(int)CBOpcode.RES_3_L] = RES(3, Register.L);
        CbOps[(int)CBOpcode.RES_3_AT_HL] = RES(3, Register.HL);
        CbOps[(int)CBOpcode.RES_3_A] = RES(3, Register.A);
        CbOps[(int)CBOpcode.RES_4_B] = RES(4, Register.B);
        CbOps[(int)CBOpcode.RES_4_C] = RES(4, Register.C);
        CbOps[(int)CBOpcode.RES_4_D] = RES(4, Register.D);
        CbOps[(int)CBOpcode.RES_4_E] = RES(4, Register.E);
        CbOps[(int)CBOpcode.RES_4_H] = RES(4, Register.H);
        CbOps[(int)CBOpcode.RES_4_L] = RES(4, Register.L);
        CbOps[(int)CBOpcode.RES_4_AT_HL] = RES(4, Register.HL);
        CbOps[(int)CBOpcode.RES_4_A] = RES(4, Register.A);
        CbOps[(int)CBOpcode.RES_5_B] = RES(5, Register.B);
        CbOps[(int)CBOpcode.RES_5_C] = RES(5, Register.C);
        CbOps[(int)CBOpcode.RES_5_D] = RES(5, Register.D);
        CbOps[(int)CBOpcode.RES_5_E] = RES(5, Register.E);
        CbOps[(int)CBOpcode.RES_5_H] = RES(5, Register.H);
        CbOps[(int)CBOpcode.RES_5_L] = RES(5, Register.L);
        CbOps[(int)CBOpcode.RES_5_AT_HL] = RES(5, Register.HL);
        CbOps[(int)CBOpcode.RES_5_A] = RES(5, Register.A);
        CbOps[(int)CBOpcode.RES_6_B] = RES(6, Register.B);
        CbOps[(int)CBOpcode.RES_6_C] = RES(6, Register.C);
        CbOps[(int)CBOpcode.RES_6_D] = RES(6, Register.D);
        CbOps[(int)CBOpcode.RES_6_E] = RES(6, Register.E);
        CbOps[(int)CBOpcode.RES_6_H] = RES(6, Register.H);
        CbOps[(int)CBOpcode.RES_6_L] = RES(6, Register.L);
        CbOps[(int)CBOpcode.RES_6_AT_HL] = RES(6, Register.HL);
        CbOps[(int)CBOpcode.RES_6_A] = RES(6, Register.A);
        CbOps[(int)CBOpcode.RES_7_B] = RES(7, Register.B);
        CbOps[(int)CBOpcode.RES_7_C] = RES(7, Register.C);
        CbOps[(int)CBOpcode.RES_7_D] = RES(7, Register.D);
        CbOps[(int)CBOpcode.RES_7_E] = RES(7, Register.E);
        CbOps[(int)CBOpcode.RES_7_H] = RES(7, Register.H);
        CbOps[(int)CBOpcode.RES_7_L] = RES(7, Register.L);
        CbOps[(int)CBOpcode.RES_7_AT_HL] = RES(7, Register.HL);
        CbOps[(int)CBOpcode.RES_7_A] = RES(7, Register.A);
        CbOps[(int)CBOpcode.SET_0_B] = SET(0, Register.B);
        CbOps[(int)CBOpcode.SET_0_C] = SET(0, Register.C);
        CbOps[(int)CBOpcode.SET_0_D] = SET(0, Register.D);
        CbOps[(int)CBOpcode.SET_0_E] = SET(0, Register.E);
        CbOps[(int)CBOpcode.SET_0_H] = SET(0, Register.H);
        CbOps[(int)CBOpcode.SET_0_L] = SET(0, Register.L);
        CbOps[(int)CBOpcode.SET_0_AT_HL] = SET(0, Register.HL);
        CbOps[(int)CBOpcode.SET_0_A] = SET(0, Register.A);
        CbOps[(int)CBOpcode.SET_1_B] = SET(1, Register.B);
        CbOps[(int)CBOpcode.SET_1_C] = SET(1, Register.C);
        CbOps[(int)CBOpcode.SET_1_D] = SET(1, Register.D);
        CbOps[(int)CBOpcode.SET_1_E] = SET(1, Register.E);
        CbOps[(int)CBOpcode.SET_1_H] = SET(1, Register.H);
        CbOps[(int)CBOpcode.SET_1_L] = SET(1, Register.L);
        CbOps[(int)CBOpcode.SET_1_AT_HL] = SET(1, Register.HL);
        CbOps[(int)CBOpcode.SET_1_A] = SET(1, Register.A);
        CbOps[(int)CBOpcode.SET_2_B] = SET(2, Register.B);
        CbOps[(int)CBOpcode.SET_2_C] = SET(2, Register.C);
        CbOps[(int)CBOpcode.SET_2_D] = SET(2, Register.D);
        CbOps[(int)CBOpcode.SET_2_E] = SET(2, Register.E);
        CbOps[(int)CBOpcode.SET_2_H] = SET(2, Register.H);
        CbOps[(int)CBOpcode.SET_2_L] = SET(2, Register.L);
        CbOps[(int)CBOpcode.SET_2_AT_HL] = SET(2, Register.HL);
        CbOps[(int)CBOpcode.SET_2_A] = SET(2, Register.A);
        CbOps[(int)CBOpcode.SET_3_B] = SET(3, Register.B);
        CbOps[(int)CBOpcode.SET_3_C] = SET(3, Register.C);
        CbOps[(int)CBOpcode.SET_3_D] = SET(3, Register.D);
        CbOps[(int)CBOpcode.SET_3_E] = SET(3, Register.E);
        CbOps[(int)CBOpcode.SET_3_H] = SET(3, Register.H);
        CbOps[(int)CBOpcode.SET_3_L] = SET(3, Register.L);
        CbOps[(int)CBOpcode.SET_3_AT_HL] = SET(3, Register.HL);
        CbOps[(int)CBOpcode.SET_3_A] = SET(3, Register.A);
        CbOps[(int)CBOpcode.SET_4_B] = SET(4, Register.B);
        CbOps[(int)CBOpcode.SET_4_C] = SET(4, Register.C);
        CbOps[(int)CBOpcode.SET_4_D] = SET(4, Register.D);
        CbOps[(int)CBOpcode.SET_4_E] = SET(4, Register.E);
        CbOps[(int)CBOpcode.SET_4_H] = SET(4, Register.H);
        CbOps[(int)CBOpcode.SET_4_L] = SET(4, Register.L);
        CbOps[(int)CBOpcode.SET_4_AT_HL] = SET(4, Register.HL);
        CbOps[(int)CBOpcode.SET_4_A] = SET(4, Register.A);
        CbOps[(int)CBOpcode.SET_5_B] = SET(5, Register.B);
        CbOps[(int)CBOpcode.SET_5_C] = SET(5, Register.C);
        CbOps[(int)CBOpcode.SET_5_D] = SET(5, Register.D);
        CbOps[(int)CBOpcode.SET_5_E] = SET(5, Register.E);
        CbOps[(int)CBOpcode.SET_5_H] = SET(5, Register.H);
        CbOps[(int)CBOpcode.SET_5_L] = SET(5, Register.L);
        CbOps[(int)CBOpcode.SET_5_AT_HL] = SET(5, Register.HL);
        CbOps[(int)CBOpcode.SET_5_A] = SET(5, Register.A);
        CbOps[(int)CBOpcode.SET_6_B] = SET(6, Register.B);
        CbOps[(int)CBOpcode.SET_6_C] = SET(6, Register.C);
        CbOps[(int)CBOpcode.SET_6_D] = SET(6, Register.D);
        CbOps[(int)CBOpcode.SET_6_E] = SET(6, Register.E);
        CbOps[(int)CBOpcode.SET_6_H] = SET(6, Register.H);
        CbOps[(int)CBOpcode.SET_6_L] = SET(6, Register.L);
        CbOps[(int)CBOpcode.SET_6_AT_HL] = SET(6, Register.HL);
        CbOps[(int)CBOpcode.SET_6_A] = SET(6, Register.A);
        CbOps[(int)CBOpcode.SET_7_B] = SET(7, Register.B);
        CbOps[(int)CBOpcode.SET_7_C] = SET(7, Register.C);
        CbOps[(int)CBOpcode.SET_7_D] = SET(7, Register.D);
        CbOps[(int)CBOpcode.SET_7_E] = SET(7, Register.E);
        CbOps[(int)CBOpcode.SET_7_H] = SET(7, Register.H);
        CbOps[(int)CBOpcode.SET_7_L] = SET(7, Register.L);
        CbOps[(int)CBOpcode.SET_7_AT_HL] = SET(7, Register.HL);
        CbOps[(int)CBOpcode.SET_7_A] = SET(7, Register.A);

        Registers = new Registers();
    }

    private byte ReadHaltBug()
    {
        Halted = HaltState.off;
        CycleElapsed();
        return Memory[PC];
    }
    internal void DoNextOP()
    {
        if (Halted != HaltState.off)
        {
            if (Halted != HaltState.haltbug)
            {
                CycleElapsed();
                return;
            }
        }

        var op = Halted == HaltState.haltbug ? ReadHaltBug() : ReadInput();
        if (op != 0xcb)
        {
            Op((Opcode)op)();
        }
        else
        {
            var CBop = ReadInput(); //Because of the CB prefix we encountered in the previous case we already skipped the extra byte of a cb instruction here
            Op((CBOpcode)CBop)();
        }
    }

    private long OurCycles;

    private void CycleElapsed()
    {
        OurCycles++;
        Cycle();
    }

    public Action Cycle = () => { };

    internal void Step()
    {
        var didInterrupt = DoInterrupt();
        ISR.EnableInterruptsIfScheduled();

        //If we did not do an interrupt we can do a normal CPU instruction instead
        if (!didInterrupt)
            DoNextOP();
    }
}
