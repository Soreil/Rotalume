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
            var p = new Core(new List<byte> {
            (byte)Unprefixed.NOP
            });
            p.DoNextOP();
            Assert.AreEqual(0x101, p.PC);
            Assert.AreEqual(4, p.Clock);
        }

        [Test]
        public void POP()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.POP_AF
            });

            p.CPU.Memory.Write(0x0fffd, 0x8020);
            p.CPU.Registers.SP = 0xfffd;
            p.DoNextOP();
            Assert.AreEqual(0x101, p.PC);
            Assert.AreEqual(12, p.Clock);
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
            var inst = new List<byte> {
            (byte)Unprefixed.LD_BC_d16,00,0x12,
                (byte)Unprefixed.PUSH_BC,
                (byte)Unprefixed.POP_AF,
                (byte)Unprefixed.PUSH_AF,
                (byte)Unprefixed.POP_DE,
                (byte)Unprefixed.LD_A_C,
                (byte)Unprefixed.AND_d8,0xf0,
                (byte)Unprefixed.CP_E,
            };
            var p = new Core(inst);
            p.CPU.Registers.SP = 0xffff;

            while (p.PC != 0x100+inst.Count)
                p.DoNextOP();
            Assert.AreEqual(0x10b, p.PC);
            Assert.IsTrue(p.CPU.Registers.Get(Flag.Z));
        }

        [Test]
        public void PUSH()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.PUSH_AF
            });

            p.CPU.Registers.SP = 0xfffe;
            p.CPU.Registers.AF = 0x12f0;

            p.DoNextOP();
            var read = p.CPU.Memory.ReadWide(0x0fffc);

            Assert.AreEqual(0x101, p.PC);
            Assert.AreEqual(16, p.Clock);
            Assert.AreEqual(0xfffc, p.CPU.Registers.SP);

            Assert.AreEqual(0x12f0, read);
        }

        [Test]
        public void RET()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.RET
            });

            p.CPU.Registers.SP = 0xfffd;
            p.CPU.Memory.Write(0xfffd, 0xfedc);

            p.DoNextOP();

            Assert.AreEqual(0xfedc, p.PC);
            Assert.AreEqual(16, p.Clock);
            Assert.AreEqual(0xffff, p.CPU.Registers.SP);
        }

        [Test]
        public void CALL()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.CALL_a16,(byte)0xab,(byte)0xcd
            });

            p.CPU.Registers.SP = 0xffff;
            p.DoNextOP();

            Assert.AreEqual(0xcdab, p.PC);
            Assert.AreEqual(24, p.Clock);
            Assert.AreEqual(0xfffd, p.CPU.Registers.SP);
            Assert.AreEqual(0x103, p.CPU.Memory.ReadWide(0xfffd));
        }

        [Test]
        public void JR_NZ_r8()
        {
            var p = new Core(new List<byte>
            {
                (byte)Unprefixed.JR_NZ_r8, 0x05}
            );
            p.CPU.Registers.Set(Flag.Z, false);

            p.DoNextOP();
            Assert.AreEqual(0x107, p.PC);
            Assert.AreEqual(12, p.Clock);

            p = new Core(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, unchecked((byte)-2)}
            );
            p.CPU.Registers.Mark(Flag.NZ);

            p.DoNextOP();
            Assert.AreEqual(0x100, p.PC);
            Assert.AreEqual(12, p.Clock);

            p = new Core(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, 0x05}
            );
            p.CPU.Registers.Mark(Flag.Z);

            p.DoNextOP();
            Assert.AreEqual(0x102, p.PC);
            Assert.AreEqual(8, p.Clock);
        }

        [Test]
        public void JR_r8()
        {
            var p = new Core(new List<byte>
            { (byte)Unprefixed.JR_r8, 0x05}
            );

            p.DoNextOP();
            Assert.AreEqual(0x107, p.PC);
            Assert.AreEqual(12, p.Clock);

            p = new Core(new List<byte>
            { (byte)Unprefixed.JR_r8, unchecked((byte)-2)}
            );

            p.DoNextOP();
            Assert.AreEqual(0x100, p.PC);
            Assert.AreEqual(12, p.Clock);


        }
    }
}