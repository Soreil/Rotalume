using emulator.opcodes;
using emulator.registers;

using NUnit.Framework;

namespace Tests;

public class Increments
{
    [Test]
    public void INC_SP()
    {
        var dec = TestHelpers.NewCore([(byte)Opcode.INC_SP]);

        {
            dec.CPU.Registers.Set(WideRegister.SP, 20);
            var before = dec.CPU.Registers.Get(WideRegister.SP);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.That(before + 1, Is.EqualTo(dec.CPU.Registers.Get(WideRegister.SP)));
        }
    }

    //[Test]
    //public void INC_AT_HL()
    //{
    //    var dec = TestHelpers.NewCore(new byte[]
    //    {
    //    (byte)Opcode.INC_AT_HL ,
    //    (byte)Opcode.INC_AT_HL ,
    //    (byte)Opcode.INC_AT_HL ,
    //    (byte)Opcode.INC_AT_HL}
    //    );
    //    {
    //        dec.Memory.Write(0xfffe, (ushort)0xFF);
    //        dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

    //        TestHelpers.StepOneCPUInstruction(dec);

    //        Assert.AreEqual(0, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

    //        Assert.IsTrue(!dec.CPU.Registers.Negative);
    //        Assert.IsTrue(dec.CPU.Registers.Zero);
    //        Assert.IsTrue(dec.CPU.Registers.Half);
    //    }
    //    {
    //        dec.Memory.Write(0xfffe, (ushort)0xFE);
    //        dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

    //        TestHelpers.StepOneCPUInstruction(dec);

    //        Assert.AreEqual(0xff, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

    //        Assert.IsTrue(!dec.CPU.Registers.Negative);
    //        Assert.IsTrue(!dec.CPU.Registers.Zero);
    //        Assert.IsTrue(!dec.CPU.Registers.Half);
    //    }
    //    {
    //        dec.Memory.Write(0xfffe, (ushort)0x0F);
    //        dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

    //        TestHelpers.StepOneCPUInstruction(dec);

    //        Assert.AreEqual(0x10, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

    //        Assert.IsTrue(!dec.CPU.Registers.Negative);
    //        Assert.IsTrue(!dec.CPU.Registers.Zero);
    //        Assert.IsTrue(dec.CPU.Registers.Half);
    //    }
    //    {
    //        dec.Memory.Write(0xfffe, (ushort)0x0E);
    //        dec.CPU.Registers.Set(WideRegister.HL, 0xfffe);

    //        TestHelpers.StepOneCPUInstruction(dec);

    //        Assert.AreEqual(0x0F, dec.Memory.Read(dec.CPU.Registers.Get(WideRegister.HL)));

    //        Assert.IsTrue(!dec.CPU.Registers.Negative);
    //        Assert.IsTrue(!dec.CPU.Registers.Zero);
    //        Assert.IsFalse(dec.CPU.Registers.Half);
    //    }
    //}

    [Test]
    public void INC_A()
    {
        var dec = TestHelpers.NewCore([(byte)Opcode.INC_A]);
        {
            dec.CPU.Registers.Set(Register.A, 20);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.That(21, Is.EqualTo(dec.CPU.Registers.Get(Register.A)));

            Assert.That(dec.CPU.Registers.Negative,Is.False);
            Assert.That(dec.CPU.Registers.Zero,Is.False);
            Assert.That(dec.CPU.Registers.Half,Is.False);
        }
    }

