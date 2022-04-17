namespace emulator;

public partial class CPU
{
    public void EnableInterruptsDelayed() => ISR.InterruptEnableScheduled = true;
    public void EnableInterrupts() => ISR.IME = true;
    public void DisableInterrupts() => ISR.IME = false;
    public void Halt() => Halted = ISR.IME
               ? HaltState.normal
               : (ISR.InterruptFireRegister & ISR.InterruptControlRegister & 0x1f) == 0 ? HaltState.normalIME0 : HaltState.haltbug;


    //Wrapper to allow easier handling of (HL) usage
    public byte GetRegister(Register r) => r == Register.HL ? Memory.Read(Registers.HL) : Registers.Get(r);

    public void SetRegister(Register r, byte b)
    {
        if (r == Register.HL)
        {
            Memory.Write(Registers.HL, b);
            CycleElapsed();
        }
        else Registers.Set(r, b);
    }

    private ushort Pop()
    {
        var popped = ReadWide(Registers.SP);
        Registers.SP += 2;

        return popped;
    }
    private void Push(ushort s)
    {
        Registers.SP -= 2;
        Write(Registers.SP, s);
    }


    private byte ReadInput()
    {
        CycleElapsed();
        return Memory[PC++];

    }
    private ushort ReadInputWide()
    {
        Span<byte> buf = stackalloc byte[2];
        buf[0] = ReadInput();
        buf[1] = ReadInput();
        return BitConverter.ToUInt16(buf);
    }
    private ushort FetchD16() => ReadInputWide();

    private byte FetchD8() => ReadInput();

    private ushort FetchA16() => ReadInputWide();

    private sbyte FetchR8() => (sbyte)ReadInput();


    private ushort ReadWide(ushort at)
    {
        Span<byte> buf = stackalloc byte[2];
        buf[0] = Memory[at];
        CycleElapsed();
        buf[1] = Memory[(ushort)(at + 1)];
        CycleElapsed();
        return BitConverter.ToUInt16(buf);
    }
    private void Write(ushort at, ushort arg)
    {
        Memory[at] = (byte)arg;
        CycleElapsed();
        Memory[(ushort)(at + 1)] = (byte)(arg >> 8);
        CycleElapsed();
    }
    private void Write(ushort at, byte arg)
    {
        Memory[at] = arg;
        CycleElapsed();
    }

    public Action NOP() => () => { };
    public Action LD_D16(WideRegister p0) => () =>
    {
        var arg = FetchD16();
        Registers.Set(p0, arg);
    };

    public Action LD((WideRegister, Postfix) p0, Register p1) => p0.Item2 switch
    {
        Postfix.increment => () =>
        {
            var address = Registers.Get(p0.Item1);
            var value = GetRegister(p1);
            Write(address, value);
            Registers.Set(p0.Item1, (ushort)(address + 1));
        }
        ,
        Postfix.decrement => () =>
        {
            var address = Registers.Get(p0.Item1);
            var value = GetRegister(p1);
            Write(address, value);
            Registers.Set(p0.Item1, (ushort)(address - 1));

        }
        ,
        _ => () =>
        {
            var address = Registers.Get(p0.Item1);
            var value = GetRegister(p1);
            Write(address, value);
        }
        ,
    };
    internal void SetStateWithoutBootrom()
    {
        PC = 0x100;
        Registers.AF = 0x0100;
        Registers.BC = 0xff13;
        Registers.DE = 0x00c1;
        Registers.HL = 0x8403;
        Registers.SP = 0xfffe;
    }
    public Action INC(WideRegister p0) => () =>
{
    var hl = Registers.Get(p0);
    var target = (ushort)(hl + 1);
    Registers.Set(p0, target);

};

    public Action INC(Register p0) => () =>
    {
        var before = GetRegister(p0);
        var arg = (byte)(before + 1);

        Registers.Zero = arg == 0;
        Registers.Negative = false;
        Registers.Half = before.IsHalfCarryAdd(1);
        SetRegister(p0, (byte)(before + 1));

    };

