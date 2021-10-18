using emulator;


namespace Tests
{
    //Mockgamecontroller is a controller which can only be queried but never actually fires and controller events.
    //We will need a second mock controller at some point which can fire from a test input file.
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
