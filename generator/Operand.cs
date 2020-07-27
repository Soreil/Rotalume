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

                _ => Name
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


}