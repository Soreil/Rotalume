using System;

namespace emulator
{
    public interface IGameController
    {
        bool IsAPressed { get; }
        bool IsBackPressed { get; }
        bool IsBPressed { get; }
        bool IsDPadDownPressed { get; }
        bool IsDPadLeftPressed { get; }
        bool IsDPadRightPressed { get; }
        bool IsDPadUpPressed { get; }
        bool IsStartPressed { get; }
        void Vibrate(double leftMotor, double rightMotor);

        public event EventHandler<EventArgs>? KeyWentDown;
    }
}