    public Action DEC(Register p0) => () =>
    {
        var before = GetRegister(p0);
        var arg = before == 0 ? (byte)0xff : (byte)(before - 1);
        SetRegister(p0, arg);

        Registers.Zero = arg == 0;
        Registers.Negative = true;
        Registers.Half = before.IsHalfCarrySub(1);

    };

    public Action LD_D8(Register p0) => () =>
    {
        var arg = FetchD8();
        SetRegister(p0, arg);

    };

    public Action LD_A16() => () =>
    {
        Registers.A = Read(FetchA16());
    };
    private byte Read(ushort v)
    {
        CycleElapsed();
        return Memory[v];
    }

    public Action RLCA() => () =>
    {
        var A = Registers.A;
        Registers.Zero = false;
        Registers.A = RLC(A);
    };

    private byte RLC(byte reg)
    {
        var TopBit = reg.GetBit(7);

        Registers.Negative = false;
        Registers.Half = false;
        Registers.Carry = TopBit;

        reg <<= 1;
        reg += (byte)(TopBit ? 1 : 0);
        return reg;
    }

    public Action WriteSPToMem() => () =>
    {
        var addr = FetchD16();
        var arg = Registers.SP;

        Write(addr, arg);
    };

    public Action ADD(WideRegister rhs) => () =>
    {
        var target = Registers.HL;
        var arg = Registers.Get(rhs);

        Registers.Negative = false;
        Registers.Half = (((target & 0x0fff) + (arg & 0x0fff)) & 0x1000) == 0x1000;
        Registers.Carry = target + arg > 0xFFFF;

        Registers.HL += arg;


    };

    public Action LD(Register p0, (WideRegister, Postfix) p1) => p1.Item2 switch
    {
        Postfix.decrement => () =>
        {
            var addr = Registers.Get(p1.Item1);
            var value = Read(addr);
            Registers.Set(p0, value);
            Registers.Set(p1.Item1, (ushort)(addr - 1));
        }
        ,
        Postfix.increment => () =>
        {
            var addr = Registers.Get(p1.Item1);
            var value = Read(addr);
            Registers.Set(p0, value);
            Registers.Set(p1.Item1, (ushort)(addr + 1));
        }
        ,
        _ => () =>
        {
            var addr = Registers.Get(p1.Item1);
            var value = Read(addr);
            Registers.Set(p0, value);
        }
        ,
    };

    //This function seems to have taken a lot of CPU with the previous imlpementation so we now explicitly return the direct lambda.
    public Action DEC(WideRegister p0) => p0 switch
    {
        WideRegister.BC => () => { Registers.BC--; }
        ,
        WideRegister.DE => () => { Registers.DE--; }
        ,
        WideRegister.HL => () => { Registers.HL--; }
        ,
        WideRegister.SP => () => { Registers.SP--; }
        ,
        _ => throw new IllegalOpCodeException()
    };

    public Action RRCA() => () =>
    {
        Registers.Zero = false;
        Registers.A = RRC(Registers.A);

    };

    private byte RRC(byte reg)
    {
        var BottomBit = reg.GetBit(0);

        Registers.Negative = false;
        Registers.Half = false;
        Registers.Carry = BottomBit;

        reg >>= 1;
        if (BottomBit)
        {
            reg += 0x80;
        }

        return reg;
    }

    public Action STOP() => () => { };
    public Action RLA() => () =>
    {
        Registers.Zero = false;
        Registers.A = RL(Registers.A);

    };

    private byte RL(byte A)
    {
        var TopBit = A.GetBit(7);
        var OldBit = Registers.Carry;

        Registers.Negative = false;
        Registers.Half = false;
        Registers.Carry = TopBit;

        A <<= 1;
        A += (byte)(OldBit ? 1 : 0);
        return A;
    }

    public Action JR() => () =>
    {
        var offset = FetchR8();
        PC = (ushort)(PC + offset);

    };
    public Action RRA() => () =>
    {
        Registers.Zero = false;
        Registers.A = RR(Registers.A);

    };

