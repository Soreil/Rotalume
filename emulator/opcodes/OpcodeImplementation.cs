using System;

namespace emulator
{
    public partial class CPU
    {
        readonly public Registers Registers;
        readonly public MMU Memory;
        readonly public Action<ushort> SetPC;
        readonly public Func<ushort> GetPC;
        readonly public Action enableInterrupts;
        readonly public Action disableInterrupts;
        readonly private Action<long> AddTicks;
        readonly public Action halt;
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
        public Action NOP(int duration)
        {
            return () => { AddTicks(duration); };
        }
        public Action LD((WideRegister, Traits) p0, (DMGInteger, Traits) p1, int duration)
        {
            return () =>
            {
                var arg = Memory.Fetch(p1.Item1);
                if (!p0.Item2.Immediate)
                {
                    var HL = Registers.Get(p0.Item1);
                    Memory.Write(HL, (byte)arg);
                }
                else
                {
                    Registers.Set(p0.Item1, (ushort)arg);
                }
                AddTicks(duration);
            };
        }
        public Action LD((WideRegister, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {
                var address = Registers.Get(p0.Item1);
                var value = Registers.Get(p1.Item1);

                Memory.Write(address, value);

                switch (p0.Item2.Postfix)
                {
                    case Postfix.increment:
                        Registers.Set(p0.Item1, (ushort)(address + 1));
                        break;
                    case Postfix.decrement:
                        Registers.Set(p0.Item1, (ushort)(address - 1));
                        break;
                    default:
                        break;
                }
                AddTicks(duration);
            };
        }
        public Action INC((WideRegister, Traits) p0, int duration)
        => () =>
        {
            //Wide registers do not use flags for INC and DEC
            if (p0.Item2.Immediate)
            {
                var hl = Registers.Get(p0.Item1);
                var target = (ushort)(hl + 1);
                Registers.Set(p0.Item1, target);
            }
            else
            {
                var addr = Registers.Get(p0.Item1);
                var before = Memory.Read(addr);
                var arg = (byte)(before + 1);
                Memory.Write(addr, arg);

                Registers.Set(Flag.Z, arg == 0);
                Registers.Mark(Flag.NN);
                Registers.Set(Flag.H, before.IsHalfCarryAdd(1));
            }
            AddTicks(duration);
        };

        public Action INC((Register, Traits) p0, int duration)
        => () =>
        {
            var before = Registers.Get(p0.Item1);
            var arg = (byte)(before + 1);
            Registers.Set(p0.Item1, arg);

            Registers.Set(Flag.Z, arg == 0);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.H, before.IsHalfCarryAdd(1));
            AddTicks(duration);
        };
        public Action DEC((Register, Traits) p0, int duration)
        => () =>
        {
            var before = Registers.Get(p0.Item1);
            var arg = before == 0 ? (byte)0xff : (byte)(before - 1);
            Registers.Set(p0.Item1, arg);

            Registers.Set(Flag.Z, arg == 0);
            Registers.Mark(Flag.N);
            Registers.Set(Flag.H, before.IsHalfCarrySub(1));
            AddTicks(duration);
        };
        public Action LD((Register, Traits) p0, (DMGInteger, Traits) p1, int duration)
            => () =>
            {
                if (p1.Item1 == DMGInteger.a16 && !p1.Item2.Immediate)
                {
                    var addr = (ushort)Memory.Fetch(p1.Item1);
                    var arg = Memory.Read(addr);
                    Registers.Set(p0.Item1, arg);
                    AddTicks(duration);
                }
                else
                {
                    var arg = (byte)Memory.Fetch(p1.Item1);
                    Registers.Set(p0.Item1, arg);
                    AddTicks(duration);
                }
            };
        public Action RLCA(int duration)

            => () =>
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
            reg += TopBit ? 1 : 0;
            return reg;
        }

        //This op is a litle weird, we should have generate
        //lefthandsided shorts as being a different type.
        public Action WriteSPToMem(int duration)
            => () =>
            {
                var addr = (ushort)Memory.Fetch(DMGInteger.d16);
                var arg = Registers.SP;

                Memory.Write(addr, arg);
                AddTicks(duration);
            };

