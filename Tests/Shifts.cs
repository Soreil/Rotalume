﻿using emulator.opcodes;
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

            Assert.AreEqual(1, dec.Registers.Get(Register.A));

            Assert.IsTrue(dec.Registers.Carry);
        }
        {
            dec.Registers.Set(Register.A, 0);

            dec.Op(Opcode.RLCA)();

            Assert.AreEqual(0, dec.Registers.Get(Register.A));

            Assert.IsTrue(!dec.Registers.Carry);
        }
        {
            dec.Registers.Set(Register.A, 0xff);

            dec.Op(Opcode.RLCA)();

            Assert.AreEqual(0xff, dec.Registers.Get(Register.A));

            Assert.IsTrue(dec.Registers.Carry);
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

            Assert.AreEqual(0x80, dec.Registers.Get(Register.A));

            Assert.IsTrue(dec.Registers.Carry);
        }
        {
            dec.Registers.Set(Register.A, 0xFF);

            dec.Op(Opcode.RRCA)();

            Assert.AreEqual(0xFF, dec.Registers.Get(Register.A));

            Assert.IsTrue(dec.Registers.Carry);
        }
        {
            dec.Registers.Set(Register.A, 0);

            dec.Op(Opcode.RRCA)();

            Assert.AreEqual(0, dec.Registers.Get(Register.A));

            Assert.IsTrue(!dec.Registers.Carry);
        }
    }
    [Test]
    public void CPL()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Registers.A = (0xff);
        dec.Op(Opcode.CPL)();

        Assert.AreEqual(0, dec.Registers.A);
    }
    [Test]
    public void SCF()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;

        dec.Op(Opcode.SCF)();

        Assert.IsTrue(dec.Registers.Carry);
    }
    [Test]
    public void CCF()
    {
        var core = TestHelpers.NewCore([]);
        var dec = core.CPU;
        dec.Registers.Carry = false;

        dec.Op(Opcode.CCF)();
        Assert.IsTrue(dec.Registers.Carry);

        dec.Op(Opcode.CCF)();
        Assert.IsTrue(!dec.Registers.Carry);
    }
}
