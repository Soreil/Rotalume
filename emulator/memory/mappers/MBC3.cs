﻿using System;
using System.IO.MemoryMappedFiles;

namespace emulator
{
    internal class MBC3 : MBC
    {
        private readonly byte[] gameROM;

        public MemoryMappedViewAccessor RAMBanks { get; }

        private bool RAMEnabled = false;
        private const int ROMBankSize = 0x4000;
        private readonly int RAMBankSize = RAMSize;
        private const int lowBank = 0;
        private int ROMBankNumber = 1;
        private int RAMBankNumber = 0;
        private int RTCRegisterNumber = 0;
        private readonly Func<long>? GetRTC;
        private readonly bool hasClock;

        private const long TicksPerSecond = 1 << 22;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;
        private bool RTCSelected = false;

        private byte PreviousLatchControlWriteValue = 0xff; //Setting this to 0 would mean it triggers on the first 1 write with a 0
        private long BaseToSubtractFromClock = 0; //We need to load this as well as the RAM in on load of the cartridge
        private long LatchedTime = 0;
        private bool ClockIsPaused = false;
        private bool DateOverflow = false;
        private long PausedClock = 0;

        private System.IO.MemoryMappedFiles.MemoryMappedViewAccessor? ClockStorage;

        public MBC3(CartHeader header, byte[] gameROM, System.IO.MemoryMappedFiles.MemoryMappedFile file, Func<long>? getClock = null)
        {
            this.gameROM = gameROM;
            RAMBanks = file.CreateViewAccessor(0, header.RAM_Size);


            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0)
            {
                RAMBankSize = 0;
            }

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0x800)
            {
                RAMBankSize = 0x800;
            }

            if (getClock is not null)
            {
                ClockStorage = file.CreateViewAccessor(header.RAM_Size, 16);
                hasClock = true;
                var InitialOffsetFromSave = ClockStorage.ReadInt64(0);
                var timeOfLastSave = ClockStorage.ReadInt64(8);
                var DotNetTicksElapsed = DateTime.Now.Ticks - timeOfLastSave;
                var GameboyTicksElapsed = (long)(DotNetTicksElapsed * (TicksPerSecond / 10000000.0));
                InitialOffsetFromSave += GameboyTicksElapsed;

                GetRTC = () => getClock() + InitialOffsetFromSave;
            }
        }

        public Action SaveRTC() => !hasClock
                ? throw new Exception("No clock present")
                : (() =>
            {
                var time = GetRTC!();
                ClockStorage!.Write(0, time);
                var realTime = DateTime.Now.Ticks;
                ClockStorage!.Write(8, realTime);
            });

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
                    if (value >= 8 && value < 0x0d)
                    {
                        RTCRegisterNumber = value;
                        RTCSelected = true;
                    }
                    if (value <= 3)
                    {
                        RAMBankNumber = value & 0x03;
                        RTCSelected = false;
                    }
                    break;
                    case var v when v < 0x8000:
                    if (PreviousLatchControlWriteValue == 0 && value == 1 && hasClock)
                    {
                        if (CurrentClock >= TicksPerDay * 0x200)
                        {
                            DateOverflow = true;
                            CurrentClock %= (TicksPerDay * 0x200);
                        }
                        LatchedTime = CurrentClock;
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

        private static bool IsUpperBank(int n) => n >= ROMBankSize;

        public byte GetRAM(int n)
        {
            if (!RTCSelected)
            {
                return (byte)(RAMEnabled ? RAMBanks.ReadByte((RAMBankNumber * RAMBankSize) + n - RAMStart) : 0xff);
            }
#pragma warning disable CS8509 // Exhaustive
            return RTCRegisterNumber switch
#pragma warning restore CS8509 // Exhaustive
            {
                0x08 => (byte)((LatchedTime % TicksPerMinute) / TicksPerSecond),
                0x09 => (byte)((LatchedTime % TicksPerHour) / TicksPerMinute),
                0x0a => (byte)((LatchedTime % TicksPerDay) / TicksPerHour),
                0x0b => (byte)(LatchedTime / TicksPerDay),
                0x0c => MakeFlags(LatchedTime / TicksPerDay),
            };
        }

        private byte MakeFlags(long days)
        {
            bool MSB = (days & 0x100) == 0x100;
            byte flags = 0;
            flags.SetBit(0, MSB);
            flags.SetBit(6, ClockIsPaused);
            flags.SetBit(7, DateOverflow);
            return flags;
        }

        private void SetRAM(int n, byte v)
        {
            if (RAMEnabled && !RTCSelected)
            {
                RAMBanks.Write((RAMBankNumber * RAMBankSize) + n - RAMStart, v);
            }

            if (RTCSelected)
            {
                SetRTCRegister(v);
            }
        }

        private long CurrentClock
        {
            get
            {
                if (ClockIsPaused)
                {
                    return PausedClock;
                }
                else
                {
                    return GetRTC!() - BaseToSubtractFromClock;
                }
            }
            set
            {
                if (ClockIsPaused)
                {
                    PausedClock = value;
                }
                else
                {
                    BaseToSubtractFromClock = GetRTC!() - value;
                }
            }
        }

        private void SetRTCRegister(byte v)
        {
            if (RTCRegisterNumber == 0x0c)
            {
                if (!ClockIsPaused && v.GetBit(6))
                {
                    StopClock();
                }
                else if (ClockIsPaused)
                {
                    ReactivateClock();
                }

                if (v.GetBit(7))
                {
                    SetCarry();
                }
                else
                {
                    UnSetCarry();
                }

                if (v.GetBit(0))
                {
                    SetDayCounterMSB();
                }
                else
                {
                    UnSetDayCounterMSB();
                }

                return;
            }

            (long days, byte hours, byte minutes, byte seconds, long remainder) = GetDateComponents(CurrentClock);
            var daysTopBit = days & 0x100;

            switch (RTCRegisterNumber)
            {
                case 0x08:
                seconds = (byte)(v & 0x3f);
                remainder = 0; //We have to reset the subseconds in case we want to set the second component of the RTC
                break;
                case 0x09:
                minutes = (byte)(v & 0x3f);
                break;
                case 0x0a:
                hours = (byte)(v & 0x1f);
                break;
                case 0x0b:
                days = v + daysTopBit;
                break;
            };
            CurrentClock = MakeDate(days, hours, minutes, seconds, remainder);
        }

        private void ReactivateClock()
        {
            if (ClockIsPaused)
            {
                ClockIsPaused = false;
                BaseToSubtractFromClock = GetRTC!() - PausedClock;
                PausedClock = long.MinValue; //Sanity check
            }
        }

        private void UnSetCarry() => DateOverflow = false;

        private void UnSetDayCounterMSB()
        {
            if (CurrentClock / TicksPerDay >= 0x100)
            {
                CurrentClock -= (TicksPerDay * 0x100);
            }
        }

        private void SetDayCounterMSB()
        {
            if (CurrentClock / TicksPerDay < 0x100)
            {
                CurrentClock += (TicksPerDay * 0x100);
            }
        }

        private void SetCarry() => DateOverflow = true;

        private void StopClock()
        {
            ClockIsPaused = true;
            PausedClock = GetRTC!() - BaseToSubtractFromClock;
        }

        private static long MakeDate(long days, byte hours, byte minutes, byte seconds, long remainder) => (days * TicksPerDay) + (hours * TicksPerHour) + (minutes * TicksPerMinute) + (seconds * TicksPerSecond) + remainder;
        private static (long days, byte hours, byte minutes, byte seconds, long remainder) GetDateComponents(long timeSpan)
        {
            var days = (timeSpan / TicksPerDay);
            var hours = (byte)(timeSpan % TicksPerDay / TicksPerHour);
            var minutes = (byte)(timeSpan % TicksPerHour / TicksPerMinute);
            var seconds = (byte)(timeSpan % TicksPerMinute / TicksPerSecond);
            var remainder = timeSpan % TicksPerSecond;
            return (days, hours, minutes, seconds, remainder);
        }
    }
}