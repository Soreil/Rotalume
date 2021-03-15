using System;

namespace emulator
{
    public class InterruptRegisters
    {
        public bool IME;
        public bool InterruptEnableScheduled;
        private byte _IE = 0xe0;

        private bool _gamePadInterruptReady;
        public bool GamePadInterruptReady()
        {
            var was = _gamePadInterruptReady;
            _gamePadInterruptReady = false;
            return was;
        }
        public byte InterruptFireRegister
        {
            get => _IE;
            set => _IE = (byte)((value & 0x1f) | 0xe0);
        }
        public byte InterruptControlRegister;

        public void TriggerEvent(object? sender, EventArgs e) => _gamePadInterruptReady = true;

        internal (Action<byte> Write, Func<byte> Read) HookUp() => (x => InterruptFireRegister = x,
    () => InterruptFireRegister);
    }
}