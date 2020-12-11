using System.Collections.Generic;

using emulator;

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
            Assert.AreEqual(4, p.Clock);
        }

        [Test]
        public void JR_NZ_r8()
        {
            var p = new BootBase(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, 0x05}
            );

            p.DoNextOP();
            Assert.AreEqual(7, p.PC);
            Assert.AreEqual(12, p.Clock);

            p = new BootBase(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, 0x05}
            );
            p.dec.Registers.Mark(Flag.Z);

            p.DoNextOP();
            Assert.AreEqual(2, p.PC);
            Assert.AreEqual(8, p.Clock);
        }
    }
}