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
            Assert.AreEqual(1, p.PC);
            Assert.AreEqual(4, p.Clock);
        }

        [Test]
        public void POP()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.POP_AF
            });

            p.CPU.Memory.Write(0x0fe, (ushort)0x3020);
            p.CPU.Registers.SP = 0xfe;
            p.DoNextOP();
            Assert.AreEqual(1, p.PC);
            Assert.AreEqual(12, p.Clock);
            Assert.AreEqual(0x100, p.CPU.Registers.SP);
            Assert.AreEqual(0x3020, p.CPU.Registers.AF);
        }

        [Test]
        public void PUSH()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.PUSH_AF
            });

            p.CPU.Registers.SP = 0x100;
            p.CPU.Registers.AF = 0x1234;

            p.DoNextOP();
            var read = p.CPU.Memory.ReadWide(0x0fe);

            Assert.AreEqual(1, p.PC);
            Assert.AreEqual(16, p.Clock);
            Assert.AreEqual(0xfe, p.CPU.Registers.SP);

            Assert.AreEqual(0x1234, read);
        }

        [Test]
        public void RET()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.RET
            });

            p.CPU.Registers.SP = 0x100;
            p.CPU.Memory.Write(0x100, 0xfedc);

            p.DoNextOP();

            Assert.AreEqual(0xfedc, p.PC);
            Assert.AreEqual(16, p.Clock);
            Assert.AreEqual(0x102, p.CPU.Registers.SP);
        }

        [Test]
        public void CALL()
        {
            var p = new Core(new List<byte> {
            (byte)Unprefixed.CALL_a16,(byte)0xab,(byte)0xcd
            });

            p.CPU.Registers.SP = 0x100;
            p.DoNextOP();

            Assert.AreEqual(0xcdab, p.PC);
            Assert.AreEqual(24, p.Clock);
            Assert.AreEqual(0xfe, p.CPU.Registers.SP);
            Assert.AreEqual(0x3, p.CPU.Memory.ReadWide(0xfe));
        }

        [Test]
        public void JR_NZ_r8()
        {
            var p = new Core(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, 0x05}
            );

            p.DoNextOP();
            Assert.AreEqual(7, p.PC);
            Assert.AreEqual(12, p.Clock);

            p = new Core(new List<byte>
            { (byte)Unprefixed.JR_NZ_r8, 0x05}
            );
            p.CPU.Registers.Mark(Flag.Z);

            p.DoNextOP();
            Assert.AreEqual(2, p.PC);
            Assert.AreEqual(8, p.Clock);
        }
    }
}