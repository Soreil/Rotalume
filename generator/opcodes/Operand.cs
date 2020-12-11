namespace emulator
{
    public record Operand
    { 
        public string Name;
        public int? Size;
        public bool Pointer;
        public Postfix Postfix;

        public Operand(string name, bool pointer)
        {
            Name = name;
            Size = null;
            Postfix = Postfix.unchanged;
            Pointer = pointer;
        }

        internal string MakeOperandArgumentValue() => Name switch
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

            "d16" => "DMGInteger.d16",
            "a16" => "DMGInteger.a16",

            "a8" => "DMGInteger.a8",
            "d8" => "DMGInteger.d8",
            "r8" => "DMGInteger.r8",

            "Zero" => "Flag.Z",
            "Negative" => "Flag.M",
            "HalfCarry" => "Flag.H",
            "Carry" => "Flag.C",

            "NZ" => "Flag.NZ",
            "NN" => "Flag.NN",
            "NH" => "Flag.NH",
            "NC" => "Flag.NC",

            "00H" => "0x00",
            "08H" => "0x08",
            "10H" => "0x10",
            "18H" => "0x18",
            "20H" => "0x20",
            "28H" => "0x28",
            "30H" => "0x30",
            "38H" => "0x38",

            _ => Name
        };

        internal string MakeOperandArgumentType() => Name switch
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

            "d16" => "DMGInteger",
            "a16" => "DMGInteger",

            "a8" => "DMGInteger",
            "d8" => "DMGInteger",
            "r8" => "DMGInteger",

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
    }
}