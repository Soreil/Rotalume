using System;

namespace generator
{

    public struct Operand
    {
        public string Name;
        public int? Size;
        public bool Increment;
        public bool Decrement;
        public bool Pointer;

        public Operand(string name, bool pointer)
        {
            Name = name;
            Size = null;
            Increment = false;
            Decrement = false;
            Pointer = pointer;
        }

        internal string MakeOperandArgumentValue()
        {
            string valueOfOperand = Name switch
            {
                "A" => "Register.A",
                "B" => "Register.B",
                "C" => "Register.C",
                "D" => "Register.D",
                "E" => "Register.E",
                "F" => "Register.F",
                "H" => "Register.H",
                "L" => "Register.L",

                "AF" => "WideRegister.AF",
                "BC" => "WideRegister.BC",
                "DE" => "WideRegister.DE",
                "HL" => "WideRegister.HL",
                "SP" => "WideRegister.SP",

                "d16" => "0",
                "a16" => "0",

                "d8" => "0",
                "r8" => "0",

                "Zero" => "Flag.Z",
                "Negative" => "Flag.M",
                "HalfCarry" => "Flag.H",
                "Carry" => "Flag.C",

                "NZ" => "Flag.NZ",
                "NN" => "Flag.NN",
                "NH" => "Flag.NH",
                "NC" => "Flag.NC",

                _ => "0"
            };

            return valueOfOperand;
        }

