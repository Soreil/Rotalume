﻿
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
        [Test]
        public void RRCA()
        {
            var dec = new Decoder(() => 0);

            {
                dec.Registers.Set(Register.A, 0x01);

                dec.StdOps[Unprefixed.RRCA]();

                Assert.AreEqual(0x80, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
            {
                dec.Registers.Set(Register.A, 0xFF);

                dec.StdOps[Unprefixed.RRCA]();

                Assert.AreEqual(0xFF, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
            {
                dec.Registers.Set(Register.A, 0);

                dec.StdOps[Unprefixed.RRCA]();

                Assert.AreEqual(0, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }
        }
        [Test]
        public void CPL()
        {
            var dec = new Decoder(() => 0);

            dec.Registers.A.Write(0xff);
            dec.StdOps[Unprefixed.CPL]();

            Assert.AreEqual(0, dec.Registers.A.Read());
        }
        [Test]
        public void SCF()
        {
            var dec = new Decoder(() => 0);

            dec.StdOps[Unprefixed.SCF]();

            Assert.IsTrue(dec.Registers.Get(Flag.C));
        }
        [Test]
        public void CCF()
        {
            var dec = new Decoder(() => 0);

            dec.StdOps[Unprefixed.CCF]();
            Assert.IsTrue(dec.Registers.Get(Flag.C));

            dec.StdOps[Unprefixed.CCF]();
            Assert.IsTrue(dec.Registers.Get(Flag.NC));
        }
    }
}