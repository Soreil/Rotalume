using emulator;

using NAudio.Wave;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void UpdateImageCb();

        private delegate byte UpdateJoypadCb(byte b);

        private delegate void UpdateLabelCb();
        public MainWindow()
        {
            InitializeComponent();

            Pressed = new(2, 8);
            Pressed.TryAdd(Key.A, false);
            Pressed.TryAdd(Key.S, false);
            Pressed.TryAdd(Key.D, false);
            Pressed.TryAdd(Key.F, false);
            Pressed.TryAdd(Key.Right, false);
            Pressed.TryAdd(Key.Left, false);
            Pressed.TryAdd(Key.Up, false);
            Pressed.TryAdd(Key.Down, false);
        }

        private volatile bool paused = false;
        private volatile bool CancelRequested = false;
        private void Gameboy(string path, bool bootromEnabled, bool fpsLimit)
        {
            var bmpCb = new UpdateImageCb(SetBitmapBacking);
            var lockCb = new UpdateImageCb(Lock);
            var unlockCb = new UpdateImageCb(Unlock);

            bool keyBoardInterruptFired()
            {
                var res = keyboardInterruptReady;
                keyboardInterruptReady = false;
                return res;
            }

            byte[]? bootrom = bootromEnabled ? Core.LoadBootROM() : null;

            Dispatcher.Invoke(bmpCb,
        System.Windows.Threading.DispatcherPriority.Render);

            void LockCB() => Dispatcher.Invoke(lockCb,
        System.Windows.Threading.DispatcherPriority.Render);

            void UnlockCB() => Dispatcher.Invoke(unlockCb,
        System.Windows.Threading.DispatcherPriority.Render);

            var gameboy = new Core(
                File.ReadAllBytes(path),
          bootrom,
          Pressed,
          keyBoardInterruptFired,
          new FrameSink(LockCB, UnlockCB, Dispatcher.Invoke(() => bmp!.BackBuffer), fpsLimit)
          );

            new Task(() =>
            {
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
            }).Start();
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

        private int frameNumber = 0;
        private readonly DateTime[] FrameTimes = new DateTime[16];

        private double AverageFPS()
        {
            TimeSpan deltas = TimeSpan.Zero;
            for (int i = 1; i < FrameTimes.Length; i++)
            {
                deltas += Delta(i, i - 1);
            }

            return TimeSpan.FromSeconds(1) / (deltas / (FrameTimes.Length - 1));
        }
        private TimeSpan Delta(int i, int j) => FrameTimes[i] - FrameTimes[j];

        private void UpdateLabel() => FPS.Content = string.Format("Frame:{0} FrameTime:{1} FPS:{2}",
                frameNumber,
                Delta(15, 14).TotalMilliseconds,
                AverageFPS());

        private void AddFrameTimeToQueue()
        {
            frameNumber++;
            var time = DateTime.Now;
            for (int i = 1; i < FrameTimes.Length; i++)
            {
                FrameTimes[i - 1] = FrameTimes[i];
            }

            FrameTimes[^1] = time;
        }

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

        private void SpinUpNewGameboy(string fn)
        {
            bool br = (bool)bootromCheckbox.IsChecked!;
            bool fps = (bool)fpsLimit.IsChecked!;
            if (GameThread is not null)
            {
                CancelRequested = true;
                GameThread.Wait();
                CancelRequested = false;
            }

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
        private readonly ConcurrentDictionary<Key, bool> Pressed;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Pressed.ContainsKey(e.Key))
            {
                Pressed[e.Key] = true;
                if (GameThread is not null)
                {
                    keyboardInterruptReady = true;
                }
            }
            if (e.Key == Key.P)
            {
                paused = !paused;
            }
        }

        //There is a bouncing issue here which might be fixed by a delay
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Pressed.ContainsKey(e.Key))
            {
                Pressed[e.Key] = false;
            }
        }
    }
}
