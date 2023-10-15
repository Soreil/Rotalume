
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;

using System.IO;

using WPFFrontend.Audio;
using WPFFrontend.Platform;
using WPFFrontend.Services;
using emulator.graphics;
using emulator.glue;
using emulator.input;

namespace WPFFrontend.Models;

public class Model(GameboyScreen gameboyScreen,
    Input input, FileService fileService, ILogger<FrameSink> logger) : ObservableObject
{
    public bool Paused
    {
        get;
        set;
    }

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

    public bool FpsLockEnabled
    {
        get;
        set;
    }
    public bool BootRomEnabled;

    public GameboyScreen GameboyScreen { get; } = gameboyScreen;
    public Input Input { get; } = input;
    public FileService FileService { get; } = fileService;
    public ILogger<FrameSink> Logger { get; } = logger;
    public Player? Player { get; set; }

    private void Gameboy(string path, bool bootromEnabled)
    {
        var fpsCheckCb = new Func<bool>(() => FpsLockEnabled);

        var bootrom = bootromEnabled ? File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin") : null;

        bool FPSLimiterEnabled()
        {
            try
            {
                return System.Windows.Application.Current.Dispatcher.Invoke(fpsCheckCb,
                    System.Windows.Threading.DispatcherPriority.Render, CancelGameboySource.Token);
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        void FramePushed(object? o, EventArgs e)
        {
            if (o is null) return;
            var pixels = (FrameSink)o;
            if (pixels is null) return;
            var frame = pixels.GetFrame();


            var draw = new Action(() => GameboyScreen.Fs_FramePushed(frame));

            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(draw,
                  System.Windows.Threading.DispatcherPriority.Render, CancelGameboySource.Token);
            }
            catch (TaskCanceledException)
            {

            }
        }


        var fs = new FrameSink(FPSLimiterEnabled, Logger);

        fs.FramePushed += FramePushed;

        using var gameboy = new Core(
            File.ReadAllBytes(path),
      bootrom,
      Path.GetFileNameWithoutExtension(path),
      new Keypad(Input.Devices),
      fs
      );

        using var player = new Player(gameboy.Samples);
        player.Play();

        while (!CancelGameboySource.IsCancellationRequested)
        {
            gameboy.Step();
            if (Paused)
                while (Paused)
                    Thread.Sleep(10);
        }
        player.Stop();
    }

    private Task? GameTask;


    private CancellationTokenSource CancelGameboySource = new();
    private bool disposedValue;

    public void SpinUpNewGameboy(string path)
    {
        ShutdownGameboy();
        CancelGameboySource = new();

        var br = BootRomEnabled;

        GameTask = new Task(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Gaming";
            Gameboy(path, br);
        });

        GameTask.Start();
    }
    public void ShutdownGameboy()
    {
        if (GameTask is not null)
        {
            CancelGameboySource.Cancel();
            GameTask.Wait();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ShutdownGameboy();
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
