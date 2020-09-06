using System.Collections.Generic;

using generator;

using NUnit.Framework;

namespace Tests
{
    public class Jumps
    {
        [Test]
        public void NO_OP()
        {
            var p = new BootBase(new List<byte> {
            (byte)Unprefixed.NOP
            });
            p.DoNextOP();
            Assert.AreEqual(1, p.PC);
        }

        [Test]
        public void JR_NZ_r8()
        {
            var p = new BootBase(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, 0x05}
            );

            p.DoNextOP();
            Assert.AreEqual(7, p.PC);

            p = new BootBase(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, 0x05}
            );
            p.dec.Registers.Mark(Flag.Z);

            p.DoNextOP();
            Assert.AreEqual(2, p.PC);
        }
    }
}