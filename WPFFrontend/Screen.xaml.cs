using emulator;

using J2i.Net.XInputWrapper;

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFFrontend;

/// <summary>
/// Interaction logic for Screen.xaml
/// </summary>
public partial class Screen : Window
{
    public Screen()
    {
        InitializeComponent();

        bmp = new WriteableBitmap(BitmapWidth, BitmapHeight, 96, 96, PixelFormats.Gray8, null);
        var whitescreen = new byte[BitmapWidth * BitmapHeight];
        for (int i = 0; i < whitescreen.Length; i++) whitescreen[i] = 0xff;
        bmp.WritePixels(new Int32Rect(0, 0, BitmapWidth, BitmapHeight), whitescreen, BitmapWidth, 0);
        Display.Source = bmp;
        RenderOptions.SetBitmapScalingMode(Display, BitmapScalingMode.NearestNeighbor);

        FPSDisplayEnable.Checked += (x, y) => FPS.Visibility = Visibility.Visible;
        FPSDisplayEnable.Unchecked += (x, y) => FPS.Visibility = Visibility.Collapsed;

        XboxController.UpdateFrequency = 5;
        XboxController.StartPolling();

        var Controller1 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(0));
        var Controller2 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(1));
        var Controller3 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(2));
        var Controller4 = new XboxControllerWithInterruptHandler(XboxController.RetrieveController(3));

        Dictionary<Key, JoypadKey> mappedKeys = new()
        {
            { Key.X, JoypadKey.A },
            { Key.LeftShift, JoypadKey.Select },
            { Key.RightShift, JoypadKey.Select },
            { Key.Z, JoypadKey.B },
            { Key.Down, JoypadKey.Down },
            { Key.Left, JoypadKey.Left },
            { Key.Right, JoypadKey.Right },
            { Key.Up, JoypadKey.Up },
            { Key.Enter, JoypadKey.Start }
        };

        var UnconnectedKeyboard = new KeyBoardWithInterruptHandler(mappedKeys);

        KeyDown += new KeyEventHandler(UnconnectedKeyboard.Down);
        KeyUp += new KeyEventHandler(UnconnectedKeyboard.Up);

        var kb = new IGameControllerKeyboardBridge(UnconnectedKeyboard);

        var Controllers = new List<IGameController> { new IGameControllerXboxBridge(Controller1), new IGameControllerXboxBridge(Controller2), new IGameControllerXboxBridge(Controller3), new IGameControllerXboxBridge(Controller4) };
        InputDevices = new(kb, Controllers);
        Default.IsChecked = true;
    }

    private readonly InputDevices InputDevices;

    private volatile bool paused;
    private volatile bool CancelRequested;
    private void Gameboy(string path, bool bootromEnabled)
    {
        var lockCb = new Func<IntPtr>(Lock);
        var fpsCheckCb = new Func<bool>(FpsLockEnabled);
        var unlockCb = new Action(Unlock);

        byte[]? bootrom = bootromEnabled ? File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin") : null;

        IntPtr LockCB()
        {
            if (CancellationTokenSource.IsCancellationRequested) return IntPtr.Zero;
            try
            {
                return Dispatcher.Invoke(lockCb,
        System.Windows.Threading.DispatcherPriority.Render, CancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                return IntPtr.Zero;
            }
        }

        void UnlockCB()
        {
            try
            {
                Dispatcher.Invoke(unlockCb,
        System.Windows.Threading.DispatcherPriority.Render, CancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {

            }
        }

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

        using var gameboy = new Core(
            File.ReadAllBytes(path),
      bootrom,
      Path.GetFileNameWithoutExtension(path),
      new Keypad(InputDevices),
      new FrameSink(LockCB, UnlockCB, FPSLimiterEnabled)
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
    private IntPtr Lock()
    {
        bmp.Lock();
        return bmp.BackBuffer;
    }
    private void Unlock()
    {
        bmp.AddDirtyRect(new Int32Rect(0, 0, (int)bmp.Width, (int)bmp.Height));
        bmp.Unlock();
        AddFrameTimeToQueue();
        UpdateLabel();
    }

    private readonly WriteableBitmap bmp;

    private int currentFrame;
    private void AddFrameTimeToQueue()
    {
        FrameTimes[currentFrame++] = DateTime.Now;
        if (currentFrame == 16)
        {
            FrameTime = Delta(15, 14).TotalMilliseconds;
            AverageFPS();
            currentFrame = 0;
        }
    }

    private readonly DateTime[] FrameTimes = new DateTime[16];

    private double FrameTime;
    private double GameboyFPS;
    private void AverageFPS()
    {
        TimeSpan deltas = TimeSpan.Zero;
        for (int i = 1; i < FrameTimes.Length; i++)
        {
            deltas += Delta(i, i - 1);
        }

        GameboyFPS = TimeSpan.FromSeconds(1) / (deltas / (FrameTimes.Length - 1));
    }
    private TimeSpan Delta(int i, int j) => FrameTimes[i] - FrameTimes[j];

    private int frameNumber;
    private void UpdateLabel() => FPS.Content = string.Format("Frame:{0}\t FrameTime:{1:N2}\t FPS:{2:N2}",
            frameNumber++,
            FrameTime,
            GameboyFPS);

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
    private const int BitmapWidth = 160;
    private const int BitmapHeight = 144;

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
            if (bmp is not null)
            {
                string fileName = string.Format(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
      @"\Screenshot" + "_" +
      DateTime.Now.ToString("(dd_MMMM_hh_mm_ss_tt)") + ".png");
                using FileStream fs = new(fileName, FileMode.Create);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fs);
            }
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
        InputDevices.SelectedController = li.Content switch
        {
            "1" => 1,
            "2" => 2,
            "3" => 3,
            "4" => 4,
            _ => 0,
        };
    }
}
