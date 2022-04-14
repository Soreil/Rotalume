
using emulator;

using NUnit.Framework;

namespace Tests;

public class Increments
{
    [Test]
    public void INC_SP()
    {
        var dec = TestHelpers.NewCore(new byte[] { (byte)Opcode.INC_SP });

        {
            dec.CPU.Registers.Set(WideRegister.SP, 20);
            var before = dec.CPU.Registers.Get(WideRegister.SP);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(before + 1, dec.CPU.Registers.Get(WideRegister.SP));
        }
    }

    [Test]
    public void INC_AT_HL()
    {
        var dec = TestHelpers.NewCore(new byte[]
        {
        (byte)Opcode.INC_AT_HL ,
        (byte)Opcode.INC_AT_HL ,
        (byte)Opcode.INC_AT_HL ,
        (byte)Opcode.INC_AT_HL}
        );
        {
            dec.Memory.Write(0xfffe, (ushort)0xFF);
            dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(0, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

            Assert.IsTrue(!dec.CPU.Registers.Negative);
            Assert.IsTrue(dec.CPU.Registers.Zero);
            Assert.IsTrue(dec.CPU.Registers.Half);
        }
        {
            dec.Memory.Write(0xfffe, (ushort)0xFE);
            dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(0xff, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

            Assert.IsTrue(!dec.CPU.Registers.Negative);
            Assert.IsTrue(!dec.CPU.Registers.Zero);
            Assert.IsTrue(!dec.CPU.Registers.Half);
        }
        {
            dec.Memory.Write(0xfffe, (ushort)0x0F);
            dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(0x10, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

            Assert.IsTrue(!dec.CPU.Registers.Negative);
            Assert.IsTrue(!dec.CPU.Registers.Zero);
            Assert.IsTrue(dec.CPU.Registers.Half);
        }
        {
            dec.Memory.Write(0xfffe, (ushort)0x0E);
            dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(0x0F, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

            Assert.IsTrue(!dec.CPU.Registers.Negative);
            Assert.IsTrue(!dec.CPU.Registers.Zero);
            Assert.IsFalse(dec.CPU.Registers.Half);
        }
    }

    [Test]
    public void INC_A()
    {
        var dec = TestHelpers.NewCore(new byte[] { (byte)Opcode.INC_A });
        {
            dec.CPU.Registers.Set(Register.A, 20);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(21, dec.CPU.Registers.Get(Register.A));

            Assert.IsTrue(!dec.CPU.Registers.Negative);
            Assert.IsTrue(!dec.CPU.Registers.Zero);
            Assert.IsTrue(!dec.CPU.Registers.Half);
        }
    }

    [Test]
    public void DEC_A()
    {
        var dec = TestHelpers.NewCore(new byte[]
        {
            (byte)Opcode.DEC_A,
            (byte)Opcode.DEC_A,
            (byte)Opcode.DEC_A,
        });

        {
            dec.CPU.Registers.Set(Register.A, 20);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(19, dec.CPU.Registers.Get(Register.A));

            Assert.IsTrue(dec.CPU.Registers.Negative);
            Assert.IsTrue(!dec.CPU.Registers.Zero);
            Assert.IsTrue(!dec.CPU.Registers.Half);
        }
        {
            dec.CPU.Registers.Set(Register.A, 0x10);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(0xf, dec.CPU.Registers.Get(Register.A));

            Assert.IsTrue(dec.CPU.Registers.Negative);
            Assert.IsTrue(!dec.CPU.Registers.Zero);
            Assert.IsTrue(dec.CPU.Registers.Half);
        }
        {
            dec.CPU.Registers.Set(Register.A, 0x0F);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.AreEqual(0x0E, dec.CPU.Registers.Get(Register.A));

            Assert.IsTrue(dec.CPU.Registers.Negative);
            Assert.IsTrue(!dec.CPU.Registers.Zero);
            Assert.IsTrue(!dec.CPU.Registers.Half);
        }
    }
    [Test]
    public void ADD_A_B()
    {
        var core = TestHelpers.NewCore(new byte[]
        {
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.ADD_A_B,
        });

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x73);
            dec.Registers.Set(Register.B, 0x26);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0x99, dec.Registers.Get(Register.A));
        }
        {
            dec.Registers.Set(Register.A, 0xF0);
            dec.Registers.Set(Register.B, 0x0F);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0xFF, dec.Registers.Get(Register.A));
            Assert.IsTrue(!dec.Registers.Carry);
        }
        {
            dec.Registers.Set(Register.A, 0xF0);
            dec.Registers.Set(Register.B, 0x10);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0, dec.Registers.Get(Register.A));
            Assert.IsTrue(dec.Registers.Carry);
        }
    }
    [Test]
    public void DAA_wrap_around()
    {
        var core = TestHelpers.NewCore(new byte[]
        {
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        });

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x73);
            dec.Registers.Set(Register.B, 0x27);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0x9a, dec.Registers.Get(Register.A));

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0x00, dec.Registers.Get(Register.A));
            Assert.IsTrue(dec.Registers.Carry);
        }
    }
    [Test]
    public void DAA_99()
    {
        var core = TestHelpers.NewCore(new byte[]
        {
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        });

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x73);
            dec.Registers.Set(Register.B, 0x26);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0x99, dec.Registers.Get(Register.A));

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0x99, dec.Registers.Get(Register.A));
            Assert.IsTrue(!dec.Registers.Carry);
        }

    }
    [Test]

    public void DAA_0109()
    {
        var core = TestHelpers.NewCore(new byte[]
        {
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        });

        var dec = core.CPU;

        dec.Registers.Set(Register.A, 0x01);
        dec.Registers.Set(Register.B, 0x09);

        TestHelpers.StepOneCPUInstruction(core);

        Assert.AreEqual(0x0a, dec.Registers.Get(Register.A));

        TestHelpers.StepOneCPUInstruction(core);

        Assert.AreEqual(0x10, dec.Registers.Get(Register.A));
    }
    [Test]
    public void DAA_00()
    {
        var core = TestHelpers.NewCore(new byte[]
        {
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        });

        var dec = core.CPU;

        dec.Registers.Set(Register.A, 0x99);
        dec.Registers.Set(Register.B, 0x01);

        TestHelpers.StepOneCPUInstruction(core);

        Assert.AreEqual(0x9a, dec.Registers.Get(Register.A));
        Assert.True(!dec.Registers.Half);
        Assert.True(!dec.Registers.Negative);

        TestHelpers.StepOneCPUInstruction(core);

        Assert.AreEqual(0, dec.Registers.Get(Register.A));

        Assert.True(dec.Registers.Carry);
        Assert.True(!dec.Registers.Half);
        Assert.True(dec.Registers.Zero);
    }

    [Test]
    public void DAA_SUB_1009()
    {
        var core = TestHelpers.NewCore(new byte[]
        {
            (byte)Opcode.SUB_B,
            (byte)Opcode.DAA,
        });

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x10);
            dec.Registers.Set(Register.B, 0x01);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0x0f, dec.Registers.Get(Register.A));

            TestHelpers.StepOneCPUInstruction(core);

            Assert.AreEqual(0x09, dec.Registers.Get(Register.A));
        }

    }
    [Test]
    public void DAA_83()
    {
        var core = TestHelpers.NewCore(new byte[] { (byte)Opcode.ADD_A_B, (byte)Opcode.DAA });

        var dec = core.CPU;
        {
            dec.Registers.Set(Register.A, 0x45);
            dec.Registers.Set(Register.B, 0x38);

            dec.Op(Opcode.ADD_A_B)();

            Assert.AreEqual(0x7D, dec.Registers.Get(Register.A));

            dec.Op(Opcode.DAA)();

            Assert.AreEqual(0x83, dec.Registers.Get(Register.A));
            Assert.IsTrue(!dec.Registers.Carry);
        }

    }
}
