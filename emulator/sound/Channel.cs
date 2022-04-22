namespace emulator.sound;

public abstract class Channel
{
    public abstract bool IsOn();
    public abstract void Clock();
}