using emulator;

using NUnit.Framework;

namespace Tests
{
    public class Jumps
    {
        [Test]
        public void NO_OP()
        {
            var p = TestHelpers.NewCore(new byte[] {
            (byte)Opcode.NOP
            });
            p.Step();
            Assert.AreEqual(0x101, p.CPU.PC);
            Assert.AreEqual(4, p.CPU.TicksWeAreWaitingFor);
        }

        [Test]
        public void POP()
        {
            var p = TestHelpers.NewCore(new byte[] {
            (byte)Opcode.POP_AF
            });

            p.Memory.Write(0x0fffd, 0x8020);
            p.CPU.Registers.SP = 0xfffd;
            p.Step();
            Assert.AreEqual(0x101, p.CPU.PC);
            Assert.AreEqual(12, p.CPU.TicksWeAreWaitingFor);
            Assert.AreEqual(0xffff, p.CPU.Registers.SP);
            Assert.AreEqual(0x8020, p.CPU.Registers.AF);
        }

        /*set_test 5,"POP AF"
          ld   bc,$1200
     -    push bc
          pop  af
          push af
          pop  de
          ld   a,c
          and  $F0
          cp   e
          jp   nz,test_failed
          inc  b
          inc  c
          jr   nz,-*/
        [Test]
        public void POP_AF()
        {
            var inst = new byte[] {
            (byte)Opcode.LD_BC_d16,00,0x12,
                (byte)Opcode.PUSH_BC,
                (byte)Opcode.POP_AF,
                (byte)Opcode.PUSH_AF,
                (byte)Opcode.POP_DE,
                (byte)Opcode.LD_A_C,
                (byte)Opcode.AND_d8,0xf0,
                (byte)Opcode.CP_E,
            };
            var p = TestHelpers.NewCore(inst);
            p.CPU.Registers.SP = 0xffff;

            while (p.CPU.PC != 0x100 + inst.Length)
                p.Step();
            Assert.AreEqual(0x10b, p.CPU.PC);
            Assert.IsTrue(p.CPU.Registers.Zero);
        }

        [Test]
        public void PUSH()
        {
            var p = TestHelpers.NewCore(new byte[] { (byte)Opcode.PUSH_AF });

            p.CPU.Registers.SP = 0xfffe;
            p.CPU.Registers.AF = 0x12f0;

            p.Step();
            var read = p.Memory.ReadWide(0x0fffc);

            Assert.AreEqual(0x101, p.CPU.PC);
            Assert.AreEqual(16, p.CPU.TicksWeAreWaitingFor);
            Assert.AreEqual(0xfffc, p.CPU.Registers.SP);

            Assert.AreEqual(0x12f0, read);
        }

        [Test]
        public void RET()
        {
            var p = TestHelpers.NewCore(new byte[] { (byte)Opcode.RET });

            p.CPU.Registers.SP = 0xfffd;
            p.Memory.Write(0xfffd, 0xfedc);

            p.Step();

            Assert.AreEqual(0xfedc, p.CPU.PC);
            Assert.AreEqual(16, p.CPU.TicksWeAreWaitingFor);
            Assert.AreEqual(0xffff, p.CPU.Registers.SP);
        }

        [Test]
        public void CALL()
        {
            var p = TestHelpers.NewCore(new byte[] {
            (byte)Opcode.CALL_a16,0xab,0xcd
            });

            p.CPU.Registers.SP = 0xffff;
            p.Step();

            Assert.AreEqual(0xcdab, p.CPU.PC);
            Assert.AreEqual(24, p.CPU.TicksWeAreWaitingFor);
            Assert.AreEqual(0xfffd, p.CPU.Registers.SP);
            Assert.AreEqual(0x103, p.Memory.ReadWide(0xfffd));
        }

        [Test]
        public void JR_NZ_r8()
        {
            var p = TestHelpers.NewCore(new byte[]
            {
                (byte)Opcode.JR_NZ_r8, 0x05}
            );
            p.CPU.Registers.Zero = false;

            p.Step();
            Assert.AreEqual(0x107, p.CPU.PC);
            Assert.AreEqual(12, p.CPU.TicksWeAreWaitingFor);

            p = TestHelpers.NewCore(new byte[]
            { (byte)Opcode.JR_NZ_r8, unchecked((byte)-2)}
            );
            p.CPU.Registers.Zero = false;

            p.Step();
            Assert.AreEqual(0x100, p.CPU.PC);
            Assert.AreEqual(12, p.CPU.TicksWeAreWaitingFor);

            p = TestHelpers.NewCore(new byte[]
            { (byte)Opcode.JR_NZ_r8, 0x05}
            );
            p.CPU.Registers.Zero = true;

            p.Step();
            Assert.AreEqual(0x102, p.CPU.PC);
            Assert.AreEqual(8, p.CPU.TicksWeAreWaitingFor);
        }

        [Test]
        public void JR_r8()
        {
            var p = TestHelpers.NewCore(new byte[]
            { (byte)Opcode.JR_r8, 0x05}
            );

            p.Step();
            Assert.AreEqual(0x107, p.CPU.PC);
            Assert.AreEqual(12, p.CPU.TicksWeAreWaitingFor);

            p = TestHelpers.NewCore(new byte[] { (byte)Opcode.JR_r8, unchecked((byte)-2) });

            p.Step();
            Assert.AreEqual(0x100, p.CPU.PC);
            Assert.AreEqual(12, p.CPU.TicksWeAreWaitingFor);


        }
    }
}