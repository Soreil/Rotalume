using emulator;

using Hardware;

using J2i.Net.XInputWrapper;

using NAudio.Wave;

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Pressed = new(2, 8);
            _ = Pressed.TryAdd(JoypadKey.A, false);
            _ = Pressed.TryAdd(JoypadKey.B, false);
            _ = Pressed.TryAdd(JoypadKey.Select, false);
            _ = Pressed.TryAdd(JoypadKey.Start, false);
            _ = Pressed.TryAdd(JoypadKey.Right, false);
            _ = Pressed.TryAdd(JoypadKey.Left, false);
            _ = Pressed.TryAdd(JoypadKey.Up, false);
            _ = Pressed.TryAdd(JoypadKey.Down, false);

            XboxController.UpdateFrequency = 5;
            XboxController.StartPolling();

            Controller = XboxController.RetrieveController(0);

            Controller.StateChanged += SelectedController_StateChanged;

            PropertyChanged += MainWindow_PropertyChanged;
        }

        private void MainWindow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Buttons") return;

            if ((!Pressed[JoypadKey.Right] && Controller.IsDPadRightPressed) ||
                (!Pressed[JoypadKey.Left] && Controller.IsDPadLeftPressed) ||
                (!Pressed[JoypadKey.Up] && Controller.IsDPadUpPressed) ||
                (!Pressed[JoypadKey.Down] && Controller.IsDPadDownPressed) ||
                (!Pressed[JoypadKey.B] && Controller.IsBPressed) ||
                (!Pressed[JoypadKey.A] && Controller.IsAPressed) ||
                (!Pressed[JoypadKey.Select] && Controller.IsBackPressed) ||
                (!Pressed[JoypadKey.Start] && Controller.IsStartPressed))
            {
                keyboardInterruptReady = true;
            }

            Pressed[JoypadKey.Right] = Controller.IsDPadRightPressed;
            Pressed[JoypadKey.Left] = Controller.IsDPadLeftPressed;
            Pressed[JoypadKey.Up] = Controller.IsDPadUpPressed;
            Pressed[JoypadKey.Down] = Controller.IsDPadDownPressed;
            Pressed[JoypadKey.B] = Controller.IsBPressed;
            Pressed[JoypadKey.A] = Controller.IsAPressed;
            Pressed[JoypadKey.Select] = Controller.IsBackPressed;
            Pressed[JoypadKey.Start] = Controller.IsStartPressed;
        }

        private readonly XboxController Controller;
        void SelectedController_StateChanged(object? sender, XboxControllerStateChangedEventArgs e) => OnPropertyChanged("Buttons");
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                Action a = () => PropertyChanged(this, new PropertyChangedEventArgs(name));
                _ = Dispatcher.BeginInvoke(a);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private volatile bool paused;
        private volatile bool CancelRequested;
        private void Gameboy(string path, bool bootromEnabled, bool fpsLimit)
        {
            var bmpCb = new Action(SetBitmapBacking);
            var lockCb = new Action(Lock);
            var unlockCb = new Action(Unlock);

            bool keyBoardInterruptFired()
            {
                var res = keyboardInterruptReady;
                keyboardInterruptReady = false;
                return res;
            }

            byte[]? bootrom = bootromEnabled ? File.ReadAllBytes(@"..\..\..\..\emulator\bootrom\DMG_ROM_BOOT.bin") : null;

            Dispatcher.Invoke(bmpCb,
        System.Windows.Threading.DispatcherPriority.Render);

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
          Pressed,
          keyBoardInterruptFired,
          new FrameSink(LockCB, UnlockCB, Dispatcher.Invoke(() => bmp!.BackBuffer), fpsLimit)
          );

            bool useSound = false;
            if (useSound)
            {

                //Using a high buffer count makes it so the audio doesn't stutter, 5 seems
                //to be just about enough to prevent stutter. Gusboy uses 50 so I'll go with that.
                using var wo = new WaveOutEvent
                {
                    DesiredLatency = 100,
                    NumberOfBuffers = 50,
                };
                wo.Init(gameboy);
                wo.Play();

                //We can only stop at a ms granularity this way, if we don't
                //have some check interval we will consume full CPU resources
                //Maybe some smarter waiting condition would help?
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    if (CancelRequested)
                    {
                        wo.Stop();
                        break;
                    }

                    if (paused)
                    {
                        wo.Pause();
                        while (paused)
                        {
                            Thread.Sleep(10);
                        }
                        wo.Play();
                    }
                }
            }
            else
            {
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
        }

        private void Lock() => bmp!.Lock();
        private void Unlock()
        {
            bmp!.AddDirtyRect(new Int32Rect(0, 0, (int)bmp.Width, (int)bmp.Height));
            bmp!.Unlock();
            AddFrameTimeToQueue();
            UpdateLabel();
        }

        private WriteableBitmap? bmp;

        private void SetBitmapBacking()
        {
            Display.Source = bmp;
            RenderOptions.SetBitmapScalingMode(Display, BitmapScalingMode.NearestNeighbor);
        }

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

        private void SpinUpNewGameboy(string fn)
        {
            bool br = (bool)bootromCheckbox.IsChecked!;
            bool fps = (bool)fpsLimit.IsChecked!;
            if (GameThread is not null)
            {
                CancelRequested = true;
                CancellationTokenSource.Cancel();
                GameThread.Wait();
                CancelRequested = false;
            }
            CancellationTokenSource = new();

            bmp = new WriteableBitmap(160, 144, 96, 96, PixelFormats.Gray8, null);

            GameThread = new Task(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Name = "Gaming";
                Gameboy(fn, br, fps);
            });

            GameThread.Start();
        }

        private volatile bool keyboardInterruptReady = false;
        private readonly ConcurrentDictionary<JoypadKey, bool> Pressed;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Abstraction.Map(e.Key) is JoypadKey p)
            {
                Pressed[p] = true;
                if (GameThread is not null)
                {
                    keyboardInterruptReady = true;
                }
            }
            else if (e.Key == Key.P)
            {
                paused = !paused;
            }
        }

        //There is a bouncing issue here which might be fixed by a delay
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Abstraction.Map(e.Key) is JoypadKey p)
            {
                Pressed[p] = false;
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
    }
}