        internal string MakeOperandArgumentType()
        {
            string typeOfOperand = Name switch
            {
                "A" => "Register",
                "B" => "Register",
                "C" => "Register",
                "D" => "Register",
                "E" => "Register",
                "F" => "Register",
                "H" => "Register",
                "L" => "Register",

                "AF" => "WideRegister",
                "BC" => "WideRegister",
                "DE" => "WideRegister",
                "HL" => "WideRegister",
                "SP" => "WideRegister",

                "d16" => "ushort",
                "a16" => "ushort",

                "d8" => "byte",
                "r8" => "sbyte",

                "Zero" => "Flag",
                "Negative" => "Flag",
                "HalfCarry" => "Flag",
                "Carry" => "Flag",

                "NZ" => "Flag",
                "NN" => "Flag",
                "NH" => "Flag",
                "NC" => "Flag",

                _ => "byte"
            };

            return typeOfOperand;
        }
    }


public enum unprefixed : byte
	{
		NOP = 0x00,
		LD_BC_d16 = 0x01,
		LD_AT_BC_A = 0x02,
		INC_BC = 0x03,
		INC_B = 0x04,
		DEC_B = 0x05,
		LD_B_d8 = 0x06,
		RLCA = 0x07,
		LD_AT_a16_SP = 0x08,
		ADD_HL_BC = 0x09,
		LD_A_AT_BC = 0x0A,
		DEC_BC = 0x0B,
		INC_C = 0x0C,
		DEC_C = 0x0D,
		LD_C_d8 = 0x0E,
		RRCA = 0x0F,
		STOP = 0x10,
		LD_DE_d16 = 0x11,
		LD_AT_DE_A = 0x12,
		INC_DE = 0x13,
		INC_D = 0x14,
		DEC_D = 0x15,
		LD_D_d8 = 0x16,
		RLA = 0x17,
		JR_r8 = 0x18,
		ADD_HL_DE = 0x19,
		LD_A_AT_DE = 0x1A,
		DEC_DE = 0x1B,
		INC_E = 0x1C,
		DEC_E = 0x1D,
		LD_E_d8 = 0x1E,
		RRA = 0x1F,
		JR_NZ_r8 = 0x20,
		LD_HL_d16 = 0x21,
		LDI_AT_HL_A = 0x22,
		INC_HL = 0x23,
		INC_H = 0x24,
		DEC_H = 0x25,
		LD_H_d8 = 0x26,
		DAA = 0x27,
		JR_Zero_r8 = 0x28,
		ADD_HL_HL = 0x29,
		LD_AI_AT_HL = 0x2A,
		DEC_HL = 0x2B,
		INC_L = 0x2C,
		DEC_L = 0x2D,
		LD_L_d8 = 0x2E,
		CPL = 0x2F,
		JR_NC_r8 = 0x30,
		LD_SP_d16 = 0x31,
		LDD_AT_HL_A = 0x32,
		INC_SP = 0x33,
		INC_AT_HL = 0x34,
		DEC_AT_HL = 0x35,
		LD_AT_HL_d8 = 0x36,
		SCF = 0x37,
		JR_Carry_r8 = 0x38,
		ADD_HL_SP = 0x39,
		LD_AD_AT_HL = 0x3A,
		DEC_SP = 0x3B,
		INC_A = 0x3C,
		DEC_A = 0x3D,
		LD_A_d8 = 0x3E,
		CCF = 0x3F,
		LD_B_B = 0x40,
		LD_B_C = 0x41,
		LD_B_D = 0x42,
		LD_B_E = 0x43,
		LD_B_H = 0x44,
		LD_B_L = 0x45,
		LD_B_AT_HL = 0x46,
		LD_B_A = 0x47,
		LD_C_B = 0x48,
		LD_C_C = 0x49,
		LD_C_D = 0x4A,
		LD_C_E = 0x4B,
		LD_C_H = 0x4C,
		LD_C_L = 0x4D,
		LD_C_AT_HL = 0x4E,
		LD_C_A = 0x4F,
		LD_D_B = 0x50,
		LD_D_C = 0x51,
		LD_D_D = 0x52,
		LD_D_E = 0x53,
		LD_D_H = 0x54,
		LD_D_L = 0x55,
		LD_D_AT_HL = 0x56,
		LD_D_A = 0x57,
		LD_E_B = 0x58,
		LD_E_C = 0x59,
		LD_E_D = 0x5A,
		LD_E_E = 0x5B,
		LD_E_H = 0x5C,
		LD_E_L = 0x5D,
		LD_E_AT_HL = 0x5E,
		LD_E_A = 0x5F,
		LD_H_B = 0x60,
		LD_H_C = 0x61,
		LD_H_D = 0x62,
		LD_H_E = 0x63,
		LD_H_H = 0x64,
		LD_H_L = 0x65,
		LD_H_AT_HL = 0x66,
		LD_H_A = 0x67,
		LD_L_B = 0x68,
		LD_L_C = 0x69,
		LD_L_D = 0x6A,
		LD_L_E = 0x6B,
		LD_L_H = 0x6C,
		LD_L_L = 0x6D,
		LD_L_AT_HL = 0x6E,
		LD_L_A = 0x6F,
		LD_AT_HL_B = 0x70,
		LD_AT_HL_C = 0x71,
		LD_AT_HL_D = 0x72,
		LD_AT_HL_E = 0x73,
		LD_AT_HL_H = 0x74,
		LD_AT_HL_L = 0x75,
		HALT = 0x76,
		LD_AT_HL_A = 0x77,
		LD_A_B = 0x78,
		LD_A_C = 0x79,
		LD_A_D = 0x7A,
		LD_A_E = 0x7B,
		LD_A_H = 0x7C,
		LD_A_L = 0x7D,
		LD_A_AT_HL = 0x7E,
		LD_A_A = 0x7F,
		ADD_A_B = 0x80,
		ADD_A_C = 0x81,
		ADD_A_D = 0x82,
		ADD_A_E = 0x83,
		ADD_A_H = 0x84,
		ADD_A_L = 0x85,
		ADD_A_AT_HL = 0x86,
		ADD_A_A = 0x87,
		ADC_A_B = 0x88,
		ADC_A_C = 0x89,
		ADC_A_D = 0x8A,
		ADC_A_E = 0x8B,
		ADC_A_H = 0x8C,
		ADC_A_L = 0x8D,
		ADC_A_AT_HL = 0x8E,
		ADC_A_A = 0x8F,
		SUB_B = 0x90,
		SUB_C = 0x91,
		SUB_D = 0x92,
		SUB_E = 0x93,
		SUB_H = 0x94,
		SUB_L = 0x95,
		SUB_AT_HL = 0x96,
		SUB_A = 0x97,
		SBC_A_B = 0x98,
		SBC_A_C = 0x99,
		SBC_A_D = 0x9A,
		SBC_A_E = 0x9B,
		SBC_A_H = 0x9C,
		SBC_A_L = 0x9D,
		SBC_A_AT_HL = 0x9E,
		SBC_A_A = 0x9F,
		AND_B = 0xA0,
		AND_C = 0xA1,
		AND_D = 0xA2,
		AND_E = 0xA3,
		AND_H = 0xA4,
		AND_L = 0xA5,
		AND_AT_HL = 0xA6,
		AND_A = 0xA7,
		XOR_B = 0xA8,
		XOR_C = 0xA9,
		XOR_D = 0xAA,
		XOR_E = 0xAB,
		XOR_H = 0xAC,
		XOR_L = 0xAD,
		XOR_AT_HL = 0xAE,
		XOR_A = 0xAF,
		OR_B = 0xB0,
		OR_C = 0xB1,
		OR_D = 0xB2,
		OR_E = 0xB3,
		OR_H = 0xB4,
		OR_L = 0xB5,
		OR_AT_HL = 0xB6,
		OR_A = 0xB7,
		CP_B = 0xB8,
		CP_C = 0xB9,
		CP_D = 0xBA,
		CP_E = 0xBB,
		CP_H = 0xBC,
		CP_L = 0xBD,
		CP_AT_HL = 0xBE,
		CP_A = 0xBF,
		RET_NZ = 0xC0,
		POP_BC = 0xC1,
		JP_NZ_a16 = 0xC2,
		JP_a16 = 0xC3,
		CALL_NZ_a16 = 0xC4,
		PUSH_BC = 0xC5,
		ADD_A_d8 = 0xC6,
		RST_00H = 0xC7,
		RET_Zero = 0xC8,
		RET = 0xC9,
		JP_Zero_a16 = 0xCA,
		PREFIX = 0xCB,
		CALL_Zero_a16 = 0xCC,
		CALL_a16 = 0xCD,
		ADC_A_d8 = 0xCE,
		RST_08H = 0xCF,
		RET_NC = 0xD0,
		POP_DE = 0xD1,
		JP_NC_a16 = 0xD2,
		ILLEGAL_D3 = 0xD3,
		CALL_NC_a16 = 0xD4,
		PUSH_DE = 0xD5,
		SUB_d8 = 0xD6,
		RST_10H = 0xD7,
		RET_Carry = 0xD8,
		RETI = 0xD9,
		JP_Carry_a16 = 0xDA,
		ILLEGAL_DB = 0xDB,
		CALL_Carry_a16 = 0xDC,
		ILLEGAL_DD = 0xDD,
		SBC_A_d8 = 0xDE,
		RST_18H = 0xDF,
		LDH_AT_a8_A = 0xE0,
		POP_HL = 0xE1,
		LD_AT_C_A = 0xE2,
		ILLEGAL_E3 = 0xE3,
		ILLEGAL_E4 = 0xE4,
		PUSH_HL = 0xE5,
		AND_d8 = 0xE6,
		RST_20H = 0xE7,
		ADD_SP_r8 = 0xE8,
		JP_HL = 0xE9,
		LD_AT_a16_A = 0xEA,
		ILLEGAL_EB = 0xEB,
		ILLEGAL_EC = 0xEC,
		ILLEGAL_ED = 0xED,
		XOR_d8 = 0xEE,
		RST_28H = 0xEF,
		LDH_A_AT_a8 = 0xF0,
		POP_AF = 0xF1,
		LD_A_AT_C = 0xF2,
		DI = 0xF3,
		ILLEGAL_F4 = 0xF4,
		PUSH_AF = 0xF5,
		OR_d8 = 0xF6,
		RST_30H = 0xF7,
		LD_HLI_SP = 0xF8,
		LD_SP_HL = 0xF9,
		LD_A_AT_a16 = 0xFA,
		EI = 0xFB,
		ILLEGAL_FC = 0xFC,
		ILLEGAL_FD = 0xFD,
		CP_d8 = 0xFE,
		RST_38H = 0xFF,
	};
	public enum cbprefixed : byte
	{
		RLC_B = 0x00,
		RLC_C = 0x01,
		RLC_D = 0x02,
		RLC_E = 0x03,
		RLC_H = 0x04,
		RLC_L = 0x05,
		RLC_AT_HL = 0x06,
		RLC_A = 0x07,
		RRC_B = 0x08,
		RRC_C = 0x09,
		RRC_D = 0x0A,
		RRC_E = 0x0B,
		RRC_H = 0x0C,
		RRC_L = 0x0D,
		RRC_AT_HL = 0x0E,
		RRC_A = 0x0F,
		RL_B = 0x10,
		RL_C = 0x11,
		RL_D = 0x12,
		RL_E = 0x13,
		RL_H = 0x14,
		RL_L = 0x15,
		RL_AT_HL = 0x16,
		RL_A = 0x17,
		RR_B = 0x18,
		RR_C = 0x19,
		RR_D = 0x1A,
		RR_E = 0x1B,
		RR_H = 0x1C,
		RR_L = 0x1D,
		RR_AT_HL = 0x1E,
		RR_A = 0x1F,
		SLA_B = 0x20,
		SLA_C = 0x21,
		SLA_D = 0x22,
		SLA_E = 0x23,
		SLA_H = 0x24,
		SLA_L = 0x25,
		SLA_AT_HL = 0x26,
		SLA_A = 0x27,
		SRA_B = 0x28,
		SRA_C = 0x29,
		SRA_D = 0x2A,
		SRA_E = 0x2B,
		SRA_H = 0x2C,
		SRA_L = 0x2D,
		SRA_AT_HL = 0x2E,
		SRA_A = 0x2F,
		SWAP_B = 0x30,
		SWAP_C = 0x31,
		SWAP_D = 0x32,
		SWAP_E = 0x33,
		SWAP_H = 0x34,
		SWAP_L = 0x35,
		SWAP_AT_HL = 0x36,
		SWAP_A = 0x37,
		SRL_B = 0x38,
		SRL_C = 0x39,
		SRL_D = 0x3A,
		SRL_E = 0x3B,
		SRL_H = 0x3C,
		SRL_L = 0x3D,
		SRL_AT_HL = 0x3E,
		SRL_A = 0x3F,
		BIT_0_B = 0x40,
		BIT_0_C = 0x41,
		BIT_0_D = 0x42,
		BIT_0_E = 0x43,
		BIT_0_H = 0x44,
		BIT_0_L = 0x45,
		BIT_0_AT_HL = 0x46,
		BIT_0_A = 0x47,
		BIT_1_B = 0x48,
		BIT_1_C = 0x49,
		BIT_1_D = 0x4A,
		BIT_1_E = 0x4B,
		BIT_1_H = 0x4C,
		BIT_1_L = 0x4D,
		BIT_1_AT_HL = 0x4E,
		BIT_1_A = 0x4F,
		BIT_2_B = 0x50,
		BIT_2_C = 0x51,
		BIT_2_D = 0x52,
		BIT_2_E = 0x53,
		BIT_2_H = 0x54,
		BIT_2_L = 0x55,
		BIT_2_AT_HL = 0x56,
		BIT_2_A = 0x57,
		BIT_3_B = 0x58,
		BIT_3_C = 0x59,
		BIT_3_D = 0x5A,
		BIT_3_E = 0x5B,
		BIT_3_H = 0x5C,
		BIT_3_L = 0x5D,
		BIT_3_AT_HL = 0x5E,
		BIT_3_A = 0x5F,
		BIT_4_B = 0x60,
		BIT_4_C = 0x61,
		BIT_4_D = 0x62,
		BIT_4_E = 0x63,
		BIT_4_H = 0x64,
		BIT_4_L = 0x65,
		BIT_4_AT_HL = 0x66,
		BIT_4_A = 0x67,
		BIT_5_B = 0x68,
		BIT_5_C = 0x69,
		BIT_5_D = 0x6A,
		BIT_5_E = 0x6B,
		BIT_5_H = 0x6C,
		BIT_5_L = 0x6D,
		BIT_5_AT_HL = 0x6E,
		BIT_5_A = 0x6F,
		BIT_6_B = 0x70,
		BIT_6_C = 0x71,
		BIT_6_D = 0x72,
		BIT_6_E = 0x73,
		BIT_6_H = 0x74,
		BIT_6_L = 0x75,
		BIT_6_AT_HL = 0x76,
		BIT_6_A = 0x77,
		BIT_7_B = 0x78,
		BIT_7_C = 0x79,
		BIT_7_D = 0x7A,
		BIT_7_E = 0x7B,
		BIT_7_H = 0x7C,
		BIT_7_L = 0x7D,
		BIT_7_AT_HL = 0x7E,
		BIT_7_A = 0x7F,
		RES_0_B = 0x80,
		RES_0_C = 0x81,
		RES_0_D = 0x82,
		RES_0_E = 0x83,
		RES_0_H = 0x84,
		RES_0_L = 0x85,
		RES_0_AT_HL = 0x86,
		RES_0_A = 0x87,
		RES_1_B = 0x88,
		RES_1_C = 0x89,
		RES_1_D = 0x8A,
		RES_1_E = 0x8B,
		RES_1_H = 0x8C,
		RES_1_L = 0x8D,
		RES_1_AT_HL = 0x8E,
		RES_1_A = 0x8F,
		RES_2_B = 0x90,
		RES_2_C = 0x91,
		RES_2_D = 0x92,
		RES_2_E = 0x93,
		RES_2_H = 0x94,
		RES_2_L = 0x95,
		RES_2_AT_HL = 0x96,
		RES_2_A = 0x97,
		RES_3_B = 0x98,
		RES_3_C = 0x99,
		RES_3_D = 0x9A,
		RES_3_E = 0x9B,
		RES_3_H = 0x9C,
		RES_3_L = 0x9D,
		RES_3_AT_HL = 0x9E,
		RES_3_A = 0x9F,
		RES_4_B = 0xA0,
		RES_4_C = 0xA1,
		RES_4_D = 0xA2,
		RES_4_E = 0xA3,
		RES_4_H = 0xA4,
		RES_4_L = 0xA5,
		RES_4_AT_HL = 0xA6,
		RES_4_A = 0xA7,
		RES_5_B = 0xA8,
		RES_5_C = 0xA9,
		RES_5_D = 0xAA,
		RES_5_E = 0xAB,
		RES_5_H = 0xAC,
		RES_5_L = 0xAD,
		RES_5_AT_HL = 0xAE,
		RES_5_A = 0xAF,
		RES_6_B = 0xB0,
		RES_6_C = 0xB1,
		RES_6_D = 0xB2,
		RES_6_E = 0xB3,
		RES_6_H = 0xB4,
		RES_6_L = 0xB5,
		RES_6_AT_HL = 0xB6,
		RES_6_A = 0xB7,
		RES_7_B = 0xB8,
		RES_7_C = 0xB9,
		RES_7_D = 0xBA,
		RES_7_E = 0xBB,
		RES_7_H = 0xBC,
		RES_7_L = 0xBD,
		RES_7_AT_HL = 0xBE,
		RES_7_A = 0xBF,
		SET_0_B = 0xC0,
		SET_0_C = 0xC1,
		SET_0_D = 0xC2,
		SET_0_E = 0xC3,
		SET_0_H = 0xC4,
		SET_0_L = 0xC5,
		SET_0_AT_HL = 0xC6,
		SET_0_A = 0xC7,
		SET_1_B = 0xC8,
		SET_1_C = 0xC9,
		SET_1_D = 0xCA,
		SET_1_E = 0xCB,
		SET_1_H = 0xCC,
		SET_1_L = 0xCD,
		SET_1_AT_HL = 0xCE,
		SET_1_A = 0xCF,
		SET_2_B = 0xD0,
		SET_2_C = 0xD1,
		SET_2_D = 0xD2,
		SET_2_E = 0xD3,
		SET_2_H = 0xD4,
		SET_2_L = 0xD5,
		SET_2_AT_HL = 0xD6,
		SET_2_A = 0xD7,
		SET_3_B = 0xD8,
		SET_3_C = 0xD9,
		SET_3_D = 0xDA,
		SET_3_E = 0xDB,
		SET_3_H = 0xDC,
		SET_3_L = 0xDD,
		SET_3_AT_HL = 0xDE,
		SET_3_A = 0xDF,
		SET_4_B = 0xE0,
		SET_4_C = 0xE1,
		SET_4_D = 0xE2,
		SET_4_E = 0xE3,
		SET_4_H = 0xE4,
		SET_4_L = 0xE5,
		SET_4_AT_HL = 0xE6,
		SET_4_A = 0xE7,
		SET_5_B = 0xE8,
		SET_5_C = 0xE9,
		SET_5_D = 0xEA,
		SET_5_E = 0xEB,
		SET_5_H = 0xEC,
		SET_5_L = 0xED,
		SET_5_AT_HL = 0xEE,
		SET_5_A = 0xEF,
		SET_6_B = 0xF0,
		SET_6_C = 0xF1,
		SET_6_D = 0xF2,
		SET_6_E = 0xF3,
		SET_6_H = 0xF4,
		SET_6_L = 0xF5,
		SET_6_AT_HL = 0xF6,
		SET_6_A = 0xF7,
		SET_7_B = 0xF8,
		SET_7_C = 0xF9,
		SET_7_D = 0xFA,
		SET_7_E = 0xFB,
		SET_7_H = 0xFC,
		SET_7_L = 0xFD,
		SET_7_AT_HL = 0xFE,
		SET_7_A = 0xFF,
	};



