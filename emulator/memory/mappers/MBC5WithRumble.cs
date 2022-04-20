using System.IO.MemoryMappedFiles;

namespace emulator;

internal class MBC5WithRumble : MBC5
{
    public MBC5WithRumble(CartHeader cartHeader, byte[] gameROM, MemoryMappedFile memoryMappedFile) : base(cartHeader, gameROM, memoryMappedFile)
    {
    }

    private bool RumbleState;
    protected override void SetRAMBank(byte value)
    {
        base.SetRAMBank(value);
        var NewRumbleState = value.GetBit(3);
        if (NewRumbleState != RumbleState)
        {
            RumbleState = NewRumbleState;
            OnRumbleStateChanged(EventArgs.Empty);
        }
    }

    public event EventHandler? RumbleStateChange;

    protected virtual void OnRumbleStateChanged(EventArgs e) => RumbleStateChange?.Invoke(this, e);
}
