
using emulator;

using NUnit.Framework;

namespace Tests
{
    public class Increments
    {
        [Test]
        public void INC_SP()
        {
            var dec = new Core(new System.Collections.Generic.List<byte> { (byte)Unprefixed.INC_SP });

            {
                dec.CPU.Registers.Set(WideRegister.SP, 20);
                var before = dec.CPU.Registers.Get(WideRegister.SP);

                dec.DoNextOP();

                Assert.AreEqual(before + 1, dec.CPU.Registers.Get(WideRegister.SP));
            }
        }

        [Test]
        public void INC_AT_HL()
        {
            var dec = new Core(new System.Collections.Generic.List<byte>
            {
            (byte)Unprefixed.INC_AT_HL ,
            (byte)Unprefixed.INC_AT_HL ,
            (byte)Unprefixed.INC_AT_HL ,
            (byte)Unprefixed.INC_AT_HL}
            );
            {
                dec.CPU.Memory.Write(0xfffe, (ushort)0xFF);
                dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

                dec.Step();

                Assert.AreEqual(0, dec.CPU.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.Z));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.H));
            }
            {
                dec.CPU.Memory.Write(0xfffe, (ushort)0xFE);
                dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

                dec.Step();

                Assert.AreEqual(0xff, dec.CPU.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NH));
            }
            {
                dec.CPU.Memory.Write(0xfffe, (ushort)0x0F);
                dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

                dec.Step();

                Assert.AreEqual(0x10, dec.CPU.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.H));
            }
            {
                dec.CPU.Memory.Write(0xfffe, (ushort)0x0E);
                dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

                dec.Step();

                Assert.AreEqual(0x0F, dec.CPU.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NZ));
                Assert.IsFalse(dec.CPU.Registers.Get(Flag.H));
            }
        }

        [Test]
        public void INC_A()
        {
            var dec = new Core(new System.Collections.Generic.List<byte> { (byte)Unprefixed.INC_A });
            {
                dec.CPU.Registers.Set(Register.A, 20);

                dec.DoNextOP();

                Assert.AreEqual(21, dec.CPU.Registers.Get(Register.A));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NN));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NH));
            }
        }

        [Test]
        public void DEC_A()
        {
            var dec = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.DEC_A,
                (byte)Unprefixed.DEC_A,
                (byte)Unprefixed.DEC_A,
            });

            {
                dec.CPU.Registers.Set(Register.A, 20);

                dec.DoNextOP();

                Assert.AreEqual(19, dec.CPU.Registers.Get(Register.A));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.N));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NH));
            }
            {
                dec.CPU.Registers.Set(Register.A, 0x10);

                dec.DoNextOP();

                Assert.AreEqual(0xf, dec.CPU.Registers.Get(Register.A));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.N));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.H));
            }
            {
                dec.CPU.Registers.Set(Register.A, 0x0F);

                dec.DoNextOP();

                Assert.AreEqual(0x0E, dec.CPU.Registers.Get(Register.A));

                Assert.IsTrue(dec.CPU.Registers.Get(Flag.N));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NZ));
                Assert.IsTrue(dec.CPU.Registers.Get(Flag.NH));
            }
        }
        [Test]
        public void ADD_A_B()
        {
            var core = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.ADD_A_B,
                (byte)Unprefixed.ADD_A_B,
                (byte)Unprefixed.ADD_A_B,
            });

            var dec = core.CPU;

            {
                dec.Registers.Set(Register.A, 0x73);
                dec.Registers.Set(Register.B, 0x26);

                core.DoNextOP();

                Assert.AreEqual(0x99, dec.Registers.Get(Register.A));
            }
            {
                dec.Registers.Set(Register.A, 0xF0);
                dec.Registers.Set(Register.B, 0x0F);

                core.DoNextOP();

                Assert.AreEqual(0xFF, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }
            {
                dec.Registers.Set(Register.A, 0xF0);
                dec.Registers.Set(Register.B, 0x10);

                core.DoNextOP();

                Assert.AreEqual(0, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
        }
        [Test]
        public void DAA_wrap_around()
        {
            var core = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.ADD_A_B,
                (byte)Unprefixed.DAA,
            });

            var dec = core.CPU;

            {
                dec.Registers.Set(Register.A, 0x73);
                dec.Registers.Set(Register.B, 0x27);

                core.DoNextOP();

                Assert.AreEqual(0x9a, dec.Registers.Get(Register.A));

                core.DoNextOP();

                Assert.AreEqual(0x00, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.C));
            }
        }
        [Test]
        public void DAA_99()
        {
            var core = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.ADD_A_B,
                (byte)Unprefixed.DAA,
            });

            var dec = core.CPU;

            {
                dec.Registers.Set(Register.A, 0x73);
                dec.Registers.Set(Register.B, 0x26);

                core.DoNextOP();

                Assert.AreEqual(0x99, dec.Registers.Get(Register.A));

                core.DoNextOP();

                Assert.AreEqual(0x99, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }

        }
        [Test]

        public void DAA_0109()
        {
            var core = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.ADD_A_B,
                (byte)Unprefixed.DAA,
            });

            var dec = core.CPU;

            dec.Registers.Set(Register.A, 0x01);
            dec.Registers.Set(Register.B, 0x09);

            core.DoNextOP();

            Assert.AreEqual(0x0a, dec.Registers.Get(Register.A));

            core.DoNextOP();

            Assert.AreEqual(0x10, dec.Registers.Get(Register.A));
        }
        [Test]
        public void DAA_00()
        {
            var core = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.ADD_A_B,
                (byte)Unprefixed.DAA,
            });

            var dec = core.CPU;

            dec.Registers.Set(Register.A, 0x99);
            dec.Registers.Set(Register.B, 0x01);

            core.DoNextOP();

            Assert.AreEqual(0x9a, dec.Registers.Get(Register.A));
            Assert.True(dec.Registers.Get(Flag.NH));
            Assert.True(dec.Registers.Get(Flag.NN));

            core.DoNextOP();

            Assert.AreEqual(0, dec.Registers.Get(Register.A));

            Assert.True(dec.Registers.Get(Flag.C));
            Assert.True(dec.Registers.Get(Flag.NH));
            Assert.True(dec.Registers.Get(Flag.Z));
        }

        [Test]
        public void DAA_SUB_1009()
        {
            var core = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.SUB_B,
                (byte)Unprefixed.DAA,
            });

            var dec = core.CPU;

            {
                dec.Registers.Set(Register.A, 0x10);
                dec.Registers.Set(Register.B, 0x01);

                core.DoNextOP();

                Assert.AreEqual(0x0f, dec.Registers.Get(Register.A));

                core.DoNextOP();

                Assert.AreEqual(0x09, dec.Registers.Get(Register.A));
            }

        }
        [Test]
        public void DAA_83()
        {
            var core = new Core(new System.Collections.Generic.List<byte>
            {
                (byte)Unprefixed.ADD_A_B,
                (byte)Unprefixed.DAA,
            });

            var dec = core.CPU;
            {
                dec.Registers.Set(Register.A, 0x45);
                dec.Registers.Set(Register.B, 0x38);

                dec.Op(Unprefixed.ADD_A_B)();

                Assert.AreEqual(0x7D, dec.Registers.Get(Register.A));

                dec.Op(Unprefixed.DAA)();

                Assert.AreEqual(0x83, dec.Registers.Get(Register.A));
                Assert.IsTrue(dec.Registers.Get(Flag.NC));
            }

        }
    }
}