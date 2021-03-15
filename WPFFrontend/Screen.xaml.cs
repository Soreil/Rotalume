using emulator;

using J2i.Net.XInputWrapper;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFFrontend
{
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

            Dictionary<JoypadKey, bool> keyboard = new()
            {
                { JoypadKey.A, false },
                { JoypadKey.B, false },
                { JoypadKey.Select, false },
                { JoypadKey.Start, false },
                { JoypadKey.Right, false },
                { JoypadKey.Left, false },
                { JoypadKey.Up, false },
                { JoypadKey.Down, false }
            };

            Keyboard = keyboard;

            var Controllers = new List<IGameController> { new IGameControllerBridge(Controller1), new IGameControllerBridge(Controller2), new IGameControllerBridge(Controller3), new IGameControllerBridge(Controller4) };
            InputDevices = new(keyboard, Controllers);
        }

        private readonly Dictionary<JoypadKey, bool> Keyboard;
        private readonly InputDevices InputDevices;

        private void FPSDisplayEnable_Checked(object sender, RoutedEventArgs e) => throw new NotImplementedException();

        private volatile bool paused;
        private volatile bool CancelRequested;
        private void Gameboy(string path, bool bootromEnabled, bool fpsLimit)
        {
            var lockCb = new Action(Lock);
            var unlockCb = new Action(Unlock);

            byte[]? bootrom = bootromEnabled ? File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin") : null;

            void LockCB()
            {
                try
                {
                    Dispatcher.Invoke(lockCb,
            System.Windows.Threading.DispatcherPriority.Render, CancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {

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

            var gameboy = new Core(
                File.ReadAllBytes(path),
          bootrom,
          new Keypad(InputDevices),
          new FrameSink(LockCB, UnlockCB, Dispatcher.Invoke(() => bmp.BackBuffer), fpsLimit)
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

        private void Lock() => bmp.Lock();
        private void Unlock()
        {
            bmp.AddDirtyRect(new Int32Rect(0, 0, (int)bmp.Width, (int)bmp.Height));
            bmp.Unlock();
            AddFrameTimeToQueue();
            UpdateLabel();
        }

        private readonly WriteableBitmap bmp;

        private int currentFrame = 0;
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

        private int frameNumber = 0;
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

        CancellationTokenSource CancellationTokenSource = new();

        const int BitmapWidth = 160;
        const int BitmapHeight = 144;

        private void SpinUpNewGameboy(string fn)
        {
            ShutdownGameboy();
            CancellationTokenSource = new();

            var br = BootRomEnable.IsChecked;
            var fps = FPSLimitEnable.IsChecked;

            GameThread = new Task(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Name = "Gaming";
                Gameboy(fn, br, fps);
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
            if (Map(e.Key) is JoypadKey p)
            {
                Keyboard[p] = true;
            }
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
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(fs);
                }
            }
        }

        private static JoypadKey? Map(Key k) => k switch
        {
            Key.A => JoypadKey.B,
            Key.S => JoypadKey.A,
            Key.D => JoypadKey.Select,
            Key.F => JoypadKey.Start,
            Key.Right => JoypadKey.Right,
            Key.Left => JoypadKey.Left,
            Key.Up => JoypadKey.Up,
            Key.Down => JoypadKey.Down,
            _ => null
        };

        //There is a bouncing issue here which might be fixed by a delay
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Map(e.Key) is JoypadKey p)
            {
                Keyboard[p] = false;
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
            RadioButton li = (RadioButton)sender;
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
}