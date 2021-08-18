namespace emulator
{
    public interface IGameController
    {
        bool IsBPressed { get; }
        bool IsSelectPressed { get; }
        bool IsAPressed { get; }
        bool IsDPadDownPressed { get; }
        bool IsDPadLeftPressed { get; }
        bool IsDPadRightPressed { get; }
        bool IsDPadUpPressed { get; }
        bool IsStartPressed { get; }
        void Vibrate(double leftMotor, double rightMotor);

        public void AddEventHandler(EventHandler<EventArgs> e);
        public void RemoveEventHandler(EventHandler<EventArgs> e);
    }
}
