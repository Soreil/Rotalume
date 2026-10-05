namespace emulator.memory;

public class BootRomState
{
    public bool Active { get; set; }
    public byte[]? Rom { get; set; }
}

public class BootRom(byte[]? rom)
{
    public bool Active { get; private set; } = rom is not null;
    public byte Register { get => 0xff; set => Active = false; }

    public void Disable() => Active = false;

    internal BootRomState SerializeState()
    {
        return new BootRomState
        {
            Active = Active,
            Rom = rom,
        };


    }

    public byte this[int n] => Active ? rom![n] : throw new Exception("Lmao");
}