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
        public void PrintFunctionConstructors()
        {
            r.PrintFunctionConstructors();
        }

        [Test]
        public void PrintFunctionSignatures()
        {
            r.PrintFunctionSignatures();
        }

        [Test]
        public void PrintFunctionSkeletons()
        {
            r.PrintFunctions();
        }
    }

}