	public static class Decoder
    {

public static void Exec(unprefixed o)
		{
			switch (o)
			{
				case unprefixed.NOP: NOP(); break;
				case unprefixed.LD_BC_d16: LD(WideRegister.BC, 0); break;
				case unprefixed.LD_AT_BC_A: LD(WideRegister.BC, Register.A); break;
				case unprefixed.INC_BC: INC(WideRegister.BC); break;
				case unprefixed.INC_B: INC(Register.B); break;
				case unprefixed.DEC_B: DEC(Register.B); break;
				case unprefixed.LD_B_d8: LD(Register.B, 0); break;
				case unprefixed.RLCA: RLCA(); break;
				case unprefixed.LD_AT_a16_SP: LD(0, WideRegister.SP); break;
				case unprefixed.ADD_HL_BC: ADD(WideRegister.HL, WideRegister.BC); break;
				case unprefixed.LD_A_AT_BC: LD(Register.A, WideRegister.BC); break;
				case unprefixed.DEC_BC: DEC(WideRegister.BC); break;
				case unprefixed.INC_C: INC(Register.C); break;
				case unprefixed.DEC_C: DEC(Register.C); break;
				case unprefixed.LD_C_d8: LD(Register.C, 0); break;
				case unprefixed.RRCA: RRCA(); break;
				case unprefixed.STOP: STOP(); break;
				case unprefixed.LD_DE_d16: LD(WideRegister.DE, 0); break;
				case unprefixed.LD_AT_DE_A: LD(WideRegister.DE, Register.A); break;
				case unprefixed.INC_DE: INC(WideRegister.DE); break;
				case unprefixed.INC_D: INC(Register.D); break;
				case unprefixed.DEC_D: DEC(Register.D); break;
				case unprefixed.LD_D_d8: LD(Register.D, 0); break;
				case unprefixed.RLA: RLA(); break;
				case unprefixed.JR_r8: JR(0); break;
				case unprefixed.ADD_HL_DE: ADD(WideRegister.HL, WideRegister.DE); break;
				case unprefixed.LD_A_AT_DE: LD(Register.A, WideRegister.DE); break;
				case unprefixed.DEC_DE: DEC(WideRegister.DE); break;
				case unprefixed.INC_E: INC(Register.E); break;
				case unprefixed.DEC_E: DEC(Register.E); break;
				case unprefixed.LD_E_d8: LD(Register.E, 0); break;
				case unprefixed.RRA: RRA(); break;
				case unprefixed.JR_NZ_r8: JR(Flag.NZ, 0); break;
				case unprefixed.LD_HL_d16: LD(WideRegister.HL, 0); break;
				case unprefixed.LDI_AT_HL_A: LD(WideRegister.HL, Register.A); break;
				case unprefixed.INC_HL: INC(WideRegister.HL); break;
				case unprefixed.INC_H: INC(Register.H); break;
				case unprefixed.DEC_H: DEC(Register.H); break;
				case unprefixed.LD_H_d8: LD(Register.H, 0); break;
				case unprefixed.DAA: DAA(); break;
				case unprefixed.JR_Zero_r8: JR(Flag.Z, 0); break;
				case unprefixed.ADD_HL_HL: ADD(WideRegister.HL, WideRegister.HL); break;
				case unprefixed.LD_AI_AT_HL: LD(Register.A, WideRegister.HL); break;
				case unprefixed.DEC_HL: DEC(WideRegister.HL); break;
				case unprefixed.INC_L: INC(Register.L); break;
				case unprefixed.DEC_L: DEC(Register.L); break;
				case unprefixed.LD_L_d8: LD(Register.L, 0); break;
				case unprefixed.CPL: CPL(); break;
				case unprefixed.JR_NC_r8: JR(Flag.NC, 0); break;
				case unprefixed.LD_SP_d16: LD(WideRegister.SP, 0); break;
				case unprefixed.LDD_AT_HL_A: LD(WideRegister.HL, Register.A); break;
				case unprefixed.INC_SP: INC(WideRegister.SP); break;
				case unprefixed.INC_AT_HL: INC(WideRegister.HL); break;
				case unprefixed.DEC_AT_HL: DEC(WideRegister.HL); break;
				case unprefixed.LD_AT_HL_d8: LD(WideRegister.HL, 0); break;
				case unprefixed.SCF: SCF(); break;
				case unprefixed.JR_Carry_r8: JR(Flag.C, 0); break;
				case unprefixed.ADD_HL_SP: ADD(WideRegister.HL, WideRegister.SP); break;
				case unprefixed.LD_AD_AT_HL: LD(Register.A, WideRegister.HL); break;
				case unprefixed.DEC_SP: DEC(WideRegister.SP); break;
				case unprefixed.INC_A: INC(Register.A); break;
				case unprefixed.DEC_A: DEC(Register.A); break;
				case unprefixed.LD_A_d8: LD(Register.A, 0); break;
				case unprefixed.CCF: CCF(); break;
				case unprefixed.LD_B_B: LD(Register.B, Register.B); break;
				case unprefixed.LD_B_C: LD(Register.B, Register.C); break;
				case unprefixed.LD_B_D: LD(Register.B, Register.D); break;
				case unprefixed.LD_B_E: LD(Register.B, Register.E); break;
				case unprefixed.LD_B_H: LD(Register.B, Register.H); break;
				case unprefixed.LD_B_L: LD(Register.B, Register.L); break;
				case unprefixed.LD_B_AT_HL: LD(Register.B, WideRegister.HL); break;
				case unprefixed.LD_B_A: LD(Register.B, Register.A); break;
				case unprefixed.LD_C_B: LD(Register.C, Register.B); break;
				case unprefixed.LD_C_C: LD(Register.C, Register.C); break;
				case unprefixed.LD_C_D: LD(Register.C, Register.D); break;
				case unprefixed.LD_C_E: LD(Register.C, Register.E); break;
				case unprefixed.LD_C_H: LD(Register.C, Register.H); break;
				case unprefixed.LD_C_L: LD(Register.C, Register.L); break;
				case unprefixed.LD_C_AT_HL: LD(Register.C, WideRegister.HL); break;
				case unprefixed.LD_C_A: LD(Register.C, Register.A); break;
				case unprefixed.LD_D_B: LD(Register.D, Register.B); break;
				case unprefixed.LD_D_C: LD(Register.D, Register.C); break;
				case unprefixed.LD_D_D: LD(Register.D, Register.D); break;
				case unprefixed.LD_D_E: LD(Register.D, Register.E); break;
				case unprefixed.LD_D_H: LD(Register.D, Register.H); break;
				case unprefixed.LD_D_L: LD(Register.D, Register.L); break;
				case unprefixed.LD_D_AT_HL: LD(Register.D, WideRegister.HL); break;
				case unprefixed.LD_D_A: LD(Register.D, Register.A); break;
				case unprefixed.LD_E_B: LD(Register.E, Register.B); break;
				case unprefixed.LD_E_C: LD(Register.E, Register.C); break;
				case unprefixed.LD_E_D: LD(Register.E, Register.D); break;
				case unprefixed.LD_E_E: LD(Register.E, Register.E); break;
				case unprefixed.LD_E_H: LD(Register.E, Register.H); break;
				case unprefixed.LD_E_L: LD(Register.E, Register.L); break;
				case unprefixed.LD_E_AT_HL: LD(Register.E, WideRegister.HL); break;
				case unprefixed.LD_E_A: LD(Register.E, Register.A); break;
				case unprefixed.LD_H_B: LD(Register.H, Register.B); break;
				case unprefixed.LD_H_C: LD(Register.H, Register.C); break;
				case unprefixed.LD_H_D: LD(Register.H, Register.D); break;
				case unprefixed.LD_H_E: LD(Register.H, Register.E); break;
				case unprefixed.LD_H_H: LD(Register.H, Register.H); break;
				case unprefixed.LD_H_L: LD(Register.H, Register.L); break;
				case unprefixed.LD_H_AT_HL: LD(Register.H, WideRegister.HL); break;
				case unprefixed.LD_H_A: LD(Register.H, Register.A); break;
				case unprefixed.LD_L_B: LD(Register.L, Register.B); break;
				case unprefixed.LD_L_C: LD(Register.L, Register.C); break;
				case unprefixed.LD_L_D: LD(Register.L, Register.D); break;
				case unprefixed.LD_L_E: LD(Register.L, Register.E); break;
				case unprefixed.LD_L_H: LD(Register.L, Register.H); break;
				case unprefixed.LD_L_L: LD(Register.L, Register.L); break;
				case unprefixed.LD_L_AT_HL: LD(Register.L, WideRegister.HL); break;
				case unprefixed.LD_L_A: LD(Register.L, Register.A); break;
				case unprefixed.LD_AT_HL_B: LD(WideRegister.HL, Register.B); break;
				case unprefixed.LD_AT_HL_C: LD(WideRegister.HL, Register.C); break;
				case unprefixed.LD_AT_HL_D: LD(WideRegister.HL, Register.D); break;
				case unprefixed.LD_AT_HL_E: LD(WideRegister.HL, Register.E); break;
				case unprefixed.LD_AT_HL_H: LD(WideRegister.HL, Register.H); break;
				case unprefixed.LD_AT_HL_L: LD(WideRegister.HL, Register.L); break;
				case unprefixed.HALT: HALT(); break;
				case unprefixed.LD_AT_HL_A: LD(WideRegister.HL, Register.A); break;
				case unprefixed.LD_A_B: LD(Register.A, Register.B); break;
				case unprefixed.LD_A_C: LD(Register.A, Register.C); break;
				case unprefixed.LD_A_D: LD(Register.A, Register.D); break;
				case unprefixed.LD_A_E: LD(Register.A, Register.E); break;
				case unprefixed.LD_A_H: LD(Register.A, Register.H); break;
				case unprefixed.LD_A_L: LD(Register.A, Register.L); break;
				case unprefixed.LD_A_AT_HL: LD(Register.A, WideRegister.HL); break;
				case unprefixed.LD_A_A: LD(Register.A, Register.A); break;
				case unprefixed.ADD_A_B: ADD(Register.A, Register.B); break;
				case unprefixed.ADD_A_C: ADD(Register.A, Register.C); break;
				case unprefixed.ADD_A_D: ADD(Register.A, Register.D); break;
				case unprefixed.ADD_A_E: ADD(Register.A, Register.E); break;
				case unprefixed.ADD_A_H: ADD(Register.A, Register.H); break;
				case unprefixed.ADD_A_L: ADD(Register.A, Register.L); break;
				case unprefixed.ADD_A_AT_HL: ADD(Register.A, WideRegister.HL); break;
				case unprefixed.ADD_A_A: ADD(Register.A, Register.A); break;
				case unprefixed.ADC_A_B: ADC(Register.A, Register.B); break;
				case unprefixed.ADC_A_C: ADC(Register.A, Register.C); break;
				case unprefixed.ADC_A_D: ADC(Register.A, Register.D); break;
				case unprefixed.ADC_A_E: ADC(Register.A, Register.E); break;
				case unprefixed.ADC_A_H: ADC(Register.A, Register.H); break;
				case unprefixed.ADC_A_L: ADC(Register.A, Register.L); break;
				case unprefixed.ADC_A_AT_HL: ADC(Register.A, WideRegister.HL); break;
				case unprefixed.ADC_A_A: ADC(Register.A, Register.A); break;
				case unprefixed.SUB_B: SUB(Register.B); break;
				case unprefixed.SUB_C: SUB(Register.C); break;
				case unprefixed.SUB_D: SUB(Register.D); break;
				case unprefixed.SUB_E: SUB(Register.E); break;
				case unprefixed.SUB_H: SUB(Register.H); break;
				case unprefixed.SUB_L: SUB(Register.L); break;
				case unprefixed.SUB_AT_HL: SUB(WideRegister.HL); break;
				case unprefixed.SUB_A: SUB(Register.A); break;
				case unprefixed.SBC_A_B: SBC(Register.A, Register.B); break;
				case unprefixed.SBC_A_C: SBC(Register.A, Register.C); break;
				case unprefixed.SBC_A_D: SBC(Register.A, Register.D); break;
				case unprefixed.SBC_A_E: SBC(Register.A, Register.E); break;
				case unprefixed.SBC_A_H: SBC(Register.A, Register.H); break;
				case unprefixed.SBC_A_L: SBC(Register.A, Register.L); break;
				case unprefixed.SBC_A_AT_HL: SBC(Register.A, WideRegister.HL); break;
				case unprefixed.SBC_A_A: SBC(Register.A, Register.A); break;
				case unprefixed.AND_B: AND(Register.B); break;
				case unprefixed.AND_C: AND(Register.C); break;
				case unprefixed.AND_D: AND(Register.D); break;
				case unprefixed.AND_E: AND(Register.E); break;
				case unprefixed.AND_H: AND(Register.H); break;
				case unprefixed.AND_L: AND(Register.L); break;
				case unprefixed.AND_AT_HL: AND(WideRegister.HL); break;
				case unprefixed.AND_A: AND(Register.A); break;
				case unprefixed.XOR_B: XOR(Register.B); break;
				case unprefixed.XOR_C: XOR(Register.C); break;
				case unprefixed.XOR_D: XOR(Register.D); break;
				case unprefixed.XOR_E: XOR(Register.E); break;
				case unprefixed.XOR_H: XOR(Register.H); break;
				case unprefixed.XOR_L: XOR(Register.L); break;
				case unprefixed.XOR_AT_HL: XOR(WideRegister.HL); break;
				case unprefixed.XOR_A: XOR(Register.A); break;
				case unprefixed.OR_B: OR(Register.B); break;
				case unprefixed.OR_C: OR(Register.C); break;
				case unprefixed.OR_D: OR(Register.D); break;
				case unprefixed.OR_E: OR(Register.E); break;
				case unprefixed.OR_H: OR(Register.H); break;
				case unprefixed.OR_L: OR(Register.L); break;
				case unprefixed.OR_AT_HL: OR(WideRegister.HL); break;
				case unprefixed.OR_A: OR(Register.A); break;
				case unprefixed.CP_B: CP(Register.B); break;
				case unprefixed.CP_C: CP(Register.C); break;
				case unprefixed.CP_D: CP(Register.D); break;
				case unprefixed.CP_E: CP(Register.E); break;
				case unprefixed.CP_H: CP(Register.H); break;
				case unprefixed.CP_L: CP(Register.L); break;
				case unprefixed.CP_AT_HL: CP(WideRegister.HL); break;
				case unprefixed.CP_A: CP(Register.A); break;
				case unprefixed.RET_NZ: RET(Flag.NZ); break;
				case unprefixed.POP_BC: POP(WideRegister.BC); break;
				case unprefixed.JP_NZ_a16: JP(Flag.NZ, 0); break;
				case unprefixed.JP_a16: JP(0); break;
				case unprefixed.CALL_NZ_a16: CALL(Flag.NZ, 0); break;
				case unprefixed.PUSH_BC: PUSH(WideRegister.BC); break;
				case unprefixed.ADD_A_d8: ADD(Register.A, 0); break;
				case unprefixed.RST_00H: RST(0); break;
				case unprefixed.RET_Zero: RET(Flag.Z); break;
				case unprefixed.RET: RET(); break;
				case unprefixed.JP_Zero_a16: JP(Flag.Z, 0); break;
				case unprefixed.PREFIX: PREFIX(); break;
				case unprefixed.CALL_Zero_a16: CALL(Flag.Z, 0); break;
				case unprefixed.CALL_a16: CALL(0); break;
				case unprefixed.ADC_A_d8: ADC(Register.A, 0); break;
				case unprefixed.RST_08H: RST(0); break;
				case unprefixed.RET_NC: RET(Flag.NC); break;
				case unprefixed.POP_DE: POP(WideRegister.DE); break;
				case unprefixed.JP_NC_a16: JP(Flag.NC, 0); break;
				case unprefixed.ILLEGAL_D3: ILLEGAL_D3(); break;
				case unprefixed.CALL_NC_a16: CALL(Flag.NC, 0); break;
				case unprefixed.PUSH_DE: PUSH(WideRegister.DE); break;
				case unprefixed.SUB_d8: SUB(0); break;
				case unprefixed.RST_10H: RST(0); break;
				case unprefixed.RET_Carry: RET(Flag.C); break;
				case unprefixed.RETI: RETI(); break;
				case unprefixed.JP_Carry_a16: JP(Flag.C, 0); break;
				case unprefixed.ILLEGAL_DB: ILLEGAL_DB(); break;
				case unprefixed.CALL_Carry_a16: CALL(Flag.C, 0); break;
				case unprefixed.ILLEGAL_DD: ILLEGAL_DD(); break;
				case unprefixed.SBC_A_d8: SBC(Register.A, 0); break;
				case unprefixed.RST_18H: RST(0); break;
				case unprefixed.LDH_AT_a8_A: LDH(0, Register.A); break;
				case unprefixed.POP_HL: POP(WideRegister.HL); break;
				case unprefixed.LD_AT_C_A: LD(Register.C, Register.A); break;
				case unprefixed.ILLEGAL_E3: ILLEGAL_E3(); break;
				case unprefixed.ILLEGAL_E4: ILLEGAL_E4(); break;
				case unprefixed.PUSH_HL: PUSH(WideRegister.HL); break;
				case unprefixed.AND_d8: AND(0); break;
				case unprefixed.RST_20H: RST(0); break;
				case unprefixed.ADD_SP_r8: ADD(WideRegister.SP, 0); break;
				case unprefixed.JP_HL: JP(WideRegister.HL); break;
				case unprefixed.LD_AT_a16_A: LD(0, Register.A); break;
				case unprefixed.ILLEGAL_EB: ILLEGAL_EB(); break;
				case unprefixed.ILLEGAL_EC: ILLEGAL_EC(); break;
				case unprefixed.ILLEGAL_ED: ILLEGAL_ED(); break;
				case unprefixed.XOR_d8: XOR(0); break;
				case unprefixed.RST_28H: RST(0); break;
				case unprefixed.LDH_A_AT_a8: LDH(Register.A, 0); break;
				case unprefixed.POP_AF: POP(WideRegister.AF); break;
				case unprefixed.LD_A_AT_C: LD(Register.A, Register.C); break;
				case unprefixed.DI: DI(); break;
				case unprefixed.ILLEGAL_F4: ILLEGAL_F4(); break;
				case unprefixed.PUSH_AF: PUSH(WideRegister.AF); break;
				case unprefixed.OR_d8: OR(0); break;
				case unprefixed.RST_30H: RST(0); break;
				case unprefixed.LD_HLI_SP: LD(WideRegister.HL, WideRegister.SP); break;
				case unprefixed.LD_SP_HL: LD(WideRegister.SP, WideRegister.HL); break;
				case unprefixed.LD_A_AT_a16: LD(Register.A, 0); break;
				case unprefixed.EI: EI(); break;
				case unprefixed.ILLEGAL_FC: ILLEGAL_FC(); break;
				case unprefixed.ILLEGAL_FD: ILLEGAL_FD(); break;
				case unprefixed.CP_d8: CP(0); break;
				case unprefixed.RST_38H: RST(0); break;
			}
		}

