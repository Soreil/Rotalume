using System;

using generator;

using NUnit.Framework;

namespace Tests
{
    public class GenerationTests
    {
        private readonly Reader r;

        public GenerationTests() => r = new Reader("..\\..\\..\\json\\Opcodes.json");

        [Test]
        public void PrintEnum()
        {
            var enums = r.PrintableEnum();
            Assert.AreEqual(2, enums.Count);
            foreach (var e in enums)
                Console.WriteLine(e);
        }

        [Test]
        public void PrintEnumToStringMapping()
        {
            var lines = r.PrintableEnumToStringMapping();
            foreach (var line in lines)
                Console.WriteLine(line);
        }

        [Test]
        public void PrintPossibleTags()
        {
            var ops = r.PossibleOperands();
            foreach (var op in ops)
                Console.WriteLine(op.Item1 + " " + op.Item2.ToString());
        }

        [Test]
        public void PrintFunctionConstructors() => r.PrintFunctionConstructors();

        [Test]
        public void PrintFunctionSignatures() => r.PrintFunctionSignatures();

        [Test]
        public void PrintFunctionSkeletons() => r.PrintFunctions();
    }

}