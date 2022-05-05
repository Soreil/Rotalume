using emulator;

using J2i.Net.XInputWrapper;

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFFrontend;

/// <summary>
/// Interaction logic for Screen.xaml
/// </summary>
public partial class Screen : Window
{
    public Screen()
    {
        InitializeComponent();

        display = new GameboyScreen(Display);
        RenderOptions.SetBitmapScalingMode(Display, BitmapScalingMode.NearestNeighbor);

        FPSDisplayEnable.Checked += (x, y) => FPS.Visibility = Visibility.Visible;
        FPSDisplayEnable.Unchecked += (x, y) => FPS.Visibility = Visibility.Collapsed;

        input = new();
        Default.IsChecked = true;

        performance = new();
    }

    private readonly Performance performance;
    private readonly GameboyScreen display;
    private readonly Input input;

    private volatile bool paused;
    private volatile bool CancelRequested;
    private void Gameboy(string path, bool bootromEnabled)
    {
        var fpsCheckCb = new Func<bool>(FpsLockEnabled);

        byte[]? bootrom = bootromEnabled ? File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin") : null;

        bool FPSLimiterEnabled()
        {
            try
            {
                return Dispatcher.Invoke(fpsCheckCb,
                    System.Windows.Threading.DispatcherPriority.Render, CancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }


        var framePushedCb = new Action<object?, EventArgs>(display.Fs_FramePushed);

        void FramePushed(object? o, EventArgs e)
        {
            _ = Dispatcher.Invoke(framePushedCb,
               System.Windows.Threading.DispatcherPriority.Render, o, e);
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
            if (paused)
            {
                while (paused)
                {
                    Thread.Sleep(10);
                }
            }
        }
    }

    private bool FpsLockEnabled() => FPSLimitEnable.IsChecked;


    private Task? GameThread;
    private void LoadROM(object sender, DragEventArgs e)
    {
        // If the DataObject contains string data, extract it.
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[]? fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            //Check that the file isn't a folder
            if (fileNames is not null && fileNames.Length == 1 && File.Exists(fileNames[0]))
            {
                SpinUpNewGameboy(fileNames[0]);
            }
        }

    }
    private void LoadROMPopUp(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".gb", Filter = "ROM Files (*.gb;*.gbc)|*.gb;*.gbc" };
        var result = ofd.ShowDialog();
        if (result == false)
        {
            return;
        }

        SpinUpNewGameboy(ofd.FileName);
    }

    private CancellationTokenSource CancellationTokenSource = new();
    private void SpinUpNewGameboy(string fn)
    {
        ShutdownGameboy();
        CancellationTokenSource = new();

        var br = BootRomEnable.IsChecked;

        GameThread = new Task(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Gaming";
            Gameboy(fn, br);
        });

        GameThread.Start();
    }

    private void CloseGameboyRequest(object sender, RoutedEventArgs e) => ShutdownGameboy();

    private void ShutdownGameboy()
    {
        if (GameThread is not null)
        {
            CancelRequested = true;
            CancellationTokenSource.Cancel();
            GameThread.Wait();
            CancelRequested = false;
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.P)
        {
            paused = !paused;
        }
        if (e.Key == Key.S)
        {
            display.SaveScreenShot();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        CancelRequested = true;

        //This should be triggered by the thread shutting down but it gets stuck calling back in to this thread
        //via the dispatcher before it even has a chance to acknowledge cancelrequested often.
        //If we don't stop it from polling the xboxcontroller will keep polling in the background forever.
        XboxController.StopPolling();
        base.OnClosing(e);
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is null) return;
        var li = (RadioButton)sender;
        input.Devices.SelectedController = li.Content switch
        {
            "1" => 1,
            "2" => 2,
            "3" => 3,
            "4" => 4,
            _ => 0,
        };
    }
}
