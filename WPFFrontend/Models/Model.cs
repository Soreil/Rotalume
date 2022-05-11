using emulator;

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

using WPFFrontend.Audio;

namespace WPFFrontend;

public class Model : INotifyPropertyChanged
{
    public Model(GameboyScreen gameboyScreen,
        Input input,
        Performance performance
        )
    {
        GameboyScreen = gameboyScreen;
        Input = input;
        Performance = performance;
    }
    public bool Paused
    {
        get;
        set;
    }

    private string rom = "";
    public string ROM
    {
        get => rom;
        set
        {
            if (value != rom)
            {
                rom = value;
                if (rom is not null)
                {
                    SpinUpNewGameboy();
                }
                OnPropertyChanged();
            }
        }
    }

    public int SelectedController
    {
        get => Input.SelectedController;
        set => Input.SelectedController = value;
    }
    public bool FpsLockEnabled
    {
        get;
        set;
    }
    public bool BootRomEnabled
    {
        get;
        set;
    }

    public GameboyScreen GameboyScreen { get; }
    public Input Input { get; }
    public Performance Performance { get; }
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

    public void SpinUpNewGameboy()
    {
        ShutdownGameboy();
        CancelGameboySource = new();

        var br = BootRomEnabled;

        GameThread = new Task(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Gaming";
            Gameboy(ROM, br);
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

    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}
