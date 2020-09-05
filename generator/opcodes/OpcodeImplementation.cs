using System;
namespace generator
{
    public partial class Decoder
    {
        readonly public Registers Registers;
        readonly public Storage Storage;
        readonly public Action<ushort> SetPC;
        readonly public Func<ushort> GetPC;

        private ushort pop()
        {
            var SP = Registers.SP.Read();
            var popped = Storage.ReadWide(SP);
            SP += 2;
            Registers.SP.Write(SP);
            return popped;
        }
        private void push(ushort s)
        {
            Storage.Write(Registers.SP.Read(), s);
            Registers.SP.Write((ushort)(Registers.SP.Read() - 2));
        }
        public Action NOP()
        {
            return () => { };
        }
        public Action LD((WideRegister, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                var arg = Storage.Fetch(p1.Item1);
                if (!p0.Item2.Immediate)
                {
                    var HL = Registers.Get(p0.Item1);
                    Storage.Write(HL, (byte)arg);
                }
                else
                {
                    Registers.Set(p0.Item1, (ushort)arg);
                }
            };
        }
        public Action LD((WideRegister, Traits) p0, (Register, Traits) p1)
        {
            return () =>
            {
                var address = Registers.Get(p0.Item1);
                var value = Registers.Get(p1.Item1);

                Storage.Write(address, value);

                switch (p0.Item2.Postfix)
                {
                    default:
                        break;
                    case Postfix.increment:
                        Registers.Set(p0.Item1, (ushort)(address + 1));
                        break;
                    case Postfix.decrement:
                        Registers.Set(p0.Item1, (ushort)(address - 1));
                        break;
                }
            };
        }
        public Action INC((WideRegister, Traits) p0)
        => () =>
            {
                //Wide registers do not use flags for INC and DEC
                if (p0.Item2.Immediate)
                    Registers.Set(p0.Item1, (ushort)(Registers.Get(p0.Item1) + 1));
                else
                {
                    var addr = Registers.Get(p0.Item1);
                    var before = Storage.Read(addr);
                    var arg = (byte)(before + 1);
                    Storage.Write(addr, arg);

                    Registers.Set(Flag.Z, arg == 0);
                    Registers.Mark(Flag.NN);
                    Registers.Set(Flag.H, before.IsHalfCarryAdd(1));
                }
            };

        public Action INC((Register, Traits) p0)
        => () =>
            {
                var before = Registers.Get(p0.Item1);
                var arg = (byte)(before + 1);
                Registers.Set(p0.Item1, arg);

                Registers.Set(Flag.Z, arg == 0);
                Registers.Mark(Flag.NN);
                Registers.Set(Flag.H, before.IsHalfCarryAdd(1));
            };
        public Action DEC((Register, Traits) p0)
        => () =>
             {
                 var before = Registers.Get(p0.Item1);
                 var arg = (byte)(before - 1);
                 Registers.Set(p0.Item1, arg);

                 Registers.Set(Flag.Z, arg == 0);
                 Registers.Mark(Flag.N);
                 Registers.Set(Flag.H, before.IsHalfCarrySub(1));
             };
        public Action LD((Register, Traits) p0, (DMGInteger, Traits) p1)
            => () =>
            {
                var arg = Storage.Fetch(p1.Item1);
                Registers.Set(p0.Item1, (byte)arg);
            };
        public Action RLCA()

            => () =>
            {
                var A = Registers.A.Read();
                var TopBit = A.GetBit(7);

                Registers.Mark(Flag.NZ);
                Registers.Mark(Flag.NN);
                Registers.Mark(Flag.NH);
                Registers.Set(Flag.C, TopBit);

                A <<= 1;
                A += TopBit ? 1 : 0;
                Registers.A.Write(A);
            };

        //This op is a litle weird, we should have generate
        //lefthandsided shorts as being a different type.
        public Action LD((DMGInteger, Traits) p0, (WideRegister, Traits) p1)
            => () =>
            {
                var addr = (ushort)Storage.Fetch(DMGInteger.d16);
                var arg = Registers.Get(p1.Item1);

                Storage.Write(addr, arg);
            };

        public Action ADD((WideRegister, Traits) p0, (WideRegister, Traits) p1)
        {
            return () =>
            {

                var target = Registers.Get(p0.Item1);
                var arg = Registers.Get(p1.Item1);

                var result = (ushort)(target + arg);
                Registers.Set(p0.Item1, result);

                Registers.Set(Flag.N, false);
                Registers.Set(Flag.H, target.IsHalfCarryAdd(arg));
                Registers.Set(Flag.C, target + arg > 0xFFFF);
            };
        }
        public Action LD((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () =>
            {
                var addr = Registers.Get(p1.Item1);
                var value = Storage.Read(addr);

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
            };
        }
        public Action DEC((WideRegister, Traits) p0)
        => () =>
        {
            //Wide registers do not use flags for INC and DEC
            if (p0.Item2.Immediate)
                Registers.Set(p0.Item1, (ushort)(Registers.Get(p0.Item1) + 1));
            else
            {
                var addr = Registers.Get(p0.Item1);
                var before = Storage.Read(addr);
                var arg = (byte)(before + 1);
                Storage.Write(addr, arg);

                Registers.Set(Flag.Z, arg == 0);
                Registers.Mark(Flag.NN);
                Registers.Set(Flag.H, before.IsHalfCarryAdd(1));
            }
        };
        public Action RRCA()
            => () =>
            {
                var A = Registers.A.Read();
                var BottomBiy = A.GetBit(0);

                Registers.Mark(Flag.NZ);
                Registers.Mark(Flag.NN);
                Registers.Mark(Flag.NH);
                Registers.Set(Flag.C, BottomBiy);

                A >>= 1;
                if (BottomBiy) A += 0x80;
                Registers.A.Write(A);
            };
        public Action STOP()
        {
            return () => { throw new Exception("Yea we ain't stopping clean partner"); };
        }
        public Action RLA()
            => () =>
            {
                var A = Registers.A.Read();
                var TopBit = A.GetBit(7);
                var OldBit = Registers.Get(Flag.C);

                Registers.Mark(Flag.NZ);
                Registers.Mark(Flag.NN);
                Registers.Mark(Flag.NH);
                Registers.Set(Flag.C, TopBit);

                A <<= 1;
                A += OldBit ? 1 : 0;
                Registers.A.Write(A);
            };
        public Action JR((DMGInteger, Traits) p0)
        {
            return () =>
            {
                var offset = (sbyte)Storage.Fetch(p0.Item1);
                SetPC((ushort)(GetPC() + offset));
            };
        }
        public Action RRA()
            => () =>
            {
                var A = Registers.A.Read();
                var TopBit = A.GetBit(0);
                var OldBit = Registers.Get(Flag.C);

                Registers.Mark(Flag.NZ);
                Registers.Mark(Flag.NN);
                Registers.Mark(Flag.NH);
                Registers.Set(Flag.C, TopBit);

                A >>= 1;
                if (OldBit) A += 0x80;
                Registers.A.Write(A);
            };
        public Action JR((Flag, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                if (Registers.Get(p0.Item1))
                {
                    var offset = (sbyte)Storage.Fetch(p1.Item1);
                    SetPC((ushort)(GetPC() + offset));
                }
            };
        }
        public Action DAA()
        {
            return () =>
            {
                Registers.Mark(Flag.NH);

                var wasSub = Registers.Get(Flag.N);

                var A = (ushort)Registers.A.Read();
                if (!wasSub)
                {
                    var low = A & 0xf;
                    if (low > 9 || Registers.Get(Flag.H)) A += 0x06;
                    var high = (A >> 4) & 0xf;
                    if (high > 9 || Registers.Get(Flag.C)) A += 0x60;
                }
                else
                {
                    var low = A & 0xf;
                    if (low > 9 || Registers.Get(Flag.H)) A -= 0x06;
                    var high = (A >> 4) & 0xf;
                    if (high > 9 || Registers.Get(Flag.C)) A -= 0x60;
                }
                Registers.Set(Flag.Z, A == 0);
                Registers.Set(Flag.C, A > 0x99);

                Registers.A.Write((byte)A);
            };
        }
        public Action CPL() => () =>
        {
            Registers.A.Write((byte)~Registers.A.Read());
            Registers.Mark(Flag.N);
            Registers.Mark(Flag.H);
        };

        public Action SCF() => () =>
        {
            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.C);
        };

        public Action CCF() => () =>
        {
            Registers.Mark(Flag.NN);
            Registers.Mark(Flag.NH);
            Registers.Set(Flag.C, !Registers.Get(Flag.C));
        };
        public Action LD((Register, Traits) p0, (Register, Traits) p1)
        {
            if (p0.Item2.Immediate && p1.Item2.Immediate)
                return () =>
                {
                    var arg = Registers.Get(p1.Item1);
                    Registers.Set(p0.Item1, arg);
                };
            else
            {
                if (p0.Item2.Immediate)
                    return () =>
                        Registers.Set(p0.Item1, Storage.Read((ushort)(0xFF00 + Registers.Get(p1.Item1))));
                else
                    return () =>
                        Storage.Write((ushort)(0xFF00 + Registers.Get(p0.Item1)), Registers.Get(p1.Item1));
            };
        }
        public Action HALT()
        {
            return () => { throw new Exception("Not implemented"); };
        }
        public Action ADD((Register, Traits) p0, (Register, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = Registers.Get(p1.Item1);
                var sum = lhs + rhs;

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.C, sum > 0xff);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd(rhs));

                Registers.Set(p0.Item1, (byte)sum);
            };
        }
        public Action ADD((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = Storage.Read(Registers.Get(p1.Item1));
                var sum = lhs + rhs;

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.C, sum > 0xff);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd(rhs));

                Registers.Set(p0.Item1, (byte)sum);
            };
        }
        public Action ADC((Register, Traits) p0, (Register, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = Registers.Get(p1.Item1);
                var sum = lhs + rhs + (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd((byte)(rhs + (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, sum > 0xff);

                Registers.Set(p0.Item1, (byte)sum);
            };
        }
        public Action ADC((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = Storage.Read(Registers.Get(p1.Item1));
                var sum = lhs + rhs + (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd((byte)(rhs + (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, sum > 0xff);

                Registers.Set(p0.Item1, (byte)sum);
            };
        }
        public Action SUB((Register, Traits) p0)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.A.Read();
                var rhs = Registers.Get(p0.Item1);
                var sum = lhs - rhs;

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.C, lhs < rhs);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));

                Registers.A.Write((byte)sum);
            };
        }
        public Action SUB((WideRegister, Traits) p0)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.A.Read();
                var rhs = Storage.Read(Registers.Get(p0.Item1));
                var sum = lhs - rhs;

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.C, lhs < rhs);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));

                Registers.A.Write((byte)sum);
            };
        }
        public Action SBC((Register, Traits) p0, (Register, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.Get(p0.Item1);
                var rhs = Registers.Get(p1.Item1);
                var sum = lhs - rhs - (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub((byte)(rhs - (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, lhs < rhs);

                Registers.Set(p0.Item1, ((byte)sum));
            };
        }
        public Action SBC((Register, Traits) p0, (WideRegister, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.Get(p0.Item1);
                var rhs = Storage.Read(Registers.Get(p1.Item1));
                var sum = lhs - rhs - (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub((byte)(rhs - (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, lhs < rhs);

                Registers.Set(p0.Item1, ((byte)sum));
            };
        }
        public Action AND((Register, Traits) p0) => () =>
        {
            var result = (byte)(Registers.A.Read() & Registers.Get(p0.Item1));
            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.H);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A.Write(result);
        };
        public Action AND((WideRegister, Traits) p0) => () =>
        {
            var result = (byte)(Registers.A.Read() & Storage.Read(Registers.Get(p0.Item1)));
            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.H);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A.Write(result);
        };
        public Action XOR((Register, Traits) p0) => () =>
        {
            var result = ((byte)(Registers.A.Read() ^ Registers.Get(p0.Item1)));
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A.Write(result);
        };
        public Action XOR((WideRegister, Traits) p0) => () =>
        {
            var result = ((byte)(Registers.A.Read() ^ Storage.Read(Registers.Get(p0.Item1))));
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A.Write(result);
        };
        public Action OR((Register, Traits) p0) => () =>
        {
            var result = ((byte)(Registers.A.Read() | Registers.Get(p0.Item1)));
            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A.Write(result);
        };
        public Action OR((WideRegister, Traits) p0) => () =>
        {
            var result = (byte)(Registers.A.Read() | Storage.Read(Registers.Get(p0.Item1)));
            Registers.Mark(Flag.NC);
            Registers.Mark(Flag.NH);
            Registers.Mark(Flag.NN);
            Registers.Set(Flag.Z, result == 0);

            Registers.A.Write(result);
        };
        public Action CP((Register, Traits) p0) => () =>
        {
            Registers.Mark(Flag.N);

            var lhs = Registers.A.Read();
            var rhs = Registers.Get(p0.Item1);
            var sum = lhs - rhs;

            Registers.Set(Flag.Z, sum == 0);
            Registers.Set(Flag.C, lhs < rhs);
            Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));
        };
        public Action CP((WideRegister, Traits) p0) => () =>
        {
            Registers.Mark(Flag.N);

            var lhs = Registers.A.Read();
            var rhs = Storage.Read(Registers.Get(p0.Item1));
            var sum = lhs - rhs;

            Registers.Set(Flag.Z, sum == 0);
            Registers.Set(Flag.C, lhs < rhs);
            Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));
        };
        public Action RET((Flag, Traits) p0)
        {
            return () =>
            {
                if (Registers.Get(p0.Item1))
                {
                    SetPC(pop());
                }
            };
        }
        public Action POP((WideRegister, Traits) p0)
        {
            return () =>
            {
                var SP = Registers.SP.Read();
                Registers.Set(p0.Item1, Storage.ReadWide(SP));
                SP += 2;
                Registers.SP.Write(SP);
            };
        }
        public Action JP((Flag, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                if (Registers.Get(p0.Item1))
                {
                    var addr = (ushort)Storage.Fetch(p1.Item1);
                    SetPC(addr);
                }
            };
        }
        public Action JP((DMGInteger, Traits) p0)
        {
            return () =>
            {
                var addr = (ushort)Storage.Fetch(p1.Item1);
                SetPC(addr);
            };
        }
        public Action CALL((Flag, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                if (Registers.Get(p0.Item1))
                {
                    push(GetPC());
                    var addr = (ushort)Storage.Fetch(p1.Item1);
                    SetPC(addr);
                }
            };
        }
        public Action PUSH((WideRegister, Traits) p0)
        {
            return () =>
            {
                push(Registers.Get(p0.Item1));
            };
        }
        public Action ADD((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = (byte)Storage.Fetch(p1.Item1);
                var sum = lhs + rhs;

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.C, sum > 0xff);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd(rhs));

                Registers.Set(p0.Item1, (byte)sum);
            };
        }
        public Action RST((byte, Traits) p0)
        {
            return () => { SetPC(p0.Item1); };
        }
        public Action RET()
        {
            return () =>
            {
                SetPC(pop());
            };
        }
        //Not an actual instruction
        public Action PREFIX()
        {
            return () => { throw new Exception("unimplementable"); };
        }
        public Action CALL((DMGInteger, Traits) p0)
        {
            return () =>
            {
                push(GetPC());
                var addr = (ushort)Storage.Fetch(p0.Item1);
                SetPC(addr);
            };
        }
        public Action ADC((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.NN);

                var lhs = Registers.Get(p0.Item1);
                var rhs = (byte)Storage.Fetch(p1.Item1);
                var sum = lhs + rhs + (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarryAdd((byte)(rhs + (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, sum > 0xff);

                Registers.Set(p0.Item1, (byte)sum);
            };
        }
        public Action ILLEGAL_D3()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action SUB((DMGInteger, Traits) p0)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.A.Read();
                var rhs = (byte)Storage.Fetch(p0.Item1);
                var sum = lhs - rhs;

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.C, lhs < rhs);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub(rhs));

                Registers.A.Write((byte)sum);
            };
        }
        public Action RETI()
        {
            return () =>
            {
                SetPC(pop());
                enableInterrupts();
            };
        }
        public Action ILLEGAL_DB()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action ILLEGAL_DD()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action SBC((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () =>
            {
                Registers.Mark(Flag.N);

                var lhs = Registers.Get(p0.Item1);
                var rhs = (byte)Storage.Fetch(p1.Item1);
                var sum = lhs - rhs - (Registers.Get(Flag.C) ? 1 : 0);

                Registers.Set(Flag.Z, sum == 0);
                Registers.Set(Flag.H, lhs.IsHalfCarrySub((byte)(rhs - (Registers.Get(Flag.C) ? 1 : 0))));
                Registers.Set(Flag.C, lhs < rhs);

                Registers.Set(p0.Item1, ((byte)sum));
            };
        }
        public Action LDH((DMGInteger, Traits) p0, (Register, Traits) p1)
        {
            return () =>
            {
                Storage.Write(p0.Item1, Registers.Get(p1.Item1));
            };
        }
        public Action ILLEGAL_E3()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action ILLEGAL_E4()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action AND((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action ADD((WideRegister, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { };
        }
        public Action JP((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action LD((DMGInteger, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action ILLEGAL_EB()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action ILLEGAL_EC()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action ILLEGAL_ED()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action XOR((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action LDH((Register, Traits) p0, (DMGInteger, Traits) p1)
        {
            return () => { Registers.Set(p0.Item1, (byte)Storage.Fetch(p1.Item1)); };
        }
        public Action DI()
        {
            return () => { disableInterrupts(); };
        }
        public Action ILLEGAL_F4()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action OR((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action LD((WideRegister, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action EI()
        {
            return () => { enableInterrupts(); };
        }
        public Action ILLEGAL_FC()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action ILLEGAL_FD()
        {
            return () => { throw new Exception("illegal"); };
        }
        public Action CP((DMGInteger, Traits) p0)
        {
            return () => { };
        }
        public Action RLC((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RLC((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RRC((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RRC((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RL((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RL((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action RR((Register, Traits) p0)
        {
            return () => { };
        }
        public Action RR((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SLA((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SLA((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SRA((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SRA((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SWAP((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SWAP((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action SRL((Register, Traits) p0)
        {
            return () => { };
        }
        public Action SRL((WideRegister, Traits) p0)
        {
            return () => { };
        }
        public Action BIT((byte, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action BIT((byte, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action RES((byte, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action RES((byte, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }
        public Action SET((byte, Traits) p0, (Register, Traits) p1)
        {
            return () => { };
        }
        public Action SET((byte, Traits) p0, (WideRegister, Traits) p1)
        {
            return () => { };
        }


    }
}