using emulator.graphics;
using emulator.memory;
using emulator.sound;

namespace emulator.glue;

public class Cycler(Timers timers, PPU ppu, APU apu, DMAControl dma, MasterClock masterClock, Samples sample)
{
    private readonly Timers Timers = timers;
    private readonly PPU Ppu = ppu;
    private readonly APU Apu = apu;
    private readonly DMAControl Dma = dma;
    private readonly MasterClock MasterClock = masterClock;
    private readonly Samples Samples = sample;

    public void Cycle()
    {
        for (int i = 0; i < 4; i++)
        {
            Timers.Tick();
            Ppu.Tick();
            Apu.Tick();
            Dma.DMA();
            MasterClock.Tick();
        }
        Samples.Sample();
    }
}