    [Test]
    public void DEC_A()
    {
        var dec = TestHelpers.NewCore(
        [
            (byte)Opcode.DEC_A,
            (byte)Opcode.DEC_A,
            (byte)Opcode.DEC_A,
        ]);

        {
            dec.CPU.Registers.Set(Register.A, 20);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.That(19, Is.EqualTo(dec.CPU.Registers.Get(Register.A)));

            Assert.That(dec.CPU.Registers.Negative,Is.True);
            Assert.That(dec.CPU.Registers.Zero,Is.False);
            Assert.That(dec.CPU.Registers.Half,Is.False);
        }
        {
            dec.CPU.Registers.Set(Register.A, 0x10);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.That(0xf, Is.EqualTo(dec.CPU.Registers.Get(Register.A)));

            Assert.That(dec.CPU.Registers.Negative,Is.True);
            Assert.That(dec.CPU.Registers.Zero,Is.False);
            Assert.That(dec.CPU.Registers.Half,Is.True);
        }
        {
            dec.CPU.Registers.Set(Register.A, 0x0F);

            TestHelpers.StepOneCPUInstruction(dec);

            Assert.That(0x0E, Is.EqualTo(dec.CPU.Registers.Get(Register.A)));

            Assert.That(dec.CPU.Registers.Negative,Is.True);
            Assert.That(dec.CPU.Registers.Zero,Is.False);
            Assert.That(dec.CPU.Registers.Half,Is.False);
        }
    }
    [Test]
    public void ADD_A_B()
    {
        var core = TestHelpers.NewCore(
        [
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.ADD_A_B,
        ]);

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x73);
            dec.Registers.Set(Register.B, 0x26);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0x99, Is.EqualTo(dec.Registers.Get(Register.A)));
        }
        {
            dec.Registers.Set(Register.A, 0xF0);
            dec.Registers.Set(Register.B, 0x0F);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0xFF, Is.EqualTo(dec.Registers.Get(Register.A)));
            Assert.That(dec.Registers.Carry,Is.False);
        }
        {
            dec.Registers.Set(Register.A, 0xF0);
            dec.Registers.Set(Register.B, 0x10);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0, Is.EqualTo(dec.Registers.Get(Register.A)));
            Assert.That(dec.Registers.Carry,Is.True);
        }
    }
    [Test]
    public void DAA_wrap_around()
    {
        var core = TestHelpers.NewCore(
        [
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        ]);

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x73);
            dec.Registers.Set(Register.B, 0x27);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0x9a, Is.EqualTo(dec.Registers.Get(Register.A)));

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0x00, Is.EqualTo(dec.Registers.Get(Register.A)));
            Assert.That(dec.Registers.Carry,Is.True);
        }
    }
    [Test]
    public void DAA_99()
    {
        var core = TestHelpers.NewCore(
        [
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        ]);

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x73);
            dec.Registers.Set(Register.B, 0x26);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0x99, Is.EqualTo(dec.Registers.Get(Register.A)));

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0x99, Is.EqualTo(dec.Registers.Get(Register.A)));
            Assert.That(dec.Registers.Carry,Is.False);
        }

    }
    [Test]

    public void DAA_0109()
    {
        var core = TestHelpers.NewCore(
        [
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        ]);

        var dec = core.CPU;

        dec.Registers.Set(Register.A, 0x01);
        dec.Registers.Set(Register.B, 0x09);

        TestHelpers.StepOneCPUInstruction(core);

        Assert.That(0x0a, Is.EqualTo(dec.Registers.Get(Register.A)));

        TestHelpers.StepOneCPUInstruction(core);

        Assert.That(0x10, Is.EqualTo(dec.Registers.Get(Register.A)));
    }
    [Test]
    public void DAA_00()
    {
        var core = TestHelpers.NewCore(
        [
            (byte)Opcode.ADD_A_B,
            (byte)Opcode.DAA,
        ]);

        var dec = core.CPU;

        dec.Registers.Set(Register.A, 0x99);
        dec.Registers.Set(Register.B, 0x01);

        TestHelpers.StepOneCPUInstruction(core);

        Assert.That(0x9a, Is.EqualTo(dec.Registers.Get(Register.A)));
        Assert.That(dec.Registers.Half,Is.False);
        Assert.That(dec.Registers.Negative,Is.False);

        TestHelpers.StepOneCPUInstruction(core);

        Assert.That(0, Is.EqualTo(dec.Registers.Get(Register.A)));
        Assert.That(dec.Registers.Carry,Is.True);
        Assert.That(dec.Registers.Half,Is.False);
        Assert.That(dec.Registers.Zero,Is.True);
    }

    [Test]
    public void DAA_SUB_1009()
    {
        var core = TestHelpers.NewCore(
        [
            (byte)Opcode.SUB_B,
            (byte)Opcode.DAA,
        ]);

        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x10);
            dec.Registers.Set(Register.B, 0x01);

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0x0f, Is.EqualTo(dec.Registers.Get(Register.A)));

            TestHelpers.StepOneCPUInstruction(core);

            Assert.That(0x09, Is.EqualTo(dec.Registers.Get(Register.A)));
        }

    }
    [Test]
    public void DAA_83()
    {
        var core = TestHelpers.NewCore([(byte)Opcode.ADD_A_B, (byte)Opcode.DAA]);

        var dec = core.CPU;
        {
            dec.Registers.Set(Register.A, 0x45);
            dec.Registers.Set(Register.B, 0x38);

            dec.Op(Opcode.ADD_A_B)();

            Assert.That(0x7D, Is.EqualTo(dec.Registers.Get(Register.A)));

            dec.Op(Opcode.DAA)();

            Assert.That(0x83, Is.EqualTo(dec.Registers.Get(Register.A)));
            Assert.That(dec.Registers.Carry,Is.False);
        }

    }
}