    private byte RR(byte A)
    {
        var TopBit = A.GetBit(0);
        var OldBit = Registers.Carry;

        Registers.Negative = false;
        Registers.Half = false;
        Registers.Carry = TopBit;

        A >>= 1;
        if (OldBit)
        {
            A += 0x80;
        }

        return A;
    }

    public Action JR(Flag p0) => () =>
    {
        var flag = p0 switch
        {
            Flag.Z => Registers.Zero,
            Flag.NZ => !Registers.Zero,
            Flag.C => Registers.Carry,
            Flag.NC => !Registers.Carry,
            _ => throw new NotImplementedException()
        };
        var offset = FetchR8();
        if (flag)
        {
            PC = (ushort)(PC + offset);
            CycleElapsed();

        }
    };

    public Action DAA() => () =>
    {
        if (!Registers.Negative)
        {
            if (Registers.Carry || Registers.A > 0x99)
            {
                Registers.A += 0x60;
                Registers.Carry = true;
            }
            if (Registers.Half || (Registers.A & 0x0f) > 0x09) Registers.A += 0x06;
        }
        else
        {
            if (Registers.Carry) Registers.A -= 0x60;
            if (Registers.Half) Registers.A -= 0x06;
        }
        Registers.Zero = Registers.A == 0;
        Registers.Half = false;

    };
    public Action CPL() => () =>
                                               {
                                                   Registers.A = (byte)~Registers.A;
                                                   Registers.Negative = true;
                                                   Registers.Half = true;

                                               };

    public Action SCF() => () =>
                                                 {
                                                     Registers.Negative = false;
                                                     Registers.Half = false;
                                                     Registers.Carry = true;

                                                 };

    public Action CCF() => () =>
                                                 {
                                                     Registers.Negative = false;
                                                     Registers.Half = false;
                                                     Registers.Carry = !Registers.Carry;

                                                 };
    public Action LD(Register p0, Register p1) => () =>
         {
             var arg = GetRegister(p1);
             SetRegister(p0, arg);

         };

    public Action LD_AT_C_A() => () =>
    {
        Write((ushort)(0xFF00 + Registers.C), Registers.A);

    };

    public Action LD_A_AT_C() => () =>
    {
        Registers.A = Read((ushort)(0xFF00 + Registers.C));

    };

    public Action HALT() => () =>
                                                  {

                                                      Halt();
                                                  };
    public Action ADD(Register p1) => () =>
    {
        var rhs = GetRegister(p1);

        ADD(rhs);

    };

    private void ADD(byte rhs)
    {
        Registers.Negative = false;
        Registers.Carry = Registers.A + rhs > 0xff;
        Registers.Half = Registers.A.IsHalfCarryAdd(rhs);

        Registers.A += rhs;
        Registers.Zero = Registers.A == 0;
    }

    public Action ADC(Register p1) => () =>
                                                              {
                                                                  var rhs = GetRegister(p1);

                                                                  ADC(rhs);

                                                              };
    public Action SUB(Register p0) => () =>
                                                              {
                                                                  var lhs = Registers.A;
                                                                  var rhs = GetRegister(p0);

                                                                  Registers.A = SUB(lhs, rhs);

                                                              };

    private byte SUB(byte lhs, byte rhs)
    {
        Registers.Negative = true;
        byte sum = (byte)(lhs - rhs);

        Registers.Zero = lhs == rhs;
        Registers.Carry = rhs > lhs;
        Registers.Half = lhs.IsHalfCarrySub(rhs);
        return sum;
    }

    public Action SBC(Register p1) => () =>
                                                              {
                                                                  var rhs = GetRegister(p1);

                                                                  SBC(rhs);

                                                              };

    private void ADC(byte rhs)
    {
        var lhs = Registers.A;
        var carry = Registers.Carry ? 1 : 0;
        Registers.A = (byte)(lhs + rhs + carry);

        Registers.Negative = false;
        Registers.Zero = ((byte)(lhs + rhs + carry)) == 0;
        Registers.Half = ((lhs & 0xf) + (rhs & 0xf) + carry) > 0x0F;
        Registers.Carry = (lhs + rhs + carry) > 0xff;
    }