        private static void ILLEGAL_FD()
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_FC()
        {
            throw new NotImplementedException();
        }

        private static void EI()
        {
            throw new NotImplementedException();
        }

        private static void LD(WideRegister hL, WideRegister sP)
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_F4()
        {
            throw new NotImplementedException();
        }

        private static void DI()
        {
            throw new NotImplementedException();
        }

        private static void LDH(Register a, int v)
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_ED()
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_EC()
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_EB()
        {
            throw new NotImplementedException();
        }

        private static void JP(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_E4()
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_E3()
        {
            throw new NotImplementedException();
        }

        private static void LDH(int v, Register a)
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_DD()
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_DB()
        {
            throw new NotImplementedException();
        }

        private static void RETI()
        {
            throw new NotImplementedException();
        }

        private static void ILLEGAL_D3()
        {
            throw new NotImplementedException();
        }

        private static void CALL(int v)
        {
            throw new NotImplementedException();
        }

        private static void PREFIX()
        {
            throw new NotImplementedException();
        }

        private static void RET()
        {
            throw new NotImplementedException();
        }

        private static void RST(int v)
        {
            throw new NotImplementedException();
        }

        private static void PUSH(WideRegister bC)
        {
            throw new NotImplementedException();
        }

        private static void CALL(Flag nZ, int v)
        {
            throw new NotImplementedException();
        }

        private static void JP(int v)
        {
            throw new NotImplementedException();
        }

        private static void JP(Flag nZ, int v)
        {
            throw new NotImplementedException();
        }

        private static void POP(WideRegister bC)
        {
            throw new NotImplementedException();
        }

        private static void RET(Flag nZ)
        {
            throw new NotImplementedException();
        }

        private static void CP(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void CP(Register c)
        {
            throw new NotImplementedException();
        }

        private static void OR(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void OR(Register b)
        {
            throw new NotImplementedException();
        }

        private static void XOR(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void XOR(Register b)
        {
            throw new NotImplementedException();
        }

        private static void AND(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void AND(Register b)
        {
            throw new NotImplementedException();
        }

        private static void SBC(Register a, WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void SBC(Register a, Register b)
        {
            throw new NotImplementedException();
        }

        private static void SUB(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void SUB(Register b)
        {
            throw new NotImplementedException();
        }

        private static void ADC(Register a, WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void ADC(Register a, Register b)
        {
            throw new NotImplementedException();
        }

        private static void ADD(Register a, WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void ADD(Register a, Register b)
        {
            throw new NotImplementedException();
        }

        private static void HALT()
        {
            throw new NotImplementedException();
        }

        private static void LD(Register b1, Register b2)
        {
            throw new NotImplementedException();
        }

        private static void CCF()
        {
            throw new NotImplementedException();
        }

        private static void SCF()
        {
            throw new NotImplementedException();
        }

        private static void JR(int v)
        {
            throw new NotImplementedException();
        }

        private static void CPL()
        {
            throw new NotImplementedException();
        }

        private static void DAA()
        {
            throw new NotImplementedException();
        }

        private static void RRA()
        {
            throw new NotImplementedException();
        }

        private static void JR(Flag nZ, int v)
        {
            throw new NotImplementedException();
        }

        private static void RLA()
        {
            throw new NotImplementedException();
        }

        private static void STOP()
        {
            throw new NotImplementedException();
        }

        private static void RRCA()
        {
            throw new NotImplementedException();
        }

        private static void DEC(WideRegister bC)
        {
            throw new NotImplementedException();
        }

        private static void LD(Register a, WideRegister bC)
        {
            throw new NotImplementedException();
        }

        private static void ADD(WideRegister hL, WideRegister bC)
        {
            throw new NotImplementedException();
        }

        private static void LD(int v, WideRegister sP)
        {
            throw new NotImplementedException();
        }

        private static void RLCA()
        {
            throw new NotImplementedException();
        }

        private static void LD(Register b, int v)
        {
            throw new NotImplementedException();
        }

        private static void DEC(Register b)
        {
            throw new NotImplementedException();
        }

        private static void INC(Register b)
        {
            throw new NotImplementedException();
        }

        private static void INC(WideRegister bC)
        {
            throw new NotImplementedException();
        }

        private static void LD(WideRegister bC, Register a)
        {
            throw new NotImplementedException();
        }

        private static void LD(WideRegister bC, int v)
        {
            throw new NotImplementedException();
        }

        private static void NOP()
        {
            throw new NotImplementedException();
        }

        public static void Exec(cbprefixed o)
		{
			switch (o)
			{
				case cbprefixed.RLC_B: RLC(Register.B); break;
				case cbprefixed.RLC_C: RLC(Register.C); break;
				case cbprefixed.RLC_D: RLC(Register.D); break;
				case cbprefixed.RLC_E: RLC(Register.E); break;
				case cbprefixed.RLC_H: RLC(Register.H); break;
				case cbprefixed.RLC_L: RLC(Register.L); break;
				case cbprefixed.RLC_AT_HL: RLC(WideRegister.HL); break;
				case cbprefixed.RLC_A: RLC(Register.A); break;
				case cbprefixed.RRC_B: RRC(Register.B); break;
				case cbprefixed.RRC_C: RRC(Register.C); break;
				case cbprefixed.RRC_D: RRC(Register.D); break;
				case cbprefixed.RRC_E: RRC(Register.E); break;
				case cbprefixed.RRC_H: RRC(Register.H); break;
				case cbprefixed.RRC_L: RRC(Register.L); break;
				case cbprefixed.RRC_AT_HL: RRC(WideRegister.HL); break;
				case cbprefixed.RRC_A: RRC(Register.A); break;
				case cbprefixed.RL_B: RL(Register.B); break;
				case cbprefixed.RL_C: RL(Register.C); break;
				case cbprefixed.RL_D: RL(Register.D); break;
				case cbprefixed.RL_E: RL(Register.E); break;
				case cbprefixed.RL_H: RL(Register.H); break;
				case cbprefixed.RL_L: RL(Register.L); break;
				case cbprefixed.RL_AT_HL: RL(WideRegister.HL); break;
				case cbprefixed.RL_A: RL(Register.A); break;
				case cbprefixed.RR_B: RR(Register.B); break;
				case cbprefixed.RR_C: RR(Register.C); break;
				case cbprefixed.RR_D: RR(Register.D); break;
				case cbprefixed.RR_E: RR(Register.E); break;
				case cbprefixed.RR_H: RR(Register.H); break;
				case cbprefixed.RR_L: RR(Register.L); break;
				case cbprefixed.RR_AT_HL: RR(WideRegister.HL); break;
				case cbprefixed.RR_A: RR(Register.A); break;
				case cbprefixed.SLA_B: SLA(Register.B); break;
				case cbprefixed.SLA_C: SLA(Register.C); break;
				case cbprefixed.SLA_D: SLA(Register.D); break;
				case cbprefixed.SLA_E: SLA(Register.E); break;
				case cbprefixed.SLA_H: SLA(Register.H); break;
				case cbprefixed.SLA_L: SLA(Register.L); break;
				case cbprefixed.SLA_AT_HL: SLA(WideRegister.HL); break;
				case cbprefixed.SLA_A: SLA(Register.A); break;
				case cbprefixed.SRA_B: SRA(Register.B); break;
				case cbprefixed.SRA_C: SRA(Register.C); break;
				case cbprefixed.SRA_D: SRA(Register.D); break;
				case cbprefixed.SRA_E: SRA(Register.E); break;
				case cbprefixed.SRA_H: SRA(Register.H); break;
				case cbprefixed.SRA_L: SRA(Register.L); break;
				case cbprefixed.SRA_AT_HL: SRA(WideRegister.HL); break;
				case cbprefixed.SRA_A: SRA(Register.A); break;
				case cbprefixed.SWAP_B: SWAP(Register.B); break;
				case cbprefixed.SWAP_C: SWAP(Register.C); break;
				case cbprefixed.SWAP_D: SWAP(Register.D); break;
				case cbprefixed.SWAP_E: SWAP(Register.E); break;
				case cbprefixed.SWAP_H: SWAP(Register.H); break;
				case cbprefixed.SWAP_L: SWAP(Register.L); break;
				case cbprefixed.SWAP_AT_HL: SWAP(WideRegister.HL); break;
				case cbprefixed.SWAP_A: SWAP(Register.A); break;
				case cbprefixed.SRL_B: SRL(Register.B); break;
				case cbprefixed.SRL_C: SRL(Register.C); break;
				case cbprefixed.SRL_D: SRL(Register.D); break;
				case cbprefixed.SRL_E: SRL(Register.E); break;
				case cbprefixed.SRL_H: SRL(Register.H); break;
				case cbprefixed.SRL_L: SRL(Register.L); break;
				case cbprefixed.SRL_AT_HL: SRL(WideRegister.HL); break;
				case cbprefixed.SRL_A: SRL(Register.A); break;
				case cbprefixed.BIT_0_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_0_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_0_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_0_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_0_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_0_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_0_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_0_A: BIT(0, Register.A); break;
				case cbprefixed.BIT_1_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_1_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_1_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_1_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_1_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_1_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_1_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_1_A: BIT(0, Register.A); break;
				case cbprefixed.BIT_2_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_2_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_2_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_2_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_2_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_2_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_2_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_2_A: BIT(0, Register.A); break;
				case cbprefixed.BIT_3_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_3_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_3_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_3_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_3_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_3_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_3_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_3_A: BIT(0, Register.A); break;
				case cbprefixed.BIT_4_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_4_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_4_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_4_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_4_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_4_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_4_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_4_A: BIT(0, Register.A); break;
				case cbprefixed.BIT_5_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_5_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_5_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_5_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_5_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_5_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_5_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_5_A: BIT(0, Register.A); break;
				case cbprefixed.BIT_6_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_6_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_6_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_6_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_6_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_6_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_6_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_6_A: BIT(0, Register.A); break;
				case cbprefixed.BIT_7_B: BIT(0, Register.B); break;
				case cbprefixed.BIT_7_C: BIT(0, Register.C); break;
				case cbprefixed.BIT_7_D: BIT(0, Register.D); break;
				case cbprefixed.BIT_7_E: BIT(0, Register.E); break;
				case cbprefixed.BIT_7_H: BIT(0, Register.H); break;
				case cbprefixed.BIT_7_L: BIT(0, Register.L); break;
				case cbprefixed.BIT_7_AT_HL: BIT(0, WideRegister.HL); break;
				case cbprefixed.BIT_7_A: BIT(0, Register.A); break;
				case cbprefixed.RES_0_B: RES(0, Register.B); break;
				case cbprefixed.RES_0_C: RES(0, Register.C); break;
				case cbprefixed.RES_0_D: RES(0, Register.D); break;
				case cbprefixed.RES_0_E: RES(0, Register.E); break;
				case cbprefixed.RES_0_H: RES(0, Register.H); break;
				case cbprefixed.RES_0_L: RES(0, Register.L); break;
				case cbprefixed.RES_0_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_0_A: RES(0, Register.A); break;
				case cbprefixed.RES_1_B: RES(0, Register.B); break;
				case cbprefixed.RES_1_C: RES(0, Register.C); break;
				case cbprefixed.RES_1_D: RES(0, Register.D); break;
				case cbprefixed.RES_1_E: RES(0, Register.E); break;
				case cbprefixed.RES_1_H: RES(0, Register.H); break;
				case cbprefixed.RES_1_L: RES(0, Register.L); break;
				case cbprefixed.RES_1_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_1_A: RES(0, Register.A); break;
				case cbprefixed.RES_2_B: RES(0, Register.B); break;
				case cbprefixed.RES_2_C: RES(0, Register.C); break;
				case cbprefixed.RES_2_D: RES(0, Register.D); break;
				case cbprefixed.RES_2_E: RES(0, Register.E); break;
				case cbprefixed.RES_2_H: RES(0, Register.H); break;
				case cbprefixed.RES_2_L: RES(0, Register.L); break;
				case cbprefixed.RES_2_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_2_A: RES(0, Register.A); break;
				case cbprefixed.RES_3_B: RES(0, Register.B); break;
				case cbprefixed.RES_3_C: RES(0, Register.C); break;
				case cbprefixed.RES_3_D: RES(0, Register.D); break;
				case cbprefixed.RES_3_E: RES(0, Register.E); break;
				case cbprefixed.RES_3_H: RES(0, Register.H); break;
				case cbprefixed.RES_3_L: RES(0, Register.L); break;
				case cbprefixed.RES_3_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_3_A: RES(0, Register.A); break;
				case cbprefixed.RES_4_B: RES(0, Register.B); break;
				case cbprefixed.RES_4_C: RES(0, Register.C); break;
				case cbprefixed.RES_4_D: RES(0, Register.D); break;
				case cbprefixed.RES_4_E: RES(0, Register.E); break;
				case cbprefixed.RES_4_H: RES(0, Register.H); break;
				case cbprefixed.RES_4_L: RES(0, Register.L); break;
				case cbprefixed.RES_4_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_4_A: RES(0, Register.A); break;
				case cbprefixed.RES_5_B: RES(0, Register.B); break;
				case cbprefixed.RES_5_C: RES(0, Register.C); break;
				case cbprefixed.RES_5_D: RES(0, Register.D); break;
				case cbprefixed.RES_5_E: RES(0, Register.E); break;
				case cbprefixed.RES_5_H: RES(0, Register.H); break;
				case cbprefixed.RES_5_L: RES(0, Register.L); break;
				case cbprefixed.RES_5_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_5_A: RES(0, Register.A); break;
				case cbprefixed.RES_6_B: RES(0, Register.B); break;
				case cbprefixed.RES_6_C: RES(0, Register.C); break;
				case cbprefixed.RES_6_D: RES(0, Register.D); break;
				case cbprefixed.RES_6_E: RES(0, Register.E); break;
				case cbprefixed.RES_6_H: RES(0, Register.H); break;
				case cbprefixed.RES_6_L: RES(0, Register.L); break;
				case cbprefixed.RES_6_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_6_A: RES(0, Register.A); break;
				case cbprefixed.RES_7_B: RES(0, Register.B); break;
				case cbprefixed.RES_7_C: RES(0, Register.C); break;
				case cbprefixed.RES_7_D: RES(0, Register.D); break;
				case cbprefixed.RES_7_E: RES(0, Register.E); break;
				case cbprefixed.RES_7_H: RES(0, Register.H); break;
				case cbprefixed.RES_7_L: RES(0, Register.L); break;
				case cbprefixed.RES_7_AT_HL: RES(0, WideRegister.HL); break;
				case cbprefixed.RES_7_A: RES(0, Register.A); break;
				case cbprefixed.SET_0_B: SET(0, Register.B); break;
				case cbprefixed.SET_0_C: SET(0, Register.C); break;
				case cbprefixed.SET_0_D: SET(0, Register.D); break;
				case cbprefixed.SET_0_E: SET(0, Register.E); break;
				case cbprefixed.SET_0_H: SET(0, Register.H); break;
				case cbprefixed.SET_0_L: SET(0, Register.L); break;
				case cbprefixed.SET_0_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_0_A: SET(0, Register.A); break;
				case cbprefixed.SET_1_B: SET(0, Register.B); break;
				case cbprefixed.SET_1_C: SET(0, Register.C); break;
				case cbprefixed.SET_1_D: SET(0, Register.D); break;
				case cbprefixed.SET_1_E: SET(0, Register.E); break;
				case cbprefixed.SET_1_H: SET(0, Register.H); break;
				case cbprefixed.SET_1_L: SET(0, Register.L); break;
				case cbprefixed.SET_1_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_1_A: SET(0, Register.A); break;
				case cbprefixed.SET_2_B: SET(0, Register.B); break;
				case cbprefixed.SET_2_C: SET(0, Register.C); break;
				case cbprefixed.SET_2_D: SET(0, Register.D); break;
				case cbprefixed.SET_2_E: SET(0, Register.E); break;
				case cbprefixed.SET_2_H: SET(0, Register.H); break;
				case cbprefixed.SET_2_L: SET(0, Register.L); break;
				case cbprefixed.SET_2_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_2_A: SET(0, Register.A); break;
				case cbprefixed.SET_3_B: SET(0, Register.B); break;
				case cbprefixed.SET_3_C: SET(0, Register.C); break;
				case cbprefixed.SET_3_D: SET(0, Register.D); break;
				case cbprefixed.SET_3_E: SET(0, Register.E); break;
				case cbprefixed.SET_3_H: SET(0, Register.H); break;
				case cbprefixed.SET_3_L: SET(0, Register.L); break;
				case cbprefixed.SET_3_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_3_A: SET(0, Register.A); break;
				case cbprefixed.SET_4_B: SET(0, Register.B); break;
				case cbprefixed.SET_4_C: SET(0, Register.C); break;
				case cbprefixed.SET_4_D: SET(0, Register.D); break;
				case cbprefixed.SET_4_E: SET(0, Register.E); break;
				case cbprefixed.SET_4_H: SET(0, Register.H); break;
				case cbprefixed.SET_4_L: SET(0, Register.L); break;
				case cbprefixed.SET_4_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_4_A: SET(0, Register.A); break;
				case cbprefixed.SET_5_B: SET(0, Register.B); break;
				case cbprefixed.SET_5_C: SET(0, Register.C); break;
				case cbprefixed.SET_5_D: SET(0, Register.D); break;
				case cbprefixed.SET_5_E: SET(0, Register.E); break;
				case cbprefixed.SET_5_H: SET(0, Register.H); break;
				case cbprefixed.SET_5_L: SET(0, Register.L); break;
				case cbprefixed.SET_5_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_5_A: SET(0, Register.A); break;
				case cbprefixed.SET_6_B: SET(0, Register.B); break;
				case cbprefixed.SET_6_C: SET(0, Register.C); break;
				case cbprefixed.SET_6_D: SET(0, Register.D); break;
				case cbprefixed.SET_6_E: SET(0, Register.E); break;
				case cbprefixed.SET_6_H: SET(0, Register.H); break;
				case cbprefixed.SET_6_L: SET(0, Register.L); break;
				case cbprefixed.SET_6_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_6_A: SET(0, Register.A); break;
				case cbprefixed.SET_7_B: SET(0, Register.B); break;
				case cbprefixed.SET_7_C: SET(0, Register.C); break;
				case cbprefixed.SET_7_D: SET(0, Register.D); break;
				case cbprefixed.SET_7_E: SET(0, Register.E); break;
				case cbprefixed.SET_7_H: SET(0, Register.H); break;
				case cbprefixed.SET_7_L: SET(0, Register.L); break;
				case cbprefixed.SET_7_AT_HL: SET(0, WideRegister.HL); break;
				case cbprefixed.SET_7_A: SET(0, Register.A); break;
			}
		}

        private static void SET(int v, WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void SET(int v, Register b)
        {
            throw new NotImplementedException();
        }

        private static void RES(int v, WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void RES(int v, Register b)
        {
            throw new NotImplementedException();
        }

        private static void BIT(int v, WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void BIT(int v, Register b)
        {
            throw new NotImplementedException();
        }

        private static void SRL(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void SRL(Register b)
        {
            throw new NotImplementedException();
        }

        private static void SWAP(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void SWAP(Register b)
        {
            throw new NotImplementedException();
        }

        private static void SRA(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void SRA(Register b)
        {
            throw new NotImplementedException();
        }

        private static void SLA(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void SLA(Register b)
        {
            throw new NotImplementedException();
        }

        private static void RR(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void RR(Register b)
        {
            throw new NotImplementedException();
        }

        private static void RL(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void RL(Register b)
        {
            throw new NotImplementedException();
        }

        private static void RRC(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void RRC(Register b)
        {
            throw new NotImplementedException();
        }

        private static void RLC(WideRegister hL)
        {
            throw new NotImplementedException();
        }

        private static void RLC(Register b)
        {
            throw new NotImplementedException();
        }
    }
}