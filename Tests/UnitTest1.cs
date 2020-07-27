using NUnit.Framework;
using generator;
using System;

namespace Tests
{
    public class Tests
    {
        private readonly Reader r;

        public Tests() => r = new Reader("..\\..\\..\\json\\Opcodes.json");

        [Test]
        public void PrintEnum() => r.PrintEnum();

        [Test]
        public void PrintEnumString() => r.PrintEnumStrings();

        [Test]
        public void PrintPossibleTags()
        {
            var ops = r.PossibleOperands();
            foreach (var op in ops)
            {
                Console.WriteLine(op.Item1 + " " + op.Item2.ToString());
            }
        }

        [Test]
        public void PrintFunctionSignatures()
        {
            r.PrintFunctionSignatures();
        }

        //[Test]
        //public void RegisterWrite()
        //{
        //    Registers s = new Registers();
        //    s.Set(AF,0xf00f);
        //    Console.WriteLine(s.AF.Read());
        //    Console.WriteLine(s.A.Read());
        //    Console.WriteLine(s.F.Read());
        //}
    }

}