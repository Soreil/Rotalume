using emulator.sound;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace emulator;

public class Core : IDisposable
{
    //Global clock used by RTC carts
    public long masterclock;

    public readonly CPU CPU;

    internal DMAControl DMA { get; }

    private readonly APU APU;
    private readonly PPU PPU;
    public readonly MMU Memory;
    private readonly Timers Timers;
    private readonly InterruptRegisters InterruptRegisters;

    private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) =>
            services.
        AddSingleton<APU>().
        AddSingleton<Timers>().
        AddSingleton<InterruptRegisters>().
        AddSingleton<MMU>().
        AddSingleton<Serial>().
        AddSingleton<OAM>().
        AddSingleton<VRAM>().
        AddSingleton<PPU>().
        AddSingleton<CPU>().
        AddSingleton<DMARegister>().
        AddSingleton<DMAControl>()
        );

    public Core(byte[] gameROM, byte[]? bootROM, string fileName, Keypad Keypad, IFrameSink frameSink)
    {
        if (gameROM.Length < 0x8000)
        {
            throw new CartridgeTooSmallException("Cartridge file has to be at least 8kb in size");
        }

        var hostBuilder = CreateHostBuilder(Array.Empty<string>());
        hostBuilder = hostBuilder.ConfigureServices((_, services) =>
            services.
            AddSingleton(s => frameSink).
            AddSingleton(s => Keypad).
            AddSingleton<MBC>(s => MakeCard(gameROM, fileName, s.GetRequiredService<Keypad>(), s.GetRequiredService<IFrameSink>())).
            AddSingleton<BootRom>(s => new(bootROM))
            );

        var host = hostBuilder.Start();

        InterruptRegisters = host.Services.GetRequiredService<InterruptRegisters>();

        APU = host.Services.GetRequiredService<APU>();
        PPU = host.Services.GetRequiredService<PPU>();

        Timers = host.Services.GetRequiredService<Timers>();

        Memory = host.Services.GetRequiredService<MMU>();

        CPU = host.Services.GetRequiredService<CPU>();

        DMA = host.Services.GetRequiredService<DMAControl>();

        CPU.OAMCorruption += PPU.OAM.Corrupt;

        //We have to replicate the state of the system post boot without running the bootrom
        if (bootROM == null)
        {
            //registers
            CPU.SetStateWithoutBootrom();

            //timers
            Timers.SetStateWithoutBootrom();


            //sound
            APU.SetStateWithoutBootrom();

            //graphics TODO: we can't really set up the graphics environment correctly
            //because we will have to also initialize the internal renderer state correctly
            PPU.SetStateWithoutBootrom();

            InterruptRegisters.SetStateWithoutBootrom();
        }

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA.DMA;
        CPU.Cycle += (o, e) => masterclock++;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA.DMA;
        CPU.Cycle += (o, e) => masterclock++;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA.DMA;
        CPU.Cycle += (o, e) => masterclock++;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA.DMA;
        CPU.Cycle += (o, e) => masterclock++;
    }

    private MBC MakeCard(byte[] gameROM, string fileName, Keypad Keypad, IFrameSink frameSink)
    {
        var Header = new CartHeader(gameROM);

        MBC Card;
        if (Header.HasBattery())
        {
            var mmf = Header.MakeMemoryMappedFile(fileName);
            Card = Header.HasClock() ? Header.MakeMBC(gameROM, mmf, () => masterclock) : Header.MakeMBC(gameROM, mmf);
        }
        else
        {
            Card = Header.MakeMBC(gameROM);
        }

        //Writing out the RTC too often would be very heavy. This writes it out once per frame.
        //
        if (Header.Type == CartType.MBC3_TIMER_RAM_BATTERY)
        {
            var SaveRTC = ((MBC3)Card).SaveRTC();

            void h(object? x, EventArgs y) => SaveRTC();
            frameSink.FramePushed += h;
        }

        if (Card is MBC5WithRumble rumble)
        {
            rumble.RumbleStateChange += Keypad.ToggleRumble;
        }

        return Card;
    }

    private bool disposedValue;

    public void Step() => CPU.Step();

    public (short left, short right) Sample() => APU.Sample();
    public (short left, short right) SampleChannel1() => APU.SampleChannel1();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Memory.Dispose();
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Core()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
