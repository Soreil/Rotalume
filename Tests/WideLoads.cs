using emulator.glue;
using emulator.opcodes;
using emulator.registers;

using NUnit.Framework;

namespace Tests;

public class WideLoads
{
    [Test]
    public void LD_BC_d16()
    {
        var dec = SetupA020Buffered();

        var before = dec.CPU.Registers.BC;

        dec.CPU.Op(Opcode.LD_BC_d16)();

        var after = dec.CPU.Registers.BC;

        Assert.That(before, Is.Not.EqualTo(after));
        Assert.That(after, Is.EqualTo(0xa020));
    }

    private static Core SetupA020Buffered()
    {
        var c = TestHelpers.NewCore([0x20, 0xa0]);
        return c;
    }

    [Test]
    public void LD_DE_d16()
    {
        var dec = SetupA020Buffered();

        var before = dec.CPU.Registers.DE;

        dec.CPU.Op(Opcode.LD_DE_d16)();

        var after = dec.CPU.Registers.DE;

        Assert.That(before, Is.Not.EqualTo(after));
        Assert.That(after, Is.EqualTo(0xa020));
    }
    [Test]
    public void LD_HL_d16()
    {
        var dec = SetupA020Buffered();

        var before = dec.CPU.Registers.HL;

        dec.CPU.Op(Opcode.LD_HL_d16)();

        var after = dec.CPU.Registers.HL;

        Assert.That(before, Is.Not.EqualTo(after));
        Assert.That(after, Is.EqualTo(0xa020));
    }
    [Test]
    public void LD_SP_d16()
    {
        var dec = SetupA020Buffered();

        var before = dec.CPU.Registers.SP;

        dec.CPU.Op(Opcode.LD_SP_d16)();

        var after = dec.CPU.Registers.SP;

        Assert.That(before, Is.Not.EqualTo(after));
        Assert.That(after, Is.EqualTo(0xa020));
    }
    [Test]
    public void LD_SP_d16_v2()
    {
        var p = TestHelpers.NewCore([(byte)Opcode.LD_SP_d16, 0x12, 0x45]
        );

        p.Step();
        Assert.That(0x103, Is.EqualTo(p.CPU.PC));
        Assert.That(0x4512, Is.EqualTo(p.CPU.Registers.SP));
    }

    [Test]
    public void LD_AT_HL_d8()
    {
        var dec = Setup0x77BufferedDecoder();

        dec.CPU.Registers.HL = 0xa000;
        var before = dec.CPU.Registers.HL;
        var memoryBefore = dec.Memory.Read(before);

        dec.CPU.Op(Opcode.LD_AT_HL_d8)();

        var after = dec.CPU.Registers.HL;
        var memoryAfter = dec.Memory.Read(after);

        Assert.That(before, Is.EqualTo(after));
        Assert.That(memoryBefore, Is.Not.EqualTo(memoryAfter));

        Assert.That(0x77, Is.EqualTo(memoryAfter));
    }

    private static Core Setup0x77BufferedDecoder()
    {
        var c = TestHelpers.NewCore([0x77]);
        return c;
    }

    [Test]
    public void LDD_AT_HL_A()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;
        dec.Registers.Set(WideRegister.HL, 0xa001);
        dec.Registers.Set(Register.A, 0x77);

        var before = dec.Registers.HL;
        var memoryBefore = core.Memory.Read(before);

        dec.Op(Opcode.LDD_AT_HL_A)();

        var after = dec.Registers.HL;
        var memoryAfter = core.Memory.Read(after);

        var memoryAfterButAtOldHL = core.Memory.Read(before);

        Assert.That(before - 1, Is.EqualTo(after));
        Assert.That(memoryBefore, Is.EqualTo(memoryAfter));

        Assert.That(0x77, Is.Not.EqualTo(memoryAfter));
        Assert.That(0x77, Is.EqualTo(memoryAfterButAtOldHL));
    }

    [Test]
    public void LDI_AT_HL_A()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;
        dec.Registers.Set(WideRegister.HL, 0xa000);
        dec.Registers.Set(Register.A, 0x77);

        var before = dec.Registers.HL;
        var memoryBefore = core.Memory.Read(before);

        dec.Op(Opcode.LDI_AT_HL_A)();

        var after = dec.Registers.HL;
        var memoryAfter = core.Memory.Read(after);

        var memoryAfterButAtOldHL = core.Memory.Read(before);

        Assert.That(before + 1, Is.EqualTo(after));
        Assert.That(memoryBefore, Is.EqualTo(memoryAfter));

        Assert.That(0x77, Is.Not.EqualTo(memoryAfter));
        Assert.That(0x77, Is.EqualTo(memoryAfterButAtOldHL));
    }

    //[Test]
    //public void LD_AT_a16_SP()
    //{
    //    var dec = SetupA020Buffered();
    //    dec.CPU.Registers.SP = (0xfffe);
    //    dec.CPU.Op(Opcode.LD_AT_a16_SP)();

    //    var result = dec.CPU.ReadWide(0xa020);
    //    Assert.AreEqual(0xfffe, result);
    //}

    [Test]
    public void ADD_HL_BC()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Registers.HL = 0x8a23;
        dec.Registers.BC = 0x0605;

        dec.Op(Opcode.ADD_HL_BC)();

        Assert.That(0x9028, Is.EqualTo(dec.Registers.HL));
        Assert.That(dec.Registers.Half, Is.True);
        Assert.That(dec.Registers.Negative, Is.False);
        Assert.That(dec.Registers.Carry, Is.False);
    }
    [Test]
    public void ADD_HL_HL()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Registers.HL = 0x8a23;

        dec.Op(Opcode.ADD_HL_HL)();

        Assert.That(0x1446, Is.EqualTo(dec.Registers.HL));
        Assert.That(dec.Registers.Half, Is.True);
        Assert.That(dec.Registers.Negative, Is.False);
        Assert.That(dec.Registers.Carry, Is.True);
    }

    [Test]
    public void LD_A_B()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Registers.B = 0x10;

        dec.Op(Opcode.LD_A_B)();

        Assert.That(0x10, Is.EqualTo(dec.Registers.A));

    }
    [Test]
    public void LD_AT_C_A()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Registers.A = 0x10;
        dec.Registers.C = 0xfe;

        dec.Op(Opcode.LD_AT_C_A)();

        Assert.That(0x10, Is.EqualTo(core.Memory.Read(0xfffe)));

    }
    [Test]
    public void LD_A_AT_C()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        core.Memory.Write(0xfffe, 0x10);
        dec.Registers.C = 0xfe;


        dec.Op(Opcode.LD_A_AT_C)();

        Assert.That(0x10, Is.EqualTo(dec.Registers.A));

    }
}
