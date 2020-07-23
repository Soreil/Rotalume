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
            o.PrintEnum();
            o.PrintEnumStrings();
        }
    }
}