    private void SBC(byte rhs)
    {
        var lhs = Registers.A;
        var carry = Registers.Carry ? 1 : 0;
        Registers.A = (byte)(lhs - rhs - carry);

        Registers.Negative = true;
        Registers.Zero = ((byte)(lhs - rhs - carry)) == 0;
        Registers.Half = (lhs & 0xf) < ((rhs & 0xf) + carry);
        Registers.Carry = lhs - rhs - carry is > 0xff or < 0;
    }

    public Action AND(Register p0) => () =>
    {
        var rhs = GetRegister(p0);

        AND(rhs);

    };
    private void AND(byte rhs)
    {
        Registers.A &= rhs;
        Registers.Carry = false;
        Registers.Half = true;
        Registers.Negative = false;
        Registers.Zero = Registers.A == 0;

    }

    public Action XOR(Register p0) => () =>
                                                              {
                                                                  var rhs = GetRegister(p0);

                                                                  XOR(rhs);

                                                              };
    private void XOR(byte rhs)
    {
        Registers.A ^= rhs;
        Registers.Half = false;
        Registers.Negative = false;
        Registers.Carry = false;
        Registers.Zero = Registers.A == 0;
    }

    public Action OR(Register p0) => () =>
                                                             {
                                                                 var rhs = GetRegister(p0);
                                                                 OR(rhs);

                                                             };

    private void OR(byte rhs)
    {
        Registers.A |= rhs;

        Registers.Carry = false;
        Registers.Half = false;
        Registers.Negative = false;
        Registers.Zero = Registers.A == 0;

    }
    public Action CP(Register p0) => () =>
    {
        var rhs = GetRegister(p0);
        CP(rhs);

    };
    public Action RET(Flag p0) => () =>
    {
        var flag = p0 switch
        {
            Flag.Z => Registers.Zero,
            Flag.NZ => !Registers.Zero,
            Flag.C => Registers.Carry,
            Flag.NC => !Registers.Carry,
            _ => throw new NotImplementedException()
        };
        if (flag)
        {
            PC = Pop();
            CycleElapsed();
            CycleElapsed();
            CycleElapsed();
        }
    };
    public Action POP(WideRegister p0) => () =>
    {
        Registers.Set(p0, ReadWide(Registers.SP));
        Registers.SP += 2;


    };
    public Action JP_A16(Flag p0) => () =>
    {
        var flag = p0 switch
        {
            Flag.Z => Registers.Zero,
            Flag.NZ => !Registers.Zero,
            Flag.C => Registers.Carry,
            Flag.NC => !Registers.Carry,
            _ => throw new NotImplementedException()
        };

        var addr = FetchA16();
        if (flag)
        {
            PC = addr;
            CycleElapsed();

        }
    };
    public Action JP_A16() => () =>
    {
        var addr = FetchA16();
        PC = addr;

    };
    public Action CALL_A16(Flag p0) => () =>
     {
         var flag = p0 switch
         {
             Flag.Z => Registers.Zero,
             Flag.NZ => !Registers.Zero,
             Flag.C => Registers.Carry,
             Flag.NC => !Registers.Carry,
             _ => throw new NotImplementedException()
         };

         var addr = FetchA16();
         if (flag)
         {
             Call(addr);
             CycleElapsed();
             CycleElapsed();
             CycleElapsed();
         }
     };


    public Action PUSH(WideRegister p0) => () =>
    {
        Push(Registers.Get(p0));

    };
    public Action ADD_A_d8() => () =>
    {
        Registers.Negative = false;

        var rhs = FetchD8();

        ADD(rhs);

    };
    public Action RST(byte adress) => () => Call(adress);
    public Action RET() => () =>
    {
        var addr = Pop();
        PC = addr;

    };
    //Not an actual instruction
    public Action PREFIX() => () =>
    {

        throw new IllegalOpCodeException("unimplementable");
    };

