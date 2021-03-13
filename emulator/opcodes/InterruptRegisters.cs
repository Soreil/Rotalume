using System;

namespace emulator
{
    public class InterruptRegisters
    {
        public bool IME;
        public bool InterruptEnableScheduled;
        public byte _IE = 0xe0;

        public readonly Func<bool> GetKeyboardInterrupt;
        public byte InterruptFireRegister
        {
            get => _IE;
            set => _IE = (byte)((value & 0x1f) | 0xe0);
        }
        public byte InterruptControlRegister;

        public InterruptRegisters(Func<bool> KeyboardInterrupt) => GetKeyboardInterrupt = KeyboardInterrupt;
        internal (Action<byte> Write, Func<byte> Read) HookUp() => (x => InterruptFireRegister = x,
    () => InterruptFireRegister);
    }
}