using System;

namespace emulator
{
    public partial class CPU
    {
        public void EnableInterruptsDelayed() => ISR.InterruptEnableScheduled = true;
        public void EnableInterrupts() => ISR.IME = true;
        public void DisableInterrupts() => ISR.IME = false;
        public void Halt() => Halted = ISR.IME
                   ? HaltState.normal
                   : (ISR.InterruptFireRegister & ISR.InterruptControlRegister & 0x1f) == 0 ? HaltState.normalIME0 : HaltState.haltbug;

        public int TicksWeAreWaitingFor = 0;
        public void AddTicks(int n) => TicksWeAreWaitingFor += n;

        //Wrapper to allow easier handling of (HL) usage
        public byte GetRegister(Register r) => r == Register.HL ? Memory.Read(Registers.HL) : Registers.Get(r);

        public void SetRegister(Register r, byte b)
        {
            if (r == Register.HL) Memory.Write(Registers.HL, b);
            else Registers.Set(r, b);
        }

        private ushort Pop()
        {
            var SP = Registers.SP;
            var popped = Memory.ReadWide(SP);
            SP += 2;
            Registers.SP = SP;

            return popped;
        }
        private void Push(ushort s)
        {
            Registers.SP -= 2;
            Memory.Write(Registers.SP, s);
        }
        public Action NOP(int duration) => () => AddTicks(duration);
        public Action LD_D16(WideRegister p0, int duration) => () =>
        {
            var arg = Memory.FetchD16();
            Registers.Set(p0, arg);
            AddTicks(duration);
        };
        public Action LD((WideRegister, Postfix) p0, Register p1, int duration) => p0.Item2 switch
        {
            Postfix.increment => () =>
            {
                var address = Registers.Get(p0.Item1);
                var value = GetRegister(p1);
                Memory.Write(address, value);
                Registers.Set(p0.Item1, (ushort)(address + 1));
                AddTicks(duration);
            }
            ,
            Postfix.decrement => () =>
            {
                var address = Registers.Get(p0.Item1);
                var value = GetRegister(p1);
                Memory.Write(address, value);
                Registers.Set(p0.Item1, (ushort)(address - 1));
                AddTicks(duration);
            }
            ,
            _ => () =>
            {
                var address = Registers.Get(p0.Item1);
                var value = GetRegister(p1);
                Memory.Write(address, value);
                AddTicks(duration);
            }
            ,
        };

        public Action INC(WideRegister p0, int duration) => () =>
        {
            var hl = Registers.Get(p0);
            var target = (ushort)(hl + 1);
            Registers.Set(p0, target);
            AddTicks(duration);
        };

        public Action INC(Register p0, int duration) => () =>
        {
            var before = GetRegister(p0);
            var arg = (byte)(before + 1);

            Registers.Set(Flag.Z, arg == 0);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.H, before.IsHalfCarryAdd(1));
            SetRegister(p0, (byte)(before + 1));
            AddTicks(duration);
        };

        public Action DEC(Register p0, int duration) => () =>
        {
            var before = GetRegister(p0);
            var arg = before == 0 ? (byte)0xff : (byte)(before - 1);
            SetRegister(p0, arg);

            Registers.Set(Flag.Z, arg == 0);
            Registers.Mark(Flag.N);
            Registers.Set(Flag.H, before.IsHalfCarrySub(1));
            AddTicks(duration);
        };

        public Action LD_D8(Register p0, int duration) => () =>
        {
            var arg = Memory.FetchD8();
            SetRegister(p0, arg);
            AddTicks(duration);
        };

        public Action LD_A16(int duration) => () =>
        {
            var addr = Memory.FetchA16();
            var arg = Memory.Read(addr);
            Registers.A = arg;
            AddTicks(duration);
        };

        public Action RLCA(int duration) => () =>
        {
            var A = Registers.A;
            Registers.Mark(Flag.NZ);
            var res = RLC(A);
            Registers.A = (res);
            AddTicks(duration);
        };

