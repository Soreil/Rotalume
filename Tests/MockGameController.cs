using emulator;

using System;


namespace Tests
{
    public class MockGameController : IGameController
    {
        public bool IsBPressed { get; }
        public bool IsSelectPressed { get; }
        public bool IsAPressed { get; }
        public bool IsDPadDownPressed { get; }
        public bool IsDPadLeftPressed { get; }
        public bool IsDPadRightPressed { get; }
        public bool IsDPadUpPressed { get; }
        public bool IsStartPressed { get; }

        public void AddEventHandler(EventHandler<EventArgs> e) { }
        public void RemoveEventHandler(EventHandler<EventArgs> e) { }
        public void Vibrate(double leftMotor, double rightMotor) { }
    }
}
