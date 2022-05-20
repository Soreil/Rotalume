using emulator;

using Microsoft.Toolkit.Mvvm.ComponentModel;

using System.IO;

using WPFFrontend.Audio;
using WPFFrontend.Services;

namespace WPFFrontend;

public class Model : ObservableObject
{
    public Model(GameboyScreen gameboyScreen,
        Input input, FileService fileService)
    {
        GameboyScreen = gameboyScreen;
        Input = input;
        FileService = fileService;
    }
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
                {
                    SpinUpNewGameboy(FileService.ROMPath);
                }
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

    public GameboyScreen GameboyScreen { get; }
    public Input Input { get; }
    public FileService FileService { get; }
    public Player? Player { get; set; }

    private void Gameboy(string path, bool bootromEnabled)
    {
        var fpsCheckCb = new Func<bool>(() => FpsLockEnabled);

        byte[]? bootrom = bootromEnabled ? File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin") : null;

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


        var fs = new FrameSink(FPSLimiterEnabled);

        fs.FramePushed += FramePushed;

        using var gameboy = new Core(
            File.ReadAllBytes(path),
      bootrom,
      Path.GetFileNameWithoutExtension(path),
      new Keypad(Input.Devices),
      fs
      );

        Player player = new Player(gameboy.Samples);
        player.Play();

        while (!CancelGameboySource.IsCancellationRequested)
        {
            gameboy.Step();
            if (Paused)
            {
                while (Paused)
                {
                    Thread.Sleep(10);
                }
            }
        }
        player.Stop();
    }

    private Task? GameThread;


    private CancellationTokenSource CancelGameboySource = new();
    private bool disposedValue;

    public void SpinUpNewGameboy(string path)
    {
        ShutdownGameboy();
        CancelGameboySource = new();

        var br = BootRomEnabled;

        GameThread = new Task(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Gaming";
            Gameboy(path, br);
        });

        GameThread.Start();
    }
    public void ShutdownGameboy()
    {
        if (GameThread is not null)
        {
            CancelGameboySource.Cancel();
            GameThread.Wait();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ShutdownGameboy();
                Input.Dispose();
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