        private byte RLC(byte reg)
        {
            var TopBit = reg.GetBit(7);

            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Set(Flag.C, TopBit);

            reg <<= 1;
            reg += (byte)(TopBit ? 1 : 0);
            return reg;
        }

        public Action WriteSPToMem(int duration) => () =>
        {
            var addr = Memory.FetchD16();
            var arg = Registers.SP;

            Memory.Write(addr, arg);
            AddTicks(duration);
        };

        public Action ADD(WideRegister rhs, int duration) => () =>
        {
            var target = Registers.HL;
            var arg = Registers.Get(rhs);

            Registers.Set(Flag.N, false);
            Registers.Set(Flag.H, (((target & 0x0fff) + (arg & 0x0fff)) & 0x1000) == 0x1000);
            Registers.Set(Flag.C, target + arg > 0xFFFF);

            Registers.HL += arg;

            AddTicks(duration);
        };

        public Action LD(Register p0, (WideRegister, Postfix) p1, int duration) => p1.Item2 switch
        {
            Postfix.decrement => () =>
            {
                var addr = Registers.Get(p1.Item1);
                var value = Memory.Read(addr);
                Registers.Set(p0, value);
                Registers.Set(p1.Item1, (ushort)(addr - 1));
                AddTicks(duration);
            }
            ,
            Postfix.increment => () =>
            {
                var addr = Registers.Get(p1.Item1);
                var value = Memory.Read(addr);
                Registers.Set(p0, value);
                Registers.Set(p1.Item1, (ushort)(addr + 1));
                AddTicks(duration);
            }
            ,
            _ => () =>
            {
                var addr = Registers.Get(p1.Item1);
                var value = Memory.Read(addr);
                Registers.Set(p0, value);
                AddTicks(duration);
            }
            ,
        };

        public Action DEC(WideRegister p0, int duration) => () =>
        {
            var v = Registers.Get(p0);
            Registers.Set(p0, --v);
            AddTicks(duration);
        };

        public Action RRCA(int duration) => () =>
        {
            Registers.Mark(Flag.NZ);
            Registers.A = RRC(Registers.A);
            AddTicks(duration);
        };

        private byte RRC(byte reg)
        {
            var BottomBit = reg.GetBit(0);

            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Set(Flag.C, BottomBit);

            reg >>= 1;
            if (BottomBit)
            {
                reg += 0x80;
            }

            return reg;
        }

        public Action STOP(int duration) => () =>
        {
            AddTicks(duration);
            //throw new Exception("Yea we ain't stopping clean partner");
        };
        public Action RLA(int duration) => () =>
        {
            Registers.Mark(Flag.NZ);
            Registers.A = RL(Registers.A);
            AddTicks(duration);
        };

        private byte RL(byte A)
        {
            var TopBit = A.GetBit(7);
            var OldBit = Registers.Get(Flag.C);

            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Set(Flag.C, TopBit);

            A <<= 1;
            A += (byte)(OldBit ? 1 : 0);
            return A;
        }

        public Action JR(int duration) => () =>
        {
            var offset = Memory.FetchR8();
            Pc.Value = (ushort)(Pc.Value + offset);
            AddTicks(duration);
        };
        public Action RRA(int duration) => () =>
        {
            Registers.Mark(Flag.NZ);
            Registers.A = RR(Registers.A);
            AddTicks(duration);
        };

        private byte RR(byte A)
        {
            var TopBit = A.GetBit(0);
            var OldBit = Registers.Get(Flag.C);

            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Set(Flag.C, TopBit);

            A >>= 1;
            if (OldBit)
            {
                A += 0x80;
            }

            return A;
        }

        public Action JR(Flag p0, int duration, int alternativeDuration) => () =>
        {
            var offset = Memory.FetchR8();
            if (Registers.Get(p0))
            {
                Pc.Value = (ushort)(Pc.Value + offset);
                AddTicks(duration);
            }
            else
            {
                AddTicks(alternativeDuration);
            }
        };

        public Action DAA(int duration) => () =>
        {
            var wasSub = Registers.Get(Flag.N);

            if (!wasSub)
            {
                if (Registers.A >= 0x9a) Registers.Set(Flag.C, true);
                if ((Registers.A & 0x0f) >= 0x0a) Registers.Set(Flag.H, true);
            }

            byte adjustment = (byte)(
            (Registers.Get(Flag.H) ? 0x06 : 0x00) |
            (Registers.Get(Flag.C) ? 0x60 : 0x00)
            );

            if (wasSub)
            {
                Registers.A -= adjustment;
            }
            else
            {
                if (adjustment + Registers.A > 0xff)
                {
                    Registers.Set(Flag.C, true);
                }

                Registers.A += adjustment;
            }

            Registers.Set(Flag.Z, Registers.A == 0);

            Registers.Set(Flag.H, false);
            AddTicks(duration);
        };
        public Action CPL(int duration) => () =>
                                                   {
                                                       Registers.A = (byte)~Registers.A;
                                                       Registers.Mark(Flag.N);
                                                       Registers.Mark(Flag.H);
                                                       AddTicks(duration);
                                                   };

        public Action SCF(int duration) => () =>
                                                     {
                                                         Registers.Mark(Flag.NN);
                                                         Registers.Mark(Flag.NH);
                                                         Registers.Mark(Flag.C);
                                                         AddTicks(duration);
                                                     };

        public Action CCF(int duration) => () =>
                                                     {
                                                         Registers.Mark(Flag.NN);
                                                         Registers.Mark(Flag.NH);
                                                         Registers.Set(Flag.C, !Registers.Get(Flag.C));
                                                         AddTicks(duration);
                                                     };
        public Action LD(Register p0, Register p1, int duration) => () =>
             {
                 var arg = GetRegister(p1);
                 SetRegister(p0, arg);
                 AddTicks(duration);
             };

        public Action LD_AT_C_A(int duration) => () =>
        {
            Memory.Write((ushort)(0xFF00 + Registers.C), Registers.A);
            AddTicks(duration);
        };

        public Action LD_A_AT_C(int duration) => () =>
        {
            Registers.A = Memory.Read((ushort)(0xFF00 + Registers.C));
            AddTicks(duration);
        };

        public Action HALT(int duration) => () =>
                                                      {
                                                          AddTicks(duration);
                                                          Halt();
                                                      };
        public Action ADD(Register p0, Register p1, int duration) => () =>
        {
            var lhs = GetRegister(p0);
            var rhs = GetRegister(p1);

            Registers.Set(p0, ADD(lhs, rhs));
            AddTicks(duration);
        };

        private byte ADD(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.NN);
            var sum = lhs + rhs;

