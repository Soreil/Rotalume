using emulator.opcodes;
using emulator.registers;

using NUnit.Framework;

namespace Tests;

public class Shifts
{
    [Test]
    public void RLCA()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x80);

            dec.Op(Opcode.RLCA)();

            Assert.That(1, Is.EqualTo(dec.Registers.Get(Register.A)));

            Assert.That(dec.Registers.Carry, Is.True);
        }
        {
            dec.Registers.Set(Register.A, 0);

            dec.Op(Opcode.RLCA)();

            Assert.That(0, Is.EqualTo(dec.Registers.Get(Register.A)));

            Assert.That(dec.Registers.Carry, Is.False);
        }
        {
            dec.Registers.Set(Register.A, 0xff);

            dec.Op(Opcode.RLCA)();

            Assert.That(0xff, Is.EqualTo(dec.Registers.Get(Register.A)));

            Assert.That(dec.Registers.Carry, Is.True);
        }
    }
    [Test]
    public void RRCA()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        {
            dec.Registers.Set(Register.A, 0x01);

            dec.Op(Opcode.RRCA)();

            Assert.That(0x80, Is.EqualTo(dec.Registers.Get(Register.A)));

            Assert.That(dec.Registers.Carry, Is.True);
        }
        {
            dec.Registers.Set(Register.A, 0xFF);

            dec.Op(Opcode.RRCA)();

            Assert.That(0xFF, Is.EqualTo(dec.Registers.Get(Register.A)));

            Assert.That(dec.Registers.Carry, Is.True);
        }
        {
            dec.Registers.Set(Register.A, 0);

            dec.Op(Opcode.RRCA)();

            Assert.That(0, Is.EqualTo(dec.Registers.Get(Register.A)));

            Assert.That(dec.Registers.Carry,Is.False);
        }
    }
    [Test]
    public void CPL()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Registers.A = (0xff);
        dec.Op(Opcode.CPL)();

        Assert.That(0, Is.EqualTo(dec.Registers.A));
    }
    [Test]
    public void SCF()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Op(Opcode.SCF)();

        Assert.That(dec.Registers.Carry,Is.True);
    }
    [Test]
    public void CCF()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;
        dec.Registers.Carry = false;

        dec.Op(Opcode.CCF)();
        Assert.That(dec.Registers.Carry,Is.True);

        dec.Op(Opcode.CCF)();
        Assert.That(dec.Registers.Carry,Is.False);
    }
}
