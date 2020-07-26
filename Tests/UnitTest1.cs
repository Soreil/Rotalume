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
        public void RegisterWrite()
        {
            Storage s = new Storage();
            s.AF.Write(0xf00f);
            Console.WriteLine(s.AF.Read());
            Console.WriteLine(s.A.Read());
            Console.WriteLine(s.F.Read());
        }
    }

}