        public Action ADD(WideRegister rhs, int duration)
        {
            return () =>
            {

                var target = Registers.Get(WideRegister.HL);
                var arg = Registers.Get(rhs);

                Registers.Set(Flag.N, false);
                Registers.Set(Flag.H, target.IsHalfCarryAdd(arg));
                Registers.Set(Flag.C, target + arg > 0xFFFF);

                Registers.HL += arg;

                AddTicks(duration);
            };
        }
        public Action LD((Register, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p1.Item1);
                var value = Memory.Read(addr);

                Registers.Set(p0.Item1, value);
                switch (p1.Item2.Postfix)
                {
                    case Postfix.decrement:
                        Registers.Set(p1.Item1, (ushort)(addr - 1));
                        break;
                    case Postfix.increment:
                        Registers.Set(p1.Item1, (ushort)(addr + 1));
                        break;
                    default:
                        break;
                }
                AddTicks(duration);
            };
        }
        public Action DEC((WideRegister, Traits) p0, int duration)
        => () =>
        {
            //Wide registers do not use flags for INC and DEC
            if (p0.Item2.Immediate)
            {
                var v = Registers.Get(p0.Item1);
                ushort result = v == 0 ? 0xffff : (ushort)(v - 1);
                Registers.Set(p0.Item1, result);
            }
            else
            {
                var read = Memory.Read(Registers.HL);
                ushort result = read == 0 ? 0xffff : (ushort)(read - 1);
                Memory.Write(Registers.HL, result);
            }
            AddTicks(duration);
        };
        public Action RRCA(int duration)
            => () =>
            {
                var A = Registers.A;
                Registers.Mark(Flag.NZ);
                A = RRC(A);
                Registers.A = (A);
                AddTicks(duration);
            };

        private byte RRC(byte reg)
        {
            var BottomBit = reg.GetBit(0);

            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Set(Flag.C, BottomBit);

            reg >>= 1;
            if (BottomBit) reg += 0x80;
            return reg;
        }

        public Action STOP(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("Yea we ain't stopping clean partner");
            };
        }
        public Action RLA(int duration)
            => () =>
            {
                var A = Registers.A;
                Registers.Mark(Flag.NZ);
                A = RL(A);
                Registers.A = (A);
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
            A += OldBit ? 1 : 0;
            return A;
        }

        public Action JR(int duration)
        {
            return () =>
            {
                var offset = (sbyte)Memory.Fetch(DMGInteger.r8);
                SetPC((ushort)(GetPC() + offset));
                AddTicks(duration);
            };
        }
        public Action RRA(int duration)
            => () =>
            {
                var A = Registers.A;
                Registers.Mark(Flag.NZ);
                A = RR(A);
                Registers.A = (A);
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
            if (OldBit) A += 0x80;
            return A;
        }

        public Action JR((Flag, Traits) p0, int duration, int alternativeDuration)
        {
            return () =>
            {
                var offset = (sbyte)Memory.Fetch(DMGInteger.r8);
                if (Registers.Get(p0.Item1))
                {
                    SetPC((ushort)(GetPC() + offset));
                    AddTicks(duration);
                }
                else AddTicks(alternativeDuration);
            };
        }
        public Action DAA(int duration)
        {
            return () =>
            {

                var wasSub = Registers.Get(Flag.N);

                if (!wasSub)
                {
                    if (Registers.Get(Flag.C) || Registers.A > 0x99)
                    {
                        Registers.A += 0x60;
                        Registers.Mark(Flag.C);
                    }
                    if (Registers.Get(Flag.H) || (Registers.A & 0x0f) > 0x09)
                    {
                        Registers.A += 0x06;
                    }
                }
                else
                {
                    if (Registers.Get(Flag.C))
                    {
                        Registers.A -= 0x60;
                        Registers.Mark(Flag.C);
                    }
                    if (Registers.Get(Flag.H))
                    {
                        Registers.A -= 0x06;
                    }
                }

                Registers.Set(Flag.Z, Registers.A == 0);
                Registers.Mark(Flag.NH);

                AddTicks(duration);
            };
        }
        public Action CPL(int duration)
        {
            return () =>
          {
              Registers.A = ((byte)~Registers.A);
              Registers.Mark(Flag.N);
              Registers.Mark(Flag.H);
              AddTicks(duration);
          };
        }

        public Action SCF(int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);
                Registers.Mark(Flag.NH);
                Registers.Mark(Flag.C);
                AddTicks(duration);
            };
        }

        public Action CCF(int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);
                Registers.Mark(Flag.NH);
                Registers.Set(Flag.C, !Registers.Get(Flag.C));
                AddTicks(duration);
            };
        }
        public Action LD((Register, Traits) p0, (Register, Traits) p1, int duration)
        {
            if (p0.Item2.Immediate && p1.Item2.Immediate)
                return () =>
                {
                    var arg = Registers.Get(p1.Item1);
                    Registers.Set(p0.Item1, arg);
                    AddTicks(duration);
                };
            else
            {
                if (p0.Item2.Immediate)
                    return () =>
                    {
                        Registers.Set(p0.Item1, Memory.Read((ushort)(0xFF00 + Registers.Get(p1.Item1))));
                        AddTicks(duration);
                    };
                else
                    return () =>
                    {
                        Memory.Write((ushort)(0xFF00 + Registers.Get(p0.Item1)), Registers.Get(p1.Item1));
                        AddTicks(duration);
                    };
            }
        }
        public Action HALT(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                halt();
            };
        }
        public Action ADD((Register, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {

                var lhs = Registers.Get(p0.Item1);
                var rhs = Registers.Get(p1.Item1);

                Registers.Set(p0.Item1, ADD(lhs, rhs));
                AddTicks(duration);
            };
        }
        public Action ADD((Register, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {

                var lhs = Registers.Get(p0.Item1);
                var rhs = Memory.Read(Registers.Get(p1.Item1));

                Registers.Set(p0.Item1, ADD(lhs, rhs));
                AddTicks(duration);
            };
        }

        private byte ADD(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.NN);
            var sum = lhs + rhs;

            Registers.Set(Flag.Z, ((byte)sum) == 0);
            Registers.Set(Flag.C, sum > 0xff);
            Registers.Set(Flag.H, lhs.IsHalfCarryAdd(rhs));
            return (byte)sum;
        }

        public Action ADC((Register, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = Registers.Get(p1.Item1);

                Registers.Set(p0.Item1, ADC(lhs, rhs));
                AddTicks(duration);
            };
        }
        public Action ADC((Register, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {
                var lhs = Registers.Get(p0.Item1);
                var rhs = Memory.Read(Registers.Get(p1.Item1));

                Registers.Set(p0.Item1, ADC(lhs, rhs));
                AddTicks(duration);
            };
        }

        private byte ADC(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.NN);
            var sum = lhs + rhs + (Registers.Get(Flag.C) ? 1 : 0);

            Registers.Set(Flag.Z, ((byte)sum) == 0);
            Registers.Set(Flag.H, lhs.IsHalfCarryAdd((byte)(rhs + (Registers.Get(Flag.C) ? 1 : 0))));
            Registers.Set(Flag.C, sum > 0xff);
            return (byte)sum;
        }

        public Action SUB((Register, Traits) p0, int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.A;
                var rhs = Registers.Get(p0.Item1);

                Registers.A = SUB(lhs, rhs);
                AddTicks(duration);
            };
        }
        public Action SUB((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Memory.Read(Registers.Get(p0.Item1));

                Registers.A = SUB(lhs, rhs);
                AddTicks(duration);
            };
        }

        private byte SUB(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.N);
            byte sum = lhs < rhs ? (byte)(0xff - (rhs - lhs)) : (byte)(lhs - rhs);

            Registers.Set(Flag.Z, sum == 0);
            Registers.Set(Flag.C, lhs < rhs);
            Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));
            return (byte)sum;
        }

        public Action SBC((Register, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {
                var lhs = Registers.Get(p0.Item1);
                var rhs = Registers.Get(p1.Item1);

                Registers.Set(p0.Item1, SBC(lhs, rhs));
                AddTicks(duration);
            };
        }
        public Action SBC((Register, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {
                var lhs = Registers.Get(p0.Item1);
                var rhs = Memory.Read(Registers.Get(p1.Item1));

                Registers.Set(p0.Item1, SBC(lhs, rhs));
                AddTicks(duration);
            };
        }

        private byte SBC(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.N);
            byte sum = lhs < rhs ? (byte)(0xff - (rhs - lhs)) : (byte)(lhs - rhs);
            sum -= (Registers.Get(Flag.C) ? 1 : 0);

            Registers.Set(Flag.Z, ((byte)sum) == 0);
            Registers.Set(Flag.H, lhs.IsHalfCarrySub((byte)(rhs - (Registers.Get(Flag.C) ? 1 : 0))));
            Registers.Set(Flag.C, lhs < rhs);
            return (byte)sum;
        }
        public Action AND((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Registers.Get(p0.Item1);

                AND(lhs, rhs);
                AddTicks(duration);
            };
        }
        public Action AND((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Memory.Read(Registers.Get(p0.Item1));

                AND(lhs, rhs);
                AddTicks(duration);
            };
        }
        private void AND(byte lhs, byte rhs)
        {
            var result = lhs & rhs;
            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.H);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A = ((byte)result);
        }

        public Action XOR((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Registers.Get(p0.Item1);

                XOR(lhs, rhs);
                AddTicks(duration);
            };
        }
        public Action XOR((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Memory.Read(Registers.Get(p0.Item1));

                XOR(lhs, rhs);
                AddTicks(duration);
            };
        }

        private void XOR(byte lhs, byte rhs)
        {
            var result = lhs ^ rhs;
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A = ((byte)result);
        }

        public Action OR((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Registers.Get(p0.Item1);
                OR(lhs, rhs);
                AddTicks(duration);
            };
        }
        public Action OR((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Memory.Read(Registers.Get(p0.Item1));
                OR(lhs, rhs);
                AddTicks(duration);
            };
        }

        private void OR(byte lhs, byte rhs)
        {
            var result = lhs | rhs;

            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A = ((byte)result);
        }
        public Action CP((Register, Traits) p0, int duration)
        {
            return () =>
                {
                    var lhs = Registers.A;
                    var rhs = Registers.Get(p0.Item1);
                    CP(lhs, rhs);
                    AddTicks(duration);
                };
        }
        public Action CP((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = Memory.Read(Registers.Get(p0.Item1));
                CP(lhs, rhs);
                AddTicks(duration);
            };
        }
        public Action RET((Flag, Traits) p0, int duration, int alternativeDuration)
        {
            return () =>
            {
                if (Registers.Get(p0.Item1))
                {
                    SetPC(Pop());
                    AddTicks(duration);
                }
                else AddTicks(alternativeDuration);
            };
        }
        public Action POP((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var SP = Registers.SP;
                Registers.Set(p0.Item1, Memory.ReadWide(SP));
                Registers.SP = (ushort)(SP + 2);

                AddTicks(duration);
            };
        }
        public Action JP((Flag, Traits) p0, (DMGInteger, Traits) p1, int duration, int alternativeDuration)
        {
            return () =>
            {
                var addr = (ushort)Memory.Fetch(p1.Item1);
                if (Registers.Get(p0.Item1))
                {
                    SetPC(addr);
                    AddTicks(duration);
                }
                else AddTicks(alternativeDuration);
            };
        }
        public Action JP((DMGInteger, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = (ushort)Memory.Fetch(p0.Item1);
                SetPC(addr);
                AddTicks(duration);
            };
        }
        public Action CALL((Flag, Traits) p0, (DMGInteger, Traits) p1, int duration, int alternativeDuration)
        {
            return () =>
            {
                var addr = (ushort)Memory.Fetch(p1.Item1);
                if (Registers.Get(p0.Item1))
                {
                    Call(duration, addr);
                }
                else AddTicks(alternativeDuration);
            };
        }
        public Action PUSH((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                Push(Registers.Get(p0.Item1));
                AddTicks(duration);
            };
        }
        public Action ADD((Register, Traits) p0, (DMGInteger, Traits) p1, int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = (byte)Memory.Fetch(p1.Item1);
                var sum = lhs + rhs;

                Registers.Set(Flag.Z, ((byte)sum) == 0);
                Registers.Set(Flag.C, sum > 0xff);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd(rhs));

                Registers.Set(p0.Item1, (byte)sum);
                AddTicks(duration);
            };
        }
        public Action RST((byte, Traits) p0, int duration)
        {
            return () =>
            {
                Call(duration, p0.Item1);
            };
        }
        public Action RET(int duration)
        {
            return () =>
            {
                var addr = Pop();
                SetPC(addr);
                AddTicks(duration);
            };
        }
        //Not an actual instruction
        public Action PREFIX(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("unimplementable");
            };
        }
        public Action CALL((DMGInteger, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = (ushort)Memory.Fetch(DMGInteger.d16);
                Call(duration, addr);
            };
        }

        public void Call(int duration, ushort addr)
        {
            Push(GetPC());
            SetPC(addr);
            AddTicks(duration);
        }

        public Action ADC((Register, Traits) p0, (DMGInteger, Traits) p1, int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = (byte)Memory.Fetch(p1.Item1);
                var sum = lhs + rhs + (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, ((byte)sum) == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd((byte)(rhs + (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, sum > 0xff);

                Registers.Set(p0.Item1, (byte)sum);
                AddTicks(duration);
            };
        }
        public Action ILLEGAL_D3(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action SUB(int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.A;
                var rhs = (byte)Memory.Fetch(DMGInteger.d8);
                byte sum = lhs < rhs ? (byte)(0xff - (rhs - lhs)) : (byte)(lhs - rhs);

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.C, lhs < rhs);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));

                Registers.A = sum;
                AddTicks(duration);
            };
        }
        public Action RETI(int duration)
        {
            return () =>
            {
                SetPC(Pop());
                enableInterrupts();
                AddTicks(duration);
            };
        }
        public Action ILLEGAL_DB(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action ILLEGAL_DD(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action SBC((Register, Traits) p0, int duration)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.Get(p0.Item1);
                var rhs = (byte)Memory.Fetch(DMGInteger.d8);
                var sum = lhs - rhs - (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, ((byte)sum) == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub((byte)(rhs - (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, lhs < rhs);

                Registers.Set(p0.Item1, ((byte)sum));
                AddTicks(duration);
            };
        }
        public Action LDH(int duration)
        {
            return () =>
            {
                Memory.Write(DMGInteger.a8, Registers.Get(Register.A));
                AddTicks(duration);
            };
        }
        public Action ILLEGAL_E3(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action ILLEGAL_E4(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action AND((DMGInteger, Traits) p0, int duration)
        {
            return () =>
            {
                var andWith = (byte)Memory.Fetch(DMGInteger.d8);
                Registers.A &= andWith;
                Registers.Set(Flag.Z, Registers.A == 0);
                Registers.Set(Flag.N, false);
                Registers.Set(Flag.H, true);
                Registers.Set(Flag.C, false);
                AddTicks(duration);
            };
        }

        public Action ADD((WideRegister, Traits) p0, (DMGInteger, Traits) p1, int duration)
        {
            return () =>
            {
                Registers.Set(p0.Item1, (ushort)(Registers.Get(p0.Item1) + (sbyte)Memory.Fetch(p1.Item1)));
                AddTicks(duration);
            };
        }
        public Action JP((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                SetPC(addr);
                AddTicks(duration);
            };
        }
        public Action LD((DMGInteger, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {
                Memory.Write(p0.Item1, Registers.Get(p1.Item1));
                AddTicks(duration);
            };
        }
        public Action ILLEGAL_EB(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action ILLEGAL_EC(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action ILLEGAL_ED(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action XOR((DMGInteger, Traits) p0, int duration)
        {
            return () =>
            {
                Registers.A = ((byte)(Registers.A ^ (byte)Memory.Fetch(p0.Item1)));
                AddTicks(duration);
            };
        }
        public Action LDH((Register, Traits) p0, (DMGInteger, Traits) p1, int duration)
        {
            return () =>
            {
                Registers.Set(p0.Item1, (byte)Memory.Fetch(p1.Item1));
                AddTicks(duration);
            };
        }
        public Action DI(int duration)
        {
            return () =>
            {
                disableInterrupts();
                AddTicks(duration);
            };
        }
        public Action ILLEGAL_F4(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action OR((DMGInteger, Traits) p0, int duration)
        {
            return () =>
            {
                Registers.A = ((byte)(Registers.A | (byte)Memory.Fetch(p0.Item1)));
                AddTicks(duration);
            };
        }

        public Action LD((WideRegister, Traits) p0, (WideRegister, Traits) p1, (DMGInteger, Traits) p2, int duration)
        {
            return () =>
            {
                var offset = (sbyte)Memory.Fetch(DMGInteger.r8);
                var SP = Registers.SP;
                Registers.Set(WideRegister.HL, (ushort)(SP + offset));
                AddTicks(duration);
            };
        }

        public Action LD((WideRegister, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {
                var newSPValue = Registers.Get(p1.Item1);
                Registers.Set(p0.Item1, newSPValue);
                AddTicks(duration);
            };

        }
        public Action EI(int duration)
        {
            return () =>
            {
                enableInterrupts();
                AddTicks(duration);
            };
        }
        public Action ILLEGAL_FC(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action ILLEGAL_FD(int duration)
        {
            return () =>
            {
                AddTicks(duration);
                throw new Exception("illegal");
            };
        }
        public Action CP((DMGInteger, Traits) p0, int duration)
        {
            return () =>
            {
                var lhs = Registers.A;
                var rhs = (byte)Memory.Fetch(p0.Item1);
                CP(lhs, rhs);
                AddTicks(duration);
            };
        }
        private void CP(byte lhs, byte rhs)
        {
            Registers.Mark(Flag.N);
            var sum = lhs - rhs;

            Registers.Set(Flag.Z, ((byte)sum) == 0);
            Registers.Set(Flag.C, lhs < rhs);
            Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));
        }

        public Action RLC((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);

                var res = RLC(reg);
                Registers.Set(Flag.Z, res == 0);

                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action RLC((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = RLC(reg);
                Registers.Set(Flag.Z, res == 0);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
        public Action RRC((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);

                var res = RRC(reg);
                Registers.Set(Flag.Z, res == 0);

                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action RRC((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = RRC(reg);
                Registers.Set(Flag.Z, res == 0);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
        public Action RL((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);

                var res = RL(reg);
                Registers.Set(Flag.Z, res == 0);

                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action RL((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = RL(reg);
                Registers.Set(Flag.Z, res == 0);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
        public Action RR((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);

                var res = RR(reg);
                Registers.Set(Flag.Z, res == 0);

                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action RR((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = RR(reg);
                Registers.Set(Flag.Z, res == 0);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
        public Action SLA((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);
                var res = SLA(reg);

                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action SLA((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = SLA(reg);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
        private byte SLA(byte reg)
        {
            var TopBit = reg.GetBit(7);

            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Set(Flag.C, TopBit);

            reg <<= 1;
            Registers.Set(Flag.Z, reg == 0);
            return reg;
        }
        public Action SRA((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);
                var res = SR(reg);

                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action SRA((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = SR(reg);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }

        private byte SR(byte reg)
        {
            var BottomBit = reg.GetBit(0);

            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);

            Registers.Set(Flag.C, BottomBit);

            reg = (byte)(reg >> 1 | reg & 0x80);
            Registers.Set(Flag.Z, reg == 0);
            return reg;
        }

        public Action SWAP((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);
                var res = SWAP(reg);
                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action SWAP((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = SWAP(reg);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }

        private byte SWAP(byte b)
        {
            var low = (b & 0xf) << 4;
            var high = (b & 0xf0) >> 4;
            var swapped = low | high;

            Registers.Set(Flag.Z, swapped == 0);
            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NC);
            return (byte)swapped;
        }

        public Action SRL((Register, Traits) p0, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p0.Item1);
                var res = SR(reg);
                Registers.Set(p0.Item1, res);
                AddTicks(duration);
            };
        }
        public Action SRL((WideRegister, Traits) p0, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p0.Item1);
                var reg = Memory.Read(addr);

                var res = SR(reg);

                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
        public Action BIT((byte, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p1.Item1);
                BIT(p0.Item1, reg);
                AddTicks(duration);
            };
        }
        public Action BIT((byte, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p1.Item1);
                var reg = Memory.Read(addr);
                BIT(p0.Item1, reg);
                AddTicks(duration);
            };
        }

        private void BIT(int at, byte b)
        {
            Registers.Set(Flag.Z, !b.GetBit(at));
            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.H);
        }
        private byte RES(int at, byte b) => b.ClearBit(at);
        private byte SET(int at, byte b) => b.SetBit(at);

        public Action RES((byte, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p1.Item1);
                var res = RES(p0.Item1, reg);
                Registers.Set(p1.Item1, res);
                AddTicks(duration);
            };
        }
        public Action RES((byte, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p1.Item1);
                var reg = Memory.Read(addr);

                var res = RES(p0.Item1, reg);
                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
        public Action SET((byte, Traits) p0, (Register, Traits) p1, int duration)
        {
            return () =>
            {
                var reg = Registers.Get(p1.Item1);
                var res = SET(p0.Item1, reg);
                Registers.Set(p1.Item1, res);
                AddTicks(duration);
            };
        }
        public Action SET((byte, Traits) p0, (WideRegister, Traits) p1, int duration)
        {
            return () =>
            {
                var addr = Registers.Get(p1.Item1);
                var reg = Memory.Read(addr);

                var res = SET(p0.Item1, reg);
                Memory.Write(addr, res);
                AddTicks(duration);
            };
        }
    }
}