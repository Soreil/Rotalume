using System.IO.MemoryMappedFiles;

namespace emulator;

internal class MBC5WithRumble : MBC5
{
    public MBC5WithRumble(CartHeader cartHeader, byte[] gameROM, MemoryMappedFile memoryMappedFile) : base(cartHeader, gameROM, memoryMappedFile)
    {
    }

    private bool RumbleState;
    public override void SetRAMBank(byte value)
    {
        RAMBankNumber = value & 0x7 & (RAMBankCount - 1);
        var NewRumbleState = (value & 0x08) == 0x08;
        if (NewRumbleState != RumbleState)
        {
            RumbleState = NewRumbleState;
            OnRumbleStateChanged(EventArgs.Empty);
        }
    }

    public event EventHandler? RumbleStateChange;

    protected virtual void OnRumbleStateChanged(EventArgs e)
    {
        if (RumbleStateChange is not null)
            RumbleStateChange(this, e);
    }

}
