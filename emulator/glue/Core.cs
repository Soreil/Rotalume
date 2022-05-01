﻿using emulator.sound;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace emulator;

public class Core : IDisposable
{
    public readonly CPU CPU;

    internal DMAControl DMA { get; }

    private readonly APU APU;
    private readonly PPU PPU;
    public readonly MMU Memory;
    private readonly Timers Timers;
    private readonly InterruptRegisters InterruptRegisters;
    private readonly MasterClock MasterClock;

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
        AddSingleton<DMAControl>().
        AddSingleton<MasterClock>()
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
            AddSingleton(s => new GameROM(gameROM, fileName)).
            AddSingleton(s => CoreHelpers.MakeCard(s.GetRequiredService<GameROM>(), s.GetRequiredService<Keypad>(), s.GetRequiredService<IFrameSink>(), s.GetRequiredService<MasterClock>())).
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



        MasterClock = host.Services.GetRequiredService<MasterClock>();

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
        CPU.Cycle += MasterClock.Tick;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA.DMA;
        CPU.Cycle += MasterClock.Tick;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA.DMA;
        CPU.Cycle += MasterClock.Tick;

        CPU.Cycle += Timers.Tick;
        CPU.Cycle += PPU.Tick;
        CPU.Cycle += APU.Tick;
        CPU.Cycle += DMA.DMA;
        CPU.Cycle += MasterClock.Tick;
    }

    public void Step() => CPU.Step();

    public (short left, short right) Sample() => APU.Sample();
    public (short left, short right) SampleChannel1() => APU.SampleChannel1();

    public long Time() => MasterClock.Now();

    private bool disposedValue;
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
