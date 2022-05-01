namespace emulator;

public class BootRom
{
    private readonly byte[]? rom;

    public BootRom(byte[]? rom)
    {
        Active = rom is not null;
        this.rom = rom;
    }

    public bool Active { get; private set; }
    public byte Register { get => 0xff; set => Active = false; }

    public void Disable() => Active = false;

    public byte this[int n] => Active ? rom![n] : throw new Exception("Lmao");
}