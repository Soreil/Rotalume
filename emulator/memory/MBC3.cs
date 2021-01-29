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

        Func<long> GetRTC;
        const long TicksPerSecond = 1 << 22;
        const long TicksPerMinute = TicksPerSecond * 60;
        const long TicksPerHour = TicksPerMinute * 60;
        const long TicksPerDay = TicksPerHour * 24;
        bool RTCSelected = false;

        private byte PreviousLatchControlWriteValue = 0;
        private long SetTime = 0; //We need to load this as well as the RAM in on load of the cartridge
        private long LatchedTime = 0;
        private bool halted = false;
        private bool DateOverflow = false;
        private long DateHaltedAgo = 0;
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

            GetRTC = getClock;
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
                            RTCRegisterNumber = value;
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
                                LatchedTime = DateHaltedAgo;
                            else
                                LatchedTime = GetRTC() - SetTime;
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
#pragma warning disable CS8509 // Exhaustive
            return RTCRegisterNumber switch
#pragma warning restore CS8509 // Exhaustive
            {
                0x08 => (byte)((LatchedTime % TicksPerMinute) / TicksPerSecond),
                0x09 => (byte)((LatchedTime % TicksPerHour) / TicksPerMinute),
                0x0a => (byte)((LatchedTime % TicksPerDay) / TicksPerHour),
                0x0b => (byte)((LatchedTime / TicksPerDay) & 0xff),
                0x0c => MakeFlags(LatchedTime / TicksPerDay),
            };
        }

        private byte MakeFlags(long days)
        {
            bool MSB = (days & 0x100) == 0x100;
            byte flags = 0xff;
            flags = flags.SetBit(0, MSB);
            flags = flags.SetBit(6, halted);
            flags = flags.SetBit(7, DateOverflow);
            return flags;
        }

        private void SetRAM(int n, byte v)
        {
            if (RAMEnabled && !RTCSelected) RAMBanks[(RAMBankNumber * RAMBankSize) + n - RAMStart] = v;
            if (RTCSelected)
            {
                SetRTCRegister(v);
            }
        }

        private void SetRTCRegister(byte v)
        {
            if (RTCRegisterNumber == 0x0c)
            {
                HandleRTCStop(v);
                return;
            }
            if (!halted) return;

            (long days, byte hours, byte minutes, byte seconds) = getDateComponents(DateHaltedAgo);
            long remainder = DateHaltedAgo % TicksPerSecond;
            var daysTopBit = days & 0x100;

            switch (RTCRegisterNumber)
            {
                case 0x08:
                    seconds = Math.Min((byte)59, v);
                    remainder = 0; //We have to reset the subseconds in case we want to set the second component of the RTC
                    break;
                case 0x09:
                    minutes = Math.Min((byte)59, v);
                    break;
                case 0x0a:
                    hours = Math.Min((byte)23, v);
                    break;
                case 0x0b:
                    days = v + daysTopBit;
                    break;
            };
            DateHaltedAgo = makeDate(days, hours, minutes, seconds, remainder);
        }

        private static long makeDate(long days, byte hours, byte minutes, byte seconds, long remainder)
        {
            return (days * TicksPerDay) + (hours * TicksPerHour) + (minutes * TicksPerMinute) + (seconds * TicksPerSecond) + remainder;
        }
        private static (long days, byte hours, byte minutes, byte seconds) getDateComponents(long timeSpan)
        {
            var days = (timeSpan / TicksPerDay);
            var hours = (byte)(timeSpan % TicksPerDay / TicksPerHour);
            var minutes = (byte)(timeSpan % TicksPerHour / TicksPerMinute);
            var seconds = (byte)(timeSpan % TicksPerMinute / TicksPerSecond);
            if (hours >= 24) throw new Exception("Hour overflow");
            if (minutes >= 60) throw new Exception("Minute overflow");
            if (seconds >= 60) throw new Exception("Second overflow");
            return (days, hours, minutes, seconds);
        }

        private void HandleRTCStop(byte v)
        {
            if (!halted && v.GetBit(6)) //Switch to timer stop mode
            {
                halted = true;
                //This delta is how many ticks before the current clock the halt is
                //Because this number is relative to the current clock our time isn't increasing
                DateHaltedAgo = GetRTC() - SetTime;
                if (DateHaltedAgo < 0) throw new Exception("Didn't expect that!");
                else
                {
                    if (DateHaltedAgo > TicksPerDay * 0x1ff)
                    {
                        DateOverflow = true;
                        DateHaltedAgo %= (TicksPerDay * 0x1ff);
                    }
                }
            }
            else if (halted && !v.GetBit(6)) //Reactivate the timer
            {
                halted = false;
                SetTime = GetRTC() - DateHaltedAgo;
            }
            if (!halted) return;

            DateOverflow = v.GetBit(7);

            //Set MSB to 0 therefore discard the days over 0xff;
            if (!v.GetBit(0))
            {
                DateHaltedAgo %= (TicksPerDay * 0xff);
            }
            //Set MSB to 1 by adding 0x100 if the bit is not already set
            else
            {
                DateHaltedAgo |= (TicksPerDay * 0x100);
            }
        }
    }
}