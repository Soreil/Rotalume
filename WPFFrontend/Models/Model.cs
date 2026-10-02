using CommunityToolkit.Mvvm.ComponentModel;

using emulator.glue;
using emulator.graphics;
using emulator.input;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

using System.IO;

using WPFFrontend.Audio;
using WPFFrontend.Platform;
using WPFFrontend.Services;

namespace WPFFrontend.Models;

public class Model(GameboyScreen gameboyScreen,
    Input input, FileService fileService, ILogger<FrameSink> logger,
    DispatcherQueue dispatcherQueue) : ObservableObject, IDisposable
{
    private volatile bool paused;
    private volatile bool fpsLockEnabled;

    public bool Paused { get => paused; set => paused = value; }

    public string? ROM
    {
        get => FileService.ROMPath;
        set
        {
            if (value != FileService.ROMPath)
            {
                FileService.ROMPath = value;
                if (FileService.ROMPath is not null)
                    SpinUpNewGameboy(FileService.ROMPath);
                OnPropertyChanged();
            }
        }
    }

    public bool FpsLockEnabled { get => fpsLockEnabled; set => fpsLockEnabled = value; }
    public bool BootRomEnabled;

    public GameboyScreen GameboyScreen { get; } = gameboyScreen;
    public Input Input { get; } = input;
    public FileService FileService { get; } = fileService;
    public ILogger<FrameSink> Logger { get; } = logger;
    public Player? Player { get; set; }

    private void Gameboy(string gameRomPath, bool bootromEnabled, CancellationToken cancellationToken)
    {
        var bootrom = bootromEnabled ? File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "bootrom", "DMG_ROM_BOOT.bin")) : null;

        bool FPSLimiterEnabled() => !cancellationToken.IsCancellationRequested && FpsLockEnabled;

        void FramePushed(object? o, EventArgs e)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (Paused)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _ = cancellationToken.WaitHandle.WaitOne(10);
            }

            if (o is FrameSink pixels)
            {
                var frame = pixels.GetFrame();
                var drawn = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                if (!dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            GameboyScreen.Fs_FramePushed(frame);
                        _ = drawn.TrySetResult();
                    }
                    catch (Exception ex)
                    {
                        _ = drawn.TrySetException(ex);
                    }
                }))
                {
                    throw new OperationCanceledException("UI dispatcher is shutting down.", cancellationToken);
                }
                //Bound the queue to one frame. Cancellation releases the worker even if the UI is closing.
                drawn.Task.Wait(cancellationToken);
            }
        }

        var fs = new FrameSink(FPSLimiterEnabled, Logger);
        fs.FramePushed += FramePushed;

        using var gameboy = new Core(
            File.ReadAllBytes(gameRomPath),
            bootrom,
            Path.GetFileNameWithoutExtension(gameRomPath),
            new Keypad(Input.Devices),
            fs
        );

        using var player = new Player(gameboy.Samples);
        player.Play();


        while (!cancellationToken.IsCancellationRequested)
        {
            gameboy.Step();
        }
        player.Stop();
    }

    private Task? GameTask;
    private CancellationTokenSource CancelGameboySource = new();
    private bool disposedValue;

    public void SpinUpNewGameboy(string path)
    {
        ShutdownGameboy();
        CancelGameboySource.Dispose();
        CancelGameboySource = new();
        var cancellationToken = CancelGameboySource.Token;

        var br = BootRomEnabled;

        GameTask = Task.Run(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Gaming";
            try
            {
                Gameboy(path, br, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                //Expected when stopping, replacing a ROM, or closing the window.
            }
        });
    }

    public void ShutdownGameboy()
    {
        if (GameTask is not null)
        {
            CancelGameboySource.Cancel();
            GameTask.Wait();
            GameTask = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ShutdownGameboy();
                CancelGameboySource.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