    //TODO: Check where this naming error originated
    public Action CALL_a16() => () => Call(FetchD16());

    public void Call(ushort addr)
    {
        CycleElapsed();
        Push(PC);
        PC = addr;

    }

    public Action ADC() => () =>
    {
        var rhs = FetchD8();

        ADC(rhs);

    };

    public Action ILLEGAL_D3() => () =>
    {

        throw new Exception("illegal");
    };

    public Action SUB() => () =>
    {
        Registers.Negative = true;

        var lhs = Registers.A;
        var rhs = FetchD8();

        Registers.A = SUB(lhs, rhs);

    };
    public Action RETI() => () =>
    {
        PC = Pop();
        EnableInterrupts();

    };
    public Action ILLEGAL_DB() => () =>
    {

        throw new IllegalOpCodeException("illegal");
    };
    public Action ILLEGAL_DD() => () =>
    {

        throw new IllegalOpCodeException("illegal");
    };
    public Action SBC() => () =>
                                                 {
                                                     var rhs = FetchD8();

                                                     SBC(rhs);

                                                 };
    public Action LDH() => () =>
                                                 {
                                                     Write((ushort)(0xff00 + FetchD8()), Registers.A);

                                                 };
    public Action ILLEGAL_E3() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action ILLEGAL_E4() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action AND() => () =>
                                                 {
                                                     var andWith = FetchD8();

                                                     AND(andWith);

                                                 };

    public Action ADD_SP_R8() => () =>
    {
        var offset = FetchR8();
        var sum = Registers.SP + offset;

        Registers.Zero = false;
        Registers.Negative = false;

        if (offset >= 0)
        {
            Registers.Carry = ((Registers.SP & 0xff) + offset) > 0xff;
            Registers.Half = ((Registers.SP & 0x0f) + (offset & 0xf)) > 0xf;
        }
        else
        {
            Registers.Carry = (sum & 0xff) <= (Registers.SP & 0xff);
            Registers.Half = (sum & 0xf) <= (Registers.SP & 0xf);
        }

        Registers.SP = (ushort)sum;

    };

    public Action JP() => () =>
                                                {
                                                    PC = Registers.HL;

                                                };
    public Action LD_AT_a16_A() => () =>
                                                         {
                                                             Write(FetchD16(), Registers.A);

                                                         };
    public Action ILLEGAL_EB() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action ILLEGAL_EC() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action ILLEGAL_ED() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action XOR() => () =>
                                                 {
                                                     XOR(FetchD8());

                                                 };
    public Action LDH_A_AT_a8() => () =>
                                                         {
                                                             Registers.A = Read((ushort)(0xFF00 + FetchD8()));

                                                         };
    public Action DI() => () =>
                                                {
                                                    DisableInterrupts();

                                                };
    public Action ILLEGAL_F4() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action OR() => () =>
                                                {
                                                    OR(FetchD8());

                                                };

    public Action LD_HL_SP_i8() => () =>
                                                         {
                                                             var offset = FetchR8();
                                                             var sum = Registers.SP + offset;
                                                             Registers.Zero = false;
                                                             Registers.Negative = false;

                                                             if (offset >= 0)
                                                             {
                                                                 Registers.Carry = ((Registers.SP & 0xff) + offset) > 0xff;
                                                                 Registers.Half = ((Registers.SP & 0x0f) + (offset & 0xf)) > 0xf;
                                                             }
                                                             else
                                                             {
                                                                 Registers.Carry = (sum & 0xff) <= (Registers.SP & 0xff);
                                                                 Registers.Half = (sum & 0xf) <= (Registers.SP & 0xf);
                                                             }

                                                             Registers.HL = (ushort)sum;


                                                         };

    public Action LD_SP_HL() => () =>
                                                      {
                                                          Registers.SP = Registers.HL;

                                                      };
    public Action EI() => () =>
                                                {
                                                    EnableInterruptsDelayed();

                                                };
    public Action ILLEGAL_FC() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action ILLEGAL_FD() => () =>
                                                        {

                                                            throw new IllegalOpCodeException("illegal");
                                                        };
    public Action CP() => () =>
                                                {
                                                    var lhs = Registers.A;
                                                    var rhs = FetchD8();
                                                    CP(rhs);

                                                };
    private void CP(byte rhs)
    {
        Registers.Negative = true;

        Registers.Zero = Registers.A == rhs;
        Registers.Carry = rhs > Registers.A;
        Registers.Half = Registers.A.IsHalfCarrySub(rhs);
    }

    public Action RLC(Register p0) => () =>
                                                              {
                                                                  var reg = GetRegister(p0);

                                                                  var res = RLC(reg);
                                                                  Registers.Zero = res == 0;

                                                                  SetRegister(p0, res);

                                                              };
    public Action RRC(Register p0) => () =>
                                                              {
                                                                  var reg = GetRegister(p0);

                                                                  var res = RRC(reg);
                                                                  Registers.Zero = res == 0;

                                                                  SetRegister(p0, res);

                                                              };
    public Action RL(Register p0) => () =>
                                                             {
                                                                 var reg = GetRegister(p0);

                                                                 var res = RL(reg);
                                                                 Registers.Zero = res == 0;

                                                                 SetRegister(p0, res);

                                                             };
    public Action RR(Register p0) => () =>
                                                             {
                                                                 var reg = GetRegister(p0);

                                                                 var res = RR(reg);
                                                                 Registers.Zero = res == 0;

                                                                 SetRegister(p0, res);

                                                             };
    public Action SLA(Register p0) => () =>
                                                              {
                                                                  var reg = GetRegister(p0);

                                                                  var TopBit = reg.GetBit(7);

                                                                  Registers.Negative = false;
                                                                  Registers.Half = false;
                                                                  Registers.Carry = TopBit;

                                                                  reg <<= 1;
                                                                  Registers.Zero = reg == 0;

                                                                  SetRegister(p0, reg);

                                                              };
    public Action SRA(Register p0) => () =>
                                                              {
                                                                  var lhs = GetRegister(p0);
                                                                  byte bit7 = (byte)(lhs & 0x80);

                                                                  Registers.Negative = false;
                                                                  Registers.Half = false;
                                                                  Registers.Carry = lhs.GetBit(0);

                                                                  lhs = (byte)((lhs >> 1) | bit7);
                                                                  SetRegister(p0, lhs);

                                                                  Registers.Zero = lhs == 0;


                                                              };
    public Action SWAP(Register p0) => () =>
                                                               {
                                                                   var reg = GetRegister(p0);

                                                                   var low = reg << 4;
                                                                   var high = reg >> 4;
                                                                   var swapped = low | high;

                                                                   Registers.Zero = swapped == 0;
                                                                   Registers.Negative = false;
                                                                   Registers.Half = false;
                                                                   Registers.Carry = false;


                                                                   SetRegister(p0, (byte)swapped);

                                                               };

    public Action SRL(Register p0) => () =>
                                                              {
                                                                  var lhs = GetRegister(p0);

                                                                  Registers.Negative = false;
                                                                  Registers.Half = false;
                                                                  Registers.Carry = lhs.GetBit(0);
                                                                  Registers.Zero = lhs >> 1 == 0;

                                                                  SetRegister(p0, (byte)(lhs >> 1));

                                                              };
    public Action BIT(byte p0, Register p1) => () =>
                                                                       {
                                                                           var reg = GetRegister(p1);
                                                                           Registers.Zero = !reg.GetBit(p0);
                                                                           Registers.Negative = false;
                                                                           Registers.Half = true;

                                                                       };

    public Action RES(byte p0, Register p1) => () =>
                                                                       {
                                                                           var reg = GetRegister(p1);
                                                                           reg.ClearBit(p0);
                                                                           SetRegister(p1, reg);

                                                                       };
    public Action SET(byte p0, Register p1) => () =>
                                                                       {
                                                                           var reg = GetRegister(p1);
                                                                           reg.SetBit(p0);
                                                                           SetRegister(p1, reg);

                                                                       };
}
