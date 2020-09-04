
using generator;

using NUnit.Framework;

namespace Tests
{
    public class Increments
    {
        [Test]
        public void INC_SP()
        {
            var dec = new Decoder(() => 0);

            {
                dec.Registers.Set(WideRegister.SP, 20);
                var before = dec.Registers.Get(WideRegister.SP);

                dec.StdOps[Unprefixed.INC_SP]();

                Assert.AreEqual(before + 1, dec.Registers.Get(WideRegister.SP));
            }
        }

        [Test]
        public void INC_AT_HL()
        {
            var dec = new Decoder(() => 0);
            {
                dec.Storage.Write(0x0001, (ushort)0xFF);
                dec.Registers.Set(WideRegister.HL, 0x0001);

                dec.StdOps[Unprefixed.INC_AT_HL]();

                Assert.AreEqual(0, dec.Storage.Read(dec.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.Registers.Get(Flag.Z));
                Assert.IsTrue(dec.Registers.Get(Flag.H));
            }
            {
                dec.Storage.Write(0x0001, (ushort)0xFE);
                dec.Registers.Set(WideRegister.HL, 0x0001);

                dec.StdOps[Unprefixed.INC_AT_HL]();

                Assert.AreEqual(0xff, dec.Storage.Read(dec.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.Registers.Get(Flag.NH));
            }
            {
                dec.Storage.Write(0x0001, (ushort)0x0F);
                dec.Registers.Set(WideRegister.HL, 0x0001);

                dec.StdOps[Unprefixed.INC_AT_HL]();

                Assert.AreEqual(0x10, dec.Storage.Read(dec.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.Registers.Get(Flag.H));
            }
            {
                dec.Storage.Write(0x0001, (ushort)0x0E);
                dec.Registers.Set(WideRegister.HL, 0x0001);

                dec.StdOps[Unprefixed.INC_AT_HL]();

                Assert.AreEqual(0x0F, dec.Storage.Read(dec.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.Registers.Get(Flag.NZ));
                Assert.IsFalse(dec.Registers.Get(Flag.H));
            }
        }

        [Test]
        public void INC_A()
        {
            var dec = new Decoder(() => 0);

            {
                dec.Registers.Set(Register.A, 20);

                dec.StdOps[Unprefixed.INC_A]();

                Assert.AreEqual(21, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.Registers.Get(Flag.NH));
            }
        }

        [Test]
        public void DEC_A()
        {
            var dec = new Decoder(() => 0);

            {
                dec.Registers.Set(Register.A, 20);

                dec.StdOps[Unprefixed.DEC_A]();

                Assert.AreEqual(19, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.N));
                Assert.IsTrue(dec.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.Registers.Get(Flag.NH));
            }
            {
                dec.Registers.Set(Register.A, 0x10);

                dec.StdOps[Unprefixed.DEC_A]();

                Assert.AreEqual(0xf, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.N));
                Assert.IsTrue(dec.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.Registers.Get(Flag.H));
            }
            {
                dec.Registers.Set(Register.A, 0x0F);

                dec.StdOps[Unprefixed.DEC_A]();

                Assert.AreEqual(0x0E, dec.Registers.Get(Register.A));

                Assert.IsTrue(dec.Registers.Get(Flag.N));
                Assert.IsTrue(dec.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.Registers.Get(Flag.NH));
            }
        }
        [Test]
        public void ADD_A_B()
        {
            var dec = new Decoder(() => 0);

            {
                dec.Registers.Set(Register.A, 0x73);
                dec.Registers.Set(Register.B, 0x26);

                dec.StdOps[Unprefixed.ADD_A_B]();

                Assert.AreEqual(0x99, dec.Registers.Get(Register.A));
            }
            {
                dec.Registers.Set(Register.A, 0xF0);
                dec.Registers.Set(Register.B, 0x0F);

                dec.StdOps[Unprefixed.ADD_A_B]();

                Assert.AreEqual(0xFF, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }
            {
                dec.Registers.Set(Register.A, 0xF0);
                dec.Registers.Set(Register.B, 0x10);

                dec.StdOps[Unprefixed.ADD_A_B]();

                Assert.AreEqual(0, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
        }
        [Test]
        public void DAA_wrap_around()
        {
            var dec = new Decoder(() => 0);

            {
                dec.Registers.Set(Register.A, 0x73);
                dec.Registers.Set(Register.B, 0x27);

                dec.StdOps[Unprefixed.ADD_A_B]();

                Assert.AreEqual(0x9a, dec.Registers.Get(Register.A));

                dec.StdOps[Unprefixed.DAA]();

                Assert.AreEqual(0x00, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
        }
        [Test]
        public void DAA_99()
        {
            var dec = new Decoder(() => 0);


            {
                dec.Registers.Set(Register.A, 0x73);
                dec.Registers.Set(Register.B, 0x26);

                dec.StdOps[Unprefixed.ADD_A_B]();

                Assert.AreEqual(0x99, dec.Registers.Get(Register.A));

                dec.StdOps[Unprefixed.DAA]();

                Assert.AreEqual(0x99, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }

        }
        [Test]
        public void DAA_83()
        {
            var dec = new Decoder(() => 0);


            {
                dec.Registers.Set(Register.A, 0x45);
                dec.Registers.Set(Register.B, 0x38);

                dec.StdOps[Unprefixed.ADD_A_B]();

                Assert.AreEqual(0x7D, dec.Registers.Get(Register.A));

                dec.StdOps[Unprefixed.DAA]();

                Assert.AreEqual(0x83, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }

        }
    }
}