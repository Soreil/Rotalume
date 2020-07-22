using NUnit.Framework;
using generator;
using System;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ReadJSON()
        {
            var o = new Reader("..\\..\\..\\json\\Opcodes.json");
            foreach (var op in o.PossibleOperands())
                Console.WriteLine(op.Item1+ " " + op.Item2.ToString());
        }
    }
}