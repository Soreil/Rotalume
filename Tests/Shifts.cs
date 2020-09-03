
using generator;

using NUnit.Framework;

namespace Tests
{
    public class Shifts
    {
        [Test]
        public void RLCA()
        {
            var dec = new Decoder(() => 0);

            {
                dec.Registers.Set(Register.A, 0x80);

                dec.StdOps[Unprefixed.RLCA]();

                Assert.AreEqual(1, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
            {
                dec.Registers.Set(Register.A, 0);

                dec.StdOps[Unprefixed.RLCA]();

                Assert.AreEqual(0, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }
            {
                dec.Registers.Set(Register.A, 0xff);

                dec.StdOps[Unprefixed.RLCA]();

                Assert.AreEqual(0xff, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
        }
    }
}