using emulator;

using NUnit.Framework;

namespace Tests
{
    internal class Clock
    {
        private readonly List<Opcode> illegalOps = new List<Opcode> {Opcode.PREFIX,Opcode.ILLEGAL_D3,Opcode.ILLEGAL_DB,Opcode.ILLEGAL_DD,Opcode.ILLEGAL_E3,
            Opcode.ILLEGAL_E4,Opcode.ILLEGAL_EB, Opcode.ILLEGAL_EC,Opcode.ILLEGAL_ED,Opcode.ILLEGAL_F4,Opcode.ILLEGAL_FC,Opcode.ILLEGAL_FD,
        Opcode.STOP,Opcode.HALT};

        [Test]
        public void C2DoesNotThrow()
        {
            var p = TestHelpers.NewCore(new byte[] { 0xc2, 0, 0 });
            p.Step();
        }

        [Test]
        public void All_Opcodes_Take_time()
        {
            for (var i = 0; i < 0x100; i++)
            {
                if (illegalOps.Contains((Opcode)i)) continue; //FIXME: when we implement halt and stop there should be some time taken here most likely.

                var p = TestHelpers.NewCore(new byte[] { (byte)i, 0, 0 });

                p.Step();
                Assert.AreNotEqual(0, p.masterclock);
            }
            for (var i = 0; i < 0x100; i++)
            {
                var p = TestHelpers.NewCore(new byte[] { 0xCB, (byte)i, 0, 0 });

                p.Step();
                Assert.AreNotEqual(0, p.masterclock);
            }
        }
    }
}
