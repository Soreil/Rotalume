using emulator;

using System.ComponentModel;
using System.IO;

namespace WPFFrontend;

internal class GameBoyViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly Performance performance;
    private readonly GameboyScreen display;
    private readonly Input input;
    public GameBoyViewModel(System.Windows.Controls.Image Display)
    {
        display = new GameboyScreen(Display);

        input = new();
        System.Windows.Application.Current.MainWindow.KeyUp += input.KeyUpHandler;
        System.Windows.Application.Current.MainWindow.KeyDown += input.KeyDownHandler;

        performance = new();

        display.FrameDrawn += Display_FrameDrawn;

        //We want to have one of the controllers selected since controllers are not zero indexed.
        SelectedController = 1;
    }

    private void Display_FrameDrawn(object? sender, EventArgs e)
    {
        PerformanceStatus = performance.Update();
    }

    public string PerformanceStatus
    {
        get => performance.Label;
        set
        {
            if (performance.Label != value)
            {
                performance.Label = value;
                OnPropertyChange(nameof(PerformanceStatus));
            }
        }
    }

    public int SelectedController
    {
        get => input.SelectedController;
        set
        {
            if (input.SelectedController != value)
            {
                input.SelectedController = value;
                OnPropertyChange(nameof(SelectedController));
            }
        }
    }

    public bool FpsLockEnabled
    {
        get;
        set;
    }
    public bool ShowPerformanceData
    {
        get;
        set;
    }

    public bool BootRomEnabled
    {
        get;
        set;
    }

    public bool Paused
    {
        get;
        set;
    }

    public void Pause() => Paused = true;

    public void SaveScreenShot() => display.SaveScreenShot();

    public volatile bool CancelRequested;
    private void Gameboy(string path, bool bootromEnabled)
    {
        var fpsCheckCb = new Func<bool>(() => FpsLockEnabled);

        byte[]? bootrom = bootromEnabled ? File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin") : null;

        bool FPSLimiterEnabled()
        {
            try
            {
                return System.Windows.Application.Current.Dispatcher.Invoke(fpsCheckCb,
                    System.Windows.Threading.DispatcherPriority.Render, CancellationTokenSource.Token);
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


            var draw = new Action(() => display.Fs_FramePushed(frame));

            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(draw,
                  System.Windows.Threading.DispatcherPriority.Render, CancellationTokenSource.Token);
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
      new Keypad(input.Devices),
      fs
      );

        while (!CancelRequested)
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
    }

    private Task? GameThread;

    private CancellationTokenSource CancellationTokenSource = new();
    private bool disposedValue;

    public void SpinUpNewGameboy(string fn)
    {
        ShutdownGameboy();
        CancellationTokenSource = new();

        var br = BootRomEnabled;

        GameThread = new Task(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Gaming";
            Gameboy(fn, br);
        });

        GameThread.Start();
    }
    public void ShutdownGameboy()
    {
        if (GameThread is not null)
        {
            CancelRequested = true;
            CancellationTokenSource.Cancel();
            GameThread.Wait();
            CancelRequested = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChange(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ShutdownGameboy();
                input.Dispose();
                // TODO: dispose managed state (managed objects)
            }

            disposedValue = true;

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~GameBoyViewModel()
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