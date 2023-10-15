
namespace emulator.opcodes;
public class OAMCorruptionEventArgs : EventArgs
{
    public bool IsOAMReadOrWrite { get; set; }
}
