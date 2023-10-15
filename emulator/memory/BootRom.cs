namespace emulator.memory;

public class BootRom(byte[]? rom)
{
    public bool Active { get; private set; } = rom is not null;
    public byte Register { get => 0xff; set => Active = false; }

    public void Disable() => Active = false;

    public byte this[int n] => Active ? rom![n] : throw new Exception("Lmao");
}