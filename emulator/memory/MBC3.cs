using System.Collections.Generic;
using System;

namespace emulator
{
    internal class MBC3 : MBC
    {
        private readonly byte[] gameROM;

        private bool RAMEnabled = false;
        const int ROMBankSize = 0x4000;
        int RAMBankSize = RAMSize;

        const int lowBank = 0;

        int RAMBankCount;
        int ROMBankCount;

        int ROMBankNumber = 1;
        int RAMBankNumber = 0;
        int RTCRegisterNumber = 0;

        Func<long> RTCSource;
        const long TicksPerSecond = 1 << 22;
        const long TicksPerMinute = TicksPerSecond * 60;
        const long TicksPerHour = TicksPerSecond * 60;
        const long TicksPerDay = TicksPerHour * 24;
        bool RTCSelected = false;

        private byte PreviousLatchControlWriteValue = 0;
        private long StartTime = 0; //We need to load this as well as the RAM in on load of the cartridge
        private long LatchedTime = 0;
        private bool halted = false;
        private long haltedAt = 0;
        public MBC3(CartHeader header, byte[] gameROM, Func<long> getClock)
        {
            this.gameROM = gameROM;
            ROMBankCount = this.gameROM.Length / 0x4000;
            if (header.Type == CartType.MBC1_RAM && header.RAM_Size == 0) header = header with { RAM_Size = 0x2000 };
            RAMBankCount = Math.Max(1, header.RAM_Size / RAMBankSize);
            RAMBanks = new byte[header.RAM_Size];

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0)
                RAMBankSize = 0;

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0x800)
                RAMBankSize = 0x800;

            RTCSource = getClock;
        }

        public override byte this[int n]
        {
            get => n >= RAMStart ? GetRAM(n) : GetROM(n);
            set
            {
                switch (n)
                {
                    case var v when v < 0x2000:
                        RAMEnabled = (value & 0x0F) == 0x0A;
                        break;
                    case var v when v < 0x4000:
                        ROMBankNumber = value == 0 ? 1 : value & 0x7f;
                        break;
                    case var v when v < 0x6000:
                        if (value > 3 && value < 0x0d)
                        {
                            RTCRegisterNumber = value & 0x0c;
                            RTCSelected = true;
                        }
                        if (value < 3)
                        {
                            RAMBankNumber = value & 0x03;
                            RTCSelected = false;
                        }
                        break;
                    case var v when v < 0x8000:
                        if (PreviousLatchControlWriteValue == 0 && value == 1)
                        {
                            if (halted)
                                LatchedTime = haltedAt - StartTime;
                            else
                                LatchedTime = RTCSource() - StartTime;
                        }
                        PreviousLatchControlWriteValue = value;
                        break;
                    default:
                        SetRAM(n, value);
                        break;
                }
            }
        }

        public byte GetROM(int n) => IsUpperBank(n) ? ReadHighBank(n) : ReadLowBank(n);
        private byte ReadLowBank(int n) => gameROM[lowBank * ROMBankSize + n];
        private byte ReadHighBank(int n) => gameROM[ROMBankNumber * ROMBankSize + (n - ROMBankSize)];

        private bool IsUpperBank(int n) => n >= ROMBankSize;

        public byte GetRAM(int n)
        {
            if (!RTCSelected) return RAMEnabled ? RAMBanks[(RAMBankNumber * RAMBankSize) + n - RAMStart] : 0xff;
            return RTCRegisterNumber switch
            {
                0x08 => (byte)(LatchedTime % TicksPerMinute / TicksPerSecond),
                0x09 => (byte)(LatchedTime % TicksPerHour / TicksPerMinute),
                0x0a => (byte)(LatchedTime % TicksPerDay / TicksPerHour),
                0x0b => (byte)((LatchedTime / TicksPerDay) & 0xff),
                0x0c => MakeFlags(LatchedTime / TicksPerDay),
            };
        }

        private byte MakeFlags(long days)
        {
            byte flags = 0xff;
            flags = flags.SetBit(0, (days & 0x100) == 0x100);
            flags = flags.SetBit(6, !halted);
            flags = flags.SetBit(7, days >= 0x200);
            return flags;
        }

        private void SetRAM(int n, byte v)
        {
            if (RAMEnabled && !RTCSelected) RAMBanks[(RAMBankNumber * RAMBankSize) + n - RAMStart] = v;
            if (RTCSelected)
            {
                if (RTCRegisterNumber == 0x0c)
                {
                    if (!halted && v.GetBit(6))
                    {
                        halted = true;
                        haltedAt = RTCSource();
                    }
                    else if (!v.GetBit(6))
                    {
                        halted = false;
                        StartTime = RTCSource() - haltedAt;
                    }
                    return;
                }
                if (!halted) return;

                var days = (byte)(haltedAt / TicksPerDay);
                var hours = (byte)(haltedAt % TicksPerDay / TicksPerHour);
                var minutes = (byte)(haltedAt % TicksPerHour / TicksPerMinute);
                var seconds = (byte)(haltedAt % TicksPerMinute / TicksPerSecond);

                switch (RTCRegisterNumber)
                {
                    case 0x08:
                        haltedAt += (seconds - v) * TicksPerSecond;
                        break;
                    case 0x09:
                        haltedAt += (minutes - v) * TicksPerMinute;
                        break;
                    case 0x0a:
                        haltedAt += (hours - v) * TicksPerHour;
                        break;
                    case 0x0b:
                        haltedAt += (days - v) * TicksPerDay;
                        break;
                };

            }
        }
    }
}