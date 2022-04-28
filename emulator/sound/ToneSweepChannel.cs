
namespace emulator.sound;

internal class ToneSweepChannel : SquareChannel
{
    private bool sweepEnabled;
    private ushort shadowFrequency;
    private int sweepTimer;

    //https://nightshade256.github.io/2021/03/27/gb-sound-emulation.html
    public void TickSweep()
    {
        if (sweepTimer > 0) sweepTimer--;


        if (sweepTimer == 0)
        {
            sweepTimer = SweepPeriod == 0 ? 8 : SweepPeriod;

            if (sweepEnabled && SweepPeriod != 0)
            {

                var newFreq = CalculateSweepFrequency();

                //If the new frequency is 2047 or less and the sweep shift is not zero,
                //this new frequency is written back to the shadow frequency and square 1's frequency in NR13 and NR14
                if (newFreq < 2048 && SweepShift != 0)
                {
                    Frequency = newFreq;
                    shadowFrequency = newFreq;
                    //frequency calculation and overflow check are run AGAIN immediately using this new value,
                    //but this second new frequency is not written back.
                    _ = CalculateSweepFrequency();
                }
            }
        }
    }

    public void TriggerSweep()
    {
        shadowFrequency = Frequency;
        sweepTimer = SweepPeriod == 0 ? 8 : SweepPeriod;
        sweepEnabled = SweepPeriod != 0 || SweepShift != 0;

        //If the sweep shift is non - zero, frequency calculation and the overflow check are performed immediately.
        if (SweepShift != 0)
        {
            _ = CalculateSweepFrequency();
        }
    }

    private ushort CalculateSweepFrequency()
    {
        var newFreq = shadowFrequency >> SweepShift;
        newFreq = !SweepIncreasing ? shadowFrequency - newFreq : shadowFrequency + newFreq;

        //Overflow check
        if (newFreq > 2047) ChannelEnabled = false;

        return (ushort)newFreq;
    }

    private int SweepPeriod;
    private bool SweepIncreasing;
    private int SweepShift;

    public byte NR10
    {
        get => (byte)(0x80 | (SweepPeriod << 4) | (Convert.ToByte(SweepIncreasing) << 3) | SweepShift);

        set
        {
            SweepPeriod = (value >> 4) & 0x7;
            SweepIncreasing = value.GetBit(3);
            SweepShift = value & 0x7;
        }
    }

    public byte NR11
    {
        get => NRs1;
        set => NRs1 = value;
    }

    public byte NR12
    {
        get => NRs2;
        set => NRs2 = value;
    }

    public byte NR13
    {
        get => NRs3;
        set => NRs3 = value;
    }

    public byte NR14
    {
        get => NRs4;
        set => NRs4 = value;
    }


    protected override void Trigger()
    {
        //Square 1's sweep does several things (see frequency sweep).
        TriggerSweep();
        base.Trigger();
    }
}
