using System.Collections;

namespace emulator.sound;

public class LFSRState
{
    public required bool[] Bits { get; init; }
}

internal class LFSR
{
    public LFSRState GetState() => new()
    {
        Bits = [.. bits.Cast<bool>()]
    };

    private const int LFSRbitCount = 15;
    private BitArray bits;

    //Waveform output is bit 0 of the LFSR flipped
    internal bool Output() => !bits[0];

    internal void Step(bool WidthMode)
    {
        var newBit = bits[0] ^ bits[1];

        bits = bits.RightShift(1);
        bits.Set(LFSRbitCount - 1, newBit);

        if (WidthMode) bits.Set(6, newBit);
    }

    internal void ResetBits() => bits.SetAll(true);

    internal LFSR()
    {
        bits = new(LFSRbitCount);
        ResetBits();
    }
}