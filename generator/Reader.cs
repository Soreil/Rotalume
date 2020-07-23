using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace generator
{
    public struct Opcode
    {
        public byte ID;
        public string mnemonic;
        public int bytes;
        public List<int> cycles;
        //There is also a possible increment flag here, we really need a struct instead 
        //of a tuple at this point since it changes too often.
        public List<Operand> operands;
        public bool immediate;
        public List<(string, string)> flags;

        public override string ToString()
        {
            List<string> cycleStrings = new List<string>();
            foreach (var c in cycles)
            {
                cycleStrings.Add(c.ToString());
            }
            var cycleString = string.Join(' ', cycleStrings);

            List<string> operandStrings = new List<string>();
            foreach (var o in operands)
                operandStrings.Add("\t" + o.Name + " " + o.Pointer);
            var operandString = string.Join('\n', operandStrings);

            List<string> flagStrings = new List<string>();
            foreach (var f in flags)
                flagStrings.Add("\t" + f.Item1 + ":" + f.Item2);
            var flagString = string.Join('\n', flagStrings);

            return string.Join('\n', new List<string> {
                mnemonic ,
                bytes.ToString(),
                cycleString,
                operandString,
                immediate.ToString(),
                flagString
            });
        }
    }
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
                        Operand operands = new Operand(currentOperand.GetProperty("name").GetString(),
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

        public void PrintEnum()
        {
            foreach (var block in opcodes)
            {
                Console.WriteLine("enum " + block.Key + " : byte");
                Console.WriteLine("{");
                foreach (var o in block.Value)
                {
                    string tag = MakeTag(o);
                    string value = "0x" + o.ID.ToString("X2");
                    Console.WriteLine("\t" + tag + " = " + value + ",");
                }
                Console.WriteLine("};");
            }
        }

        public void PrintEnumStrings()
        {
            foreach (var block in opcodes)
            {
                Console.WriteLine("string " + block.Key + "ToString(Opcode op)");
                Console.WriteLine("=> op switch {");
                foreach (var v in block.Value)
                {
                    string tag = block.Key + "." + MakeTag(v);
                    string value = "\"" + MakePrettyTag(v) + "\"";
                    Console.WriteLine("\t" + tag + " => " + value + ",");
                }
                Console.WriteLine("};");
            }
        }

        private static string MakeTag(Opcode o)
        {
            string tag = o.mnemonic;
            foreach (var t in o.operands)
            {
                if (t.Increment) tag += "I";
                if (t.Decrement) tag += "D";
                tag += "_";
                tag += t.Pointer ? t.Name : "AT_" + t.Name;
            }

            return tag;
        }

        private static string MakePrettyTag(Opcode o)
        {
            string tag = o.mnemonic;
            foreach (var t in o.operands)
            {
                tag += " ";
                if (!t.Pointer) tag += "(";
                tag += t.Name;
                if (t.Increment) tag += "+";
                if (t.Decrement) tag += "-";
                if (!t.Pointer) tag += ")";
            }

            return tag;
        }

        public HashSet<(string, bool)> PossibleOperands()
        {
            HashSet<(string, bool)> s = new HashSet<(string, bool)>();
            foreach (var block in opcodes)
                foreach (var op in block.Value)
                    foreach (var operand in op.operands)
                        s.Add((operand.Name, operand.Pointer));

            return s;
        }
    }
}
