extern alias PreviousEmulator;
using emulator.input;

namespace BenchmarkSuite1;

public sealed class QuietController : IGameController, PreviousEmulator::emulator.input.IGameController
{
    public bool IsBPressed => false;
    public bool IsSelectPressed => false;
    public bool IsAPressed => false;
    public bool IsDPadDownPressed => false;
    public bool IsDPadLeftPressed => false;
    public bool IsDPadRightPressed => false;
    public bool IsDPadUpPressed => false;
    public bool IsStartPressed => false;

    public void AddEventHandler(EventHandler<EventArgs> e)
    {
    }

    public void RemoveEventHandler(EventHandler<EventArgs> e)
    {
    }

    public void Vibrate(double leftMotor, double rightMotor)
    {
    }
}