            Registers.Set(Flag.Z, ((byte)sum) == 0);
            Registers.Set(Flag.C, sum > 0xff);
            Registers.Set(Flag.H, lhs.IsHalfCarryAdd(rhs));
            return (byte)sum;
        }

        public Action ADC(Register p1, int duration) => () =>
                                                                  {
                                                                      var rhs = GetRegister(p1);

                                                                      ADC(rhs);
                                                                      AddTicks(duration);
                                                                  };
        public Action SUB(Register p0, int duration) => () =>
                                                                  {
                                                                      var lhs = Registers.A;
                                                                      var rhs = GetRegister(p0);

                                                                      Registers.A = SUB(lhs, rhs);
                                                                      AddTicks(duration);
                                                                  };

        private byte SUB(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.N);
            byte sum = (byte)(lhs - rhs);

            Registers.Set(Flag.Z, lhs == rhs);
            Registers.Set(Flag.C, rhs > lhs);
            Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));
            return sum;
        }

        public Action SBC(Register p1, int duration) => () =>
                                                                  {
                                                                      var rhs = GetRegister(p1);

                                                                      SBC(rhs);
                                                                      AddTicks(duration);
                                                                  };

        private void ADC(byte rhs)
        {
            var lhs = Registers.A;
            var carry = Registers.Get(Flag.C) ? 1 : 0;
            Registers.A = (byte)(lhs + rhs + carry);

            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, ((byte)(lhs + rhs + carry)) == 0);
            Registers.Set(Flag.H, ((lhs & 0xf) + (rhs & 0xf) + carry) > 0x0F);
            Registers.Set(Flag.C, (lhs + rhs + carry) > 0xff);
        }

        private void SBC(byte rhs)
        {
            var lhs = Registers.A;
            var carry = Registers.Get(Flag.C) ? 1 : 0;
            Registers.A = (byte)(lhs - rhs - carry);

            Registers.Mark(Flag.N);
            Registers.Set(Flag.Z, ((byte)(lhs - rhs - carry)) == 0);
            Registers.Set(Flag.H, (lhs & 0xf) < ((rhs & 0xf) + carry));
            Registers.Set(Flag.C, (lhs - rhs - carry) > 0xff || (lhs - rhs - carry) < 0);
        }

        public Action AND(Register p0, int duration) => () =>
                                                                  {
                                                                      var lhs = Registers.A;
                                                                      var rhs = GetRegister(p0);

                                                                      AND(lhs, rhs);
                                                                      AddTicks(duration);
                                                                  };
        private void AND(byte lhs, byte rhs)
        {
            var result = lhs & rhs;
            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.H);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A = (byte)result;
        }

        public Action XOR(Register p0, int duration) => () =>
                                                                  {
                                                                      var lhs = Registers.A;
                                                                      var rhs = GetRegister(p0);

                                                                      XOR(lhs, rhs);
                                                                      AddTicks(duration);
                                                                  };
        private void XOR(byte lhs, byte rhs)
        {
            var result = lhs ^ rhs;
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NC);
            Registers.Set(Flag.Z, result == 0);

            Registers.A = (byte)result;
        }

        public Action OR(Register p0, int duration) => () =>
                                                                 {
                                                                     var lhs = Registers.A;
                                                                     var rhs = GetRegister(p0);
                                                                     OR(lhs, rhs);
                                                                     AddTicks(duration);
                                                                 };

        private void OR(byte lhs, byte rhs)
        {
            var result = lhs | rhs;

            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A = (byte)result;
        }
        public Action CP(Register p0, int duration) => () =>
                                                                     {
                                                                         var lhs = Registers.A;
                                                                         var rhs = GetRegister(p0);
                                                                         CP(lhs, rhs);
                                                                         AddTicks(duration);
                                                                     };
        public Action RET(Flag p0, int duration, int alternativeDuration) => () =>
                                                                                       {
                                                                                           if (Registers.Get(p0))
                                                                                           {
                                                                                               Pc.Value = Pop();
                                                                                               AddTicks(duration);
                                                                                           }
                                                                                           else
                                                                                           {
                                                                                               AddTicks(alternativeDuration);
                                                                                           }
                                                                                       };
        public Action POP(WideRegister p0, int duration) => () =>
                                                                      {
                                                                          var SP = Registers.SP;
                                                                          Registers.Set(p0, Memory.ReadWide(SP));
                                                                          Registers.SP = (ushort)(SP + 2);

                                                                          AddTicks(duration);
                                                                      };
        public Action JP_A16(Flag p0, int duration, int alternativeDuration) => () =>
                                                                                          {
                                                                                              var addr = Memory.FetchA16();
                                                                                              if (Registers.Get(p0))
                                                                                              {
                                                                                                  Pc.Value = addr;
                                                                                                  AddTicks(duration);
                                                                                              }
                                                                                              else
                                                                                              {
                                                                                                  AddTicks(alternativeDuration);
                                                                                              }
                                                                                          };
        public Action JP_A16(int duration) => () =>
                                                                   {
                                                                       var addr = Memory.FetchA16();
                                                                       Pc.Value = addr;
                                                                       AddTicks(duration);
                                                                   };
        public Action CALL_A16(Flag p0, int duration, int alternativeDuration) => () =>
                                                                                            {
                                                                                                var addr = Memory.FetchA16();
                                                                                                if (Registers.Get(p0))
                                                                                                {
                                                                                                    Call(duration, addr);
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    AddTicks(alternativeDuration);
                                                                                                }
                                                                                            };
        public Action PUSH(WideRegister p0, int duration) => () =>
                                                                       {
                                                                           Push(Registers.Get(p0));
                                                                           AddTicks(duration);
                                                                       };
        public Action ADD_A_d8(int duration) => () =>
                                                          {
                                                              Registers.Mark(Flag.NN);

                                                              var lhs = Registers.A;
                                                              var rhs = Memory.FetchD8();

                                                              Registers.A = ADD(lhs, rhs);
                                                              AddTicks(duration);
                                                          };
        public Action RST(byte adress, int duration) => () =>
                                                              {
                                                                  Call(duration, adress);
                                                              };
        public Action RET(int duration) => () =>
                                                     {
                                                         var addr = Pop();
                                                         Pc.Value = addr;
                                                         AddTicks(duration);
                                                     };
        //Not an actual instruction
        public Action PREFIX(int duration) => () =>
                                                        {
                                                            AddTicks(duration);
                                                            throw new Exception("unimplementable");
                                                        };

        //TODO: Check where this naming error originated
        public Action CALL_a16(int duration) => () =>
                                                          {
                                                              var addr = Memory.FetchD16();
                                                              Call(duration, addr);
                                                          };

        public void Call(int duration, ushort addr)
        {
            Push(Pc.Value);
            Pc.Value = addr;
            AddTicks(duration);
        }

        public Action ADC(int duration) => () =>
                                                     {
                                                         var rhs = Memory.FetchD8();

                                                         ADC(rhs);
                                                         AddTicks(duration);
                                                     };

        public Action ILLEGAL_D3(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };

        public Action SUB(int duration) => () =>
                                                     {
                                                         Registers.Mark(Flag.N);

                                                         var lhs = Registers.A;
                                                         var rhs = Memory.FetchD8();

                                                         Registers.A = SUB(lhs, rhs);
                                                         AddTicks(duration);
                                                     };
        public Action RETI(int duration) => () =>
                                                      {
                                                          Pc.Value = Pop();
                                                          EnableInterrupts();
                                                          AddTicks(duration);
                                                      };
        public Action ILLEGAL_DB(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action ILLEGAL_DD(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action SBC(int duration) => () =>
                                                     {
                                                         var rhs = Memory.FetchD8();

                                                         SBC(rhs);
                                                         AddTicks(duration);
                                                     };
        public Action LDH(int duration) => () =>
                                                     {
                                                         Memory.Write((ushort)(0xff00 + Memory.FetchD8()), Registers.A);
                                                         AddTicks(duration);
                                                     };
        public Action ILLEGAL_E3(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action ILLEGAL_E4(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action AND(int duration) => () =>
                                                     {
                                                         var andWith = Memory.FetchD8();

                                                         AND(Registers.A, andWith);
                                                         AddTicks(duration);
                                                     };

        public Action ADD_SP_R8(int duration) => () =>
                                                           {
                                                               var offset = Memory.FetchR8();
                                                               var sum = Registers.SP + offset;

                                                               Registers.Mark(Flag.NZ);
                                                               Registers.Mark(Flag.NN);

                                                               if (offset >= 0)
                                                               {
                                                                   Registers.Set(Flag.C, ((Registers.SP & 0xff) + offset) > 0xff);
                                                                   Registers.Set(Flag.H, ((Registers.SP & 0x0f) + (offset & 0xf)) > 0xf);
                                                               }
                                                               else
                                                               {
                                                                   Registers.Set(Flag.C, (sum & 0xff) <= (Registers.SP & 0xff));
                                                                   Registers.Set(Flag.H, (sum & 0xf) <= (Registers.SP & 0xf));
                                                               }

                                                               Registers.SP = (ushort)sum;
                                                               AddTicks(duration);
                                                           };

        public Action JP(int duration) => () =>
                                                    {
                                                        Pc.Value = Registers.HL;
                                                        AddTicks(duration);
                                                    };
        public Action LD_AT_a16_A(int duration) => () =>
                                                             {
                                                                 Memory.Write(Memory.FetchD16(), Registers.A);
                                                                 AddTicks(duration);
                                                             };
        public Action ILLEGAL_EB(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action ILLEGAL_EC(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action ILLEGAL_ED(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action XOR(int duration) => () =>
                                                     {
                                                         XOR(Registers.A, Memory.FetchD8());
                                                         AddTicks(duration);
                                                     };
        public Action LDH_A_AT_a8(int duration) => () =>
                                                             {
                                                                 Registers.A = Memory[0xFF00 + Memory.FetchD8()];
                                                                 AddTicks(duration);
                                                             };
        public Action DI(int duration) => () =>
                                                    {
                                                        DisableInterrupts();
                                                        AddTicks(duration);
                                                    };
        public Action ILLEGAL_F4(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action OR(int duration) => () =>
                                                    {
                                                        OR(Registers.A, Memory.FetchD8());
                                                        AddTicks(duration);
                                                    };

        public Action LD_HL_SP_i8(int duration) => () =>
                                                             {
                                                                 var offset = Memory.FetchR8();
                                                                 var sum = Registers.SP + offset;
                                                                 Registers.Set(Flag.Z, false);
                                                                 Registers.Set(Flag.N, false);

                                                                 if (offset >= 0)
                                                                 {
                                                                     Registers.Set(Flag.C, ((Registers.SP & 0xff) + offset) > 0xff);
                                                                     Registers.Set(Flag.H, ((Registers.SP & 0x0f) + (offset & 0xf)) > 0xf);
                                                                 }
                                                                 else
                                                                 {
                                                                     Registers.Set(Flag.C, (sum & 0xff) <= (Registers.SP & 0xff));
                                                                     Registers.Set(Flag.H, (sum & 0xf) <= (Registers.SP & 0xf));
                                                                 }

                                                                 Registers.HL = (ushort)sum;

                                                                 AddTicks(duration);
                                                             };

        public Action LD_SP_HL(int duration) => () =>
                                                          {
                                                              Registers.SP = Registers.HL;
                                                              AddTicks(duration);
                                                          };
        public Action EI(int duration) => () =>
                                                    {
                                                        EnableInterruptsDelayed();
                                                        AddTicks(duration);
                                                    };
        public Action ILLEGAL_FC(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action ILLEGAL_FD(int duration) => () =>
                                                            {
                                                                AddTicks(duration);
                                                                throw new Exception("illegal");
                                                            };
        public Action CP(int duration) => () =>
                                                    {
                                                        var lhs = Registers.A;
                                                        var rhs = Memory.FetchD8();
                                                        CP(lhs, rhs);
                                                        AddTicks(duration);
                                                    };
        private void CP(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.N);

            Registers.Set(Flag.Z, lhs == rhs);
            Registers.Set(Flag.C, rhs > lhs);
            Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));
        }

        public Action RLC(Register p0, int duration) => () =>
                                                                  {
                                                                      var reg = GetRegister(p0);

                                                                      var res = RLC(reg);
                                                                      Registers.Set(Flag.Z, res == 0);

                                                                      SetRegister(p0, res);
                                                                      AddTicks(duration);
                                                                  };
        public Action RRC(Register p0, int duration) => () =>
                                                                  {
                                                                      var reg = GetRegister(p0);

                                                                      var res = RRC(reg);
                                                                      Registers.Set(Flag.Z, res == 0);

                                                                      SetRegister(p0, res);
                                                                      AddTicks(duration);
                                                                  };
        public Action RL(Register p0, int duration) => () =>
                                                                 {
                                                                     var reg = GetRegister(p0);

                                                                     var res = RL(reg);
                                                                     Registers.Set(Flag.Z, res == 0);

                                                                     SetRegister(p0, res);
                                                                     AddTicks(duration);
                                                                 };
        public Action RR(Register p0, int duration) => () =>
                                                                 {
                                                                     var reg = GetRegister(p0);

                                                                     var res = RR(reg);
                                                                     Registers.Set(Flag.Z, res == 0);

                                                                     SetRegister(p0, res);
                                                                     AddTicks(duration);
                                                                 };
        public Action SLA(Register p0, int duration) => () =>
                                                                  {
                                                                      var reg = GetRegister(p0);

                                                                      var TopBit = reg.GetBit(7);

                                                                      Registers.Mark(Flag.NN);
                                                                      Registers.Mark(Flag.NH);
                                                                      Registers.Set(Flag.C, TopBit);

                                                                      reg <<= 1;
                                                                      Registers.Set(Flag.Z, reg == 0);

                                                                      SetRegister(p0, reg);
                                                                      AddTicks(duration);
                                                                  };
        public Action SRA(Register p0, int duration) => () =>
                                                                  {
                                                                      var lhs = GetRegister(p0);
                                                                      byte bit7 = (byte)(lhs & 0x80);

                                                                      Registers.Mark(Flag.NN);
                                                                      Registers.Mark(Flag.NH);
                                                                      Registers.Set(Flag.C, lhs.GetBit(0));

                                                                      lhs = (byte)((lhs >> 1) | bit7);
                                                                      SetRegister(p0, lhs);

                                                                      Registers.Set(Flag.Z, lhs == 0);

                                                                      AddTicks(duration);
                                                                  };
        public Action SWAP(Register p0, int duration) => () =>
                                                                   {
                                                                       var reg = GetRegister(p0);

                                                                       var low = (reg & 0xf) << 4;
                                                                       var high = (reg & 0xf0) >> 4;
                                                                       var swapped = low | high;

                                                                       Registers.Set(Flag.Z, swapped == 0);
                                                                       Registers.Mark(Flag.NN);
                                                                       Registers.Mark(Flag.NH);
                                                                       Registers.Mark(Flag.NC);


                                                                       SetRegister(p0, (byte)swapped);
                                                                       AddTicks(duration);
                                                                   };

        public Action SRL(Register p0, int duration) => () =>
                                                                  {
                                                                      var lhs = GetRegister(p0);

                                                                      Registers.Mark(Flag.NN);
                                                                      Registers.Mark(Flag.NH);
                                                                      Registers.Set(Flag.C, lhs.GetBit(0));
                                                                      Registers.Set(Flag.Z, lhs >> 1 == 0);

                                                                      SetRegister(p0, (byte)(lhs >> 1));
                                                                      AddTicks(duration);
                                                                  };
        public Action BIT(byte p0, Register p1, int duration) => () =>
                                                                           {
                                                                               var reg = GetRegister(p1);
                                                                               Registers.Set(Flag.Z, !reg.GetBit(p0));
                                                                               Registers.Mark(Flag.NN);
                                                                               Registers.Mark(Flag.H);
                                                                               AddTicks(duration);
                                                                           };

        public Action RES(byte p0, Register p1, int duration) => () =>
                                                                           {
                                                                               var reg = GetRegister(p1);
                                                                               reg.ClearBit(p0);
                                                                               SetRegister(p1, reg);
                                                                               AddTicks(duration);
                                                                           };
        public Action SET(byte p0, Register p1, int duration) => () =>
                                                                           {
                                                                               var reg = GetRegister(p1);
                                                                               reg.SetBit(p0);
                                                                               SetRegister(p1, reg);
                                                                               AddTicks(duration);
                                                                           };
    }
}