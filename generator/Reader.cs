using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace generator
{

    public class Reader
    {
        readonly Dictionary<string, List<Opcode>> opcodes = new Dictionary<string, List<Opcode>>();

        public Reader(string s)
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(s));
            var root = document.RootElement;

            foreach (var block in new string[] { "unprefixed", "cbprefixed" })
            {
                var OpCodeCategory = root.GetProperty(block);
                opcodes[block] = new List<Opcode>();

                foreach (var op in OpCodeCategory.EnumerateObject())
                {
                    Opcode current;
                    current.ID = Convert.ToByte(op.Name, 16);
                    current.mnemonic = op.Value.GetProperty("mnemonic").GetString();
                    current.bytes = op.Value.GetProperty("bytes").GetInt32();

                    current.cycles = new List<int>();
                    foreach (var cycle in op.Value.GetProperty("cycles").EnumerateArray())
                        current.cycles.Add(cycle.GetInt32());

                    current.operands = new List<Operand>();
                    foreach (var currentOperand in op.Value.GetProperty("operands").EnumerateArray())
                    {
                        var operandName = currentOperand.GetProperty("name").GetString();

                        bool isConditional;
                        switch (current.mnemonic)
                        {
                            case "JR":
                            case "RET":
                            case "JP":
                            case "CALL":
                                isConditional = true;
                                break;
                            default:
                                isConditional = false;
                                break;
                        }

                        if (isConditional) operandName = DisambiguateFlagName(operandName);

                        Operand operands = new Operand(operandName,
                            currentOperand.GetProperty("immediate").GetBoolean());

                        if (currentOperand.TryGetProperty("bytes", out JsonElement optional))
                            operands.Size = optional.GetInt32();
                        if (currentOperand.TryGetProperty("increment", out JsonElement increment))
                            operands.Increment = increment.GetBoolean();
                        if (currentOperand.TryGetProperty("decrement", out JsonElement decrement))
                            operands.Decrement = decrement.GetBoolean();

                        current.operands.Add(operands);
                    }

                    current.immediate = op.Value.GetProperty("immediate").GetBoolean();

                    current.flags = new List<(string, string)>();
                    foreach (var flagMode in op.Value.GetProperty("flags").EnumerateObject())
                    {
                        (string, string) mode;
                        mode.Item1 = flagMode.Name;
                        mode.Item2 = flagMode.Value.GetString();
                    }

                    opcodes[block].Add(current);
                }
            }
        }

        private string DisambiguateFlagName(string operandName) => operandName switch
        {
            "Z" => "Zero",
            "N" => "Negative",
            "H" => "HalfCarry",
            "C" => "Carry",
            _ => operandName,
        };

        public List<string> PrintableEnum()
        {
            var output = new List<string>();
            foreach (var block in opcodes)
            {
                var currentEnum = new List<string>();
                currentEnum.Add("public enum " + block.Key.Substring(0, 1).ToString().ToUpper() +
                                  block.Key.Substring(1) + " : byte");
                currentEnum.Add("{");
                foreach (var o in block.Value)
                {
                    string tag = o.MakePrettyTag();
                    string value = "0x" + o.ID.ToString("X2");
                    currentEnum.Add("\t" + tag + " = " + value + ",");
                }

                currentEnum.Add("};");
                output.Add(string.Join('\n', currentEnum));

            }
            return output;
        }

        public List<string> PrintableEnumToStringMapping()
        {
            List<string> output = new List<string>();
            foreach (var block in opcodes)
            {
                List<string> current = new List<string>();
                current.Add("string " + block.Key + "ToString(Opcode op)");
                current.Add("=> op switch {");
                foreach (var v in block.Value)
                {
                    string tag = block.Key + "." + v.MakeTag();
                    string value = "\"" + v.MakePrettyTag() + "\"";
                    current.Add("\t" + tag + " => " + value + ",");
                }

                current.Add("};");
                output.Add(string.Join('\n', current));
            }
            return output;
        }
        public void PrintFunctionConstructors()
        {
            foreach (var block in opcodes) PrintFunctionConstructor(block);
        }
        public void PrintFunctionSignatures()
        {
            List<string> functions = new List<string>();
            foreach (var block in opcodes)
                foreach (var op in block.Value)
                    functions.Add(op.MakeFunctionSignature());

            foreach (var s in functions)
                Console.WriteLine(s);
        }

        private static void PrintFunctionConstructor(KeyValuePair<string, List<Opcode>> block)
        {
            string mapType = "Dictionary <" + block.Key + ", Action>";
            Console.WriteLine("public " + mapType + " MakeTable(" + block.Key + " o" + ") {");
            Console.WriteLine(mapType + " m = new " + mapType + "();");
            foreach (var op in block.Value)
                Console.WriteLine("m[" + TypedTag(block.Key, op.MakeTag()) + "] = " +
                                  string.Join(',', op.MakeFunctionConstructorArguments()) + ";");
            Console.WriteLine("}");
        }

        private static string TypedTag(string key, string op) => key + "." + op;


        public HashSet<(string, bool)> PossibleOperands()
        {
            HashSet<(string, bool)> s = new HashSet<(string, bool)>();
            foreach (var block in opcodes)
                foreach (var op in block.Value)
                    foreach (var operand in op.operands)
                        s.Add((operand.Name, operand.Pointer));

            return s;
        }

        public void PrintFunctions()
        {
            foreach (var f in MakeUniqueFunctions())
                Console.WriteLine(f);
        }

        private List<string> MakeUniqueFunctions()
        {
            HashSet<string> Seen = new HashSet<string>();

            foreach (var block in opcodes)
                foreach (var op in block.Value)
                    Seen.Add(op.MakeFunction());
            return Seen.ToList();
        }

    }
}