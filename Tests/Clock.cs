using System.Collections.Generic;

using generator;

using NUnit.Framework;

namespace Tests
{
    class Clock
    {
        readonly List<Unprefixed> illegalOps = new List<Unprefixed> {Unprefixed.PREFIX,Unprefixed.ILLEGAL_D3,Unprefixed.ILLEGAL_DB,Unprefixed.ILLEGAL_DD,Unprefixed.ILLEGAL_E3,
            Unprefixed.ILLEGAL_E4,Unprefixed.ILLEGAL_EB, Unprefixed.ILLEGAL_EC,Unprefixed.ILLEGAL_ED,Unprefixed.ILLEGAL_F4,Unprefixed.ILLEGAL_FC,Unprefixed.ILLEGAL_FD,
        Unprefixed.STOP,Unprefixed.HALT};

        [Test]
        public void C2DoesNotThrow()
        {
            var p = new BootBase(new List<byte> { 0xc2, 0, 0 });
            p.DoNextOP();
        }

        [Test]
        public void All_Opcodes_Take_time()
        {
            for (int i = 0; i < 0x100; i++)
            {
                if (illegalOps.Contains((Unprefixed)i)) continue; //FIXME: when we implement halt and stop there should be some time taken here most likely.

                var p = new BootBase(new List<byte> { (byte)i, 0, 0 });

                p.DoNextOP();
                Assert.AreNotEqual(0, p.Clock);
            }
            for (int i = 0; i < 0x100; i++)
            {
                var p = new BootBase(new List<byte> { 0xCB, (byte)i, 0, 0 });

                p.DoNextOP();
                Assert.AreNotEqual(0, p.Clock);
            }
        }
    }
}
