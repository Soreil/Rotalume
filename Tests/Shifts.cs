
using emulator;

using NUnit.Framework;

namespace Tests
{
    public class Shifts
    {
        [Test]
        public void RLCA()
        {
            var core = new Core(new System.Collections.Generic.List<byte> { });
            var dec = core.CPU;

            {
                dec.Registers.Set(Register.A, 0x80);

                dec.Op(Unprefixed.RLCA)();

                Assert.AreEqual(1, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
            {
                dec.Registers.Set(Register.A, 0);

                dec.Op(Unprefixed.RLCA)();

                Assert.AreEqual(0, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }
            {
                dec.Registers.Set(Register.A, 0xff);

                dec.Op(Unprefixed.RLCA)();

                Assert.AreEqual(0xff, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
        }
        [Test]
        public void RRCA()
        {
            var core = new Core(new System.Collections.Generic.List<byte> { });
            var dec = core.CPU;

            {
                dec.Registers.Set(Register.A, 0x01);

                dec.Op(Unprefixed.RRCA)();

                Assert.AreEqual(0x80, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
            {
                dec.Registers.Set(Register.A, 0xFF);

                dec.Op(Unprefixed.RRCA)();

                Assert.AreEqual(0xFF, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
            {
                dec.Registers.Set(Register.A, 0);

                dec.Op(Unprefixed.RRCA)();

                Assert.AreEqual(0, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }
        }
        [Test]
        public void CPL()
        {
            var core = new Core(new System.Collections.Generic.List<byte> { });
            var dec = core.CPU;

            dec.Registers.A = (0xff);
            dec.Op(Unprefixed.CPL)();

            Assert.AreEqual(0, dec.Registers.A);
        }
        [Test]
        public void SCF()
        {
            var core = new Core(new System.Collections.Generic.List<byte> { });
            var dec = core.CPU;

            dec.Op(Unprefixed.SCF)();

            Assert.IsTrue(dec.Registers.Get(Flag.C));
        }
        [Test]
        public void CCF()
        {
            var core = new Core(new System.Collections.Generic.List<byte> { });
            var dec = core.CPU;

            dec.Op(Unprefixed.CCF)();
            Assert.IsTrue(dec.Registers.Get(Flag.C));

            dec.Op(Unprefixed.CCF)();
            Assert.IsTrue(dec.Registers.Get(Flag.NC));
        }
    }
}