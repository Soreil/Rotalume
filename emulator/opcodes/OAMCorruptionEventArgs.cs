
namespace emulator.opcodes;
public enum OAMCorruptionKind
{
    Address,
    Write,
    Read,
    ReadAndIncrement
}

public class OAMCorruptionEventArgs : EventArgs
{
    public OAMCorruptionKind Kind { get; init; }
    public bool IsOAMReadOrWrite => Kind != OAMCorruptionKind.Address;
}
