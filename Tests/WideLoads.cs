using emulator;

using NUnit.Framework;

namespace Tests
{
    public class WideLoads
    {
        [Test]
        public void LD_BC_d16()
        {
            var dec = SetupA020Buffered();

            var before = dec.Registers.BC;

            dec.Op(Unprefixed.LD_BC_d16)();

            var after = dec.Registers.BC;

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0xa020);
        }

        private static CPU SetupA020Buffered()
        {
            var c = TestHelpers.NewCore(new byte[] { 0x20, 0xa0 });
            return c.CPU;
        }

        [Test]
        public void LD_DE_d16()
        {
            var dec = SetupA020Buffered();

            var before = dec.Registers.DE;

            dec.Op(Unprefixed.LD_DE_d16)();

            var after = dec.Registers.DE;

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0xa020);
        }
        [Test]
        public void LD_HL_d16()
        {
            var dec = SetupA020Buffered();

            var before = dec.Registers.HL;

            dec.Op(Unprefixed.LD_HL_d16)();

            var after = dec.Registers.HL;

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0xa020);
        }
        [Test]
        public void LD_SP_d16()
        {
            var dec = SetupA020Buffered();

            var before = dec.Registers.SP;

            dec.Op(Unprefixed.LD_SP_d16)();

            var after = dec.Registers.SP;

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0xa020);
        }
        [Test]
        public void LD_SP_d16_v2()
        {
            var p = TestHelpers.NewCore(new byte[]
            { (byte)Unprefixed.LD_SP_d16, 0x12, 0x45 }
            );

            p.Step();
            Assert.AreEqual(0x103, p.CPU.PC);
            Assert.AreEqual(0x4512, p.CPU.Registers.SP);
            Assert.AreEqual(12, p.CPU.TicksWeAreWaitingFor);
        }

        [Test]
        public void LD_AT_HL_d8()
        {
            var dec = Setup0x77BufferedDecoder();

            dec.Registers.HL = 0xa000;
            var before = dec.Registers.HL;
            var memoryBefore = dec.Memory.Read(before);

            dec.Op(Unprefixed.LD_AT_HL_d8)();

            var after = dec.Registers.HL;
            var memoryAfter = dec.Memory.Read(after);

            Assert.AreEqual(before, after);
            Assert.AreNotEqual(memoryBefore, memoryAfter);

            Assert.AreEqual(0x77, memoryAfter);
        }

        private CPU Setup0x77BufferedDecoder()
        {
            var c = TestHelpers.NewCore(new byte[] { 0x77 });
            return c.CPU;
        }

        [Test]
        public void LDD_AT_HL_A()
        {
            var core = TestHelpers.NewCore(new byte[] { });
            var dec = core.CPU;
            dec.Registers.Set(WideRegister.HL, 0xa001);
            dec.Registers.Set(Register.A, 0x77);

            var before = dec.Registers.HL;
            var memoryBefore = dec.Memory.Read(before);

            dec.Op(Unprefixed.LDD_AT_HL_A)();

            var after = dec.Registers.HL;
            var memoryAfter = dec.Memory.Read(after);

            var memoryAfterButAtOldHL = dec.Memory.Read(before);

            Assert.AreEqual(before - 1, after);
            Assert.AreEqual(memoryBefore, memoryAfter);

            Assert.AreNotEqual(0x77, memoryAfter);
            Assert.AreEqual(0x77, memoryAfterButAtOldHL);
        }

        [Test]
        public void LDI_AT_HL_A()
        {
            var core = TestHelpers.NewCore(new byte[] { });
            var dec = core.CPU;
            dec.Registers.Set(WideRegister.HL, 0xa000);
            dec.Registers.Set(Register.A, 0x77);

            var before = dec.Registers.HL;
            var memoryBefore = dec.Memory.Read(before);

            dec.Op(Unprefixed.LDI_AT_HL_A)();

            var after = dec.Registers.HL;
            var memoryAfter = dec.Memory.Read(after);

            var memoryAfterButAtOldHL = dec.Memory.Read(before);

            Assert.AreEqual(before + 1, after);
            Assert.AreEqual(memoryBefore, memoryAfter);

            Assert.AreNotEqual(0x77, memoryAfter);
            Assert.AreEqual(0x77, memoryAfterButAtOldHL);
        }

        [Test]
        public void LD_AT_a16_SP()
        {
            var dec = SetupA020Buffered();
            dec.Registers.SP = (0xfffe);
            dec.Op(Unprefixed.LD_AT_a16_SP)();

            var result = dec.Memory.ReadWide(0xa020);
            Assert.AreEqual(0xfffe, result);
        }

        [Test]
        public void ADD_HL_BC()
        {
            var core = TestHelpers.NewCore(new byte[] { });
            var dec = core.CPU;

            dec.Registers.HL = (0x8a23);
            dec.Registers.BC = (0x0605);

            dec.Op(Unprefixed.ADD_HL_BC)();

            Assert.AreEqual(0x9028, dec.Registers.HL);
            Assert.IsTrue(dec.Registers.Get(Flag.H));
            Assert.IsTrue(dec.Registers.Get(Flag.NN));
            Assert.IsTrue(dec.Registers.Get(Flag.NC));
        }
        [Test]
        public void ADD_HL_HL()
        {
            var core = TestHelpers.NewCore(new byte[] { });
            var dec = core.CPU;

            dec.Registers.HL = (0x8a23);

            dec.Op(Unprefixed.ADD_HL_HL)();

            Assert.AreEqual(0x1446, dec.Registers.HL);
            Assert.IsTrue(dec.Registers.Get(Flag.H));
            Assert.IsTrue(dec.Registers.Get(Flag.NN));
            Assert.IsTrue(dec.Registers.Get(Flag.C));
        }

        [Test]
        public void LD_A_B()
        {
            var core = TestHelpers.NewCore(new byte[] { });
            var dec = core.CPU;

            dec.Registers.B = (0x10);

            dec.Op(Unprefixed.LD_A_B)();

            Assert.AreEqual(0x10, dec.Registers.A);

        }
        [Test]
        public void LD_AT_C_A()
        {
            var core = TestHelpers.NewCore(new byte[] { });
            var dec = core.CPU;

            dec.Registers.A = (0x10);
            dec.Registers.C = (0xfe);

            dec.Op(Unprefixed.LD_AT_C_A)();

            Assert.AreEqual(0x10, dec.Memory.Read(0xfffe));

        }
        [Test]
        public void LD_A_AT_C()
        {
            var core = TestHelpers.NewCore(new byte[] { });
            var dec = core.CPU;

            dec.Memory.Write(0xfffe, 0x10);
            dec.Registers.C = (0xfe);


            dec.Op(Unprefixed.LD_A_AT_C)();

            Assert.AreEqual(0x10, dec.Registers.A);

        }
    }
}