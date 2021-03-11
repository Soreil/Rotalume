using System;

namespace emulator
{
    public interface IGameController
    {
        bool IsAPressed { get; }
        bool IsBackPressed { get; }
        bool IsBPressed { get; }
        bool IsConnected { get; }
        bool IsDPadDownPressed { get; }
        bool IsDPadLeftPressed { get; }
        bool IsDPadRightPressed { get; }
        bool IsDPadUpPressed { get; }
        bool IsLeftShoulderPressed { get; }
        bool IsLeftStickPressed { get; }
        bool IsRightShoulderPressed { get; }
        bool IsRightStickPressed { get; }
        bool IsStartPressed { get; }
        bool IsXPressed { get; }
        bool IsYPressed { get; }
        (int X, int Y) LeftThumbStick { get; }
        int LeftTrigger { get; }
        (int X, int Y) RightThumbStick { get; }
        int RightTrigger { get; }
        void UpdateState();
        void Vibrate(double leftMotor, double rightMotor);
        void Vibrate(double leftMotor, double rightMotor, TimeSpan length);
    }

}
