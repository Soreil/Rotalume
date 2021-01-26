using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using emulator;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate void UpdateImageCb();
        delegate byte UpdateJoypadCb(byte b);
        delegate void UpdateImagePixelsCb(byte[] data);
        delegate void UpdateLabelCb();
        public MainWindow()
        {
            InitializeComponent();

        }

        volatile bool paused = false;
        volatile bool CancelRequested = false;
        private void Gameboy(string path, bool bootromEnabled)
        {
            byte updateJoyPad(byte x)
            {
                var inv = Dispatcher.BeginInvoke(new UpdateJoypadCb(UpdateJoypadPresses),
                    System.Windows.Threading.DispatcherPriority.Render,
                    x);
                inv.Wait(new TimeSpan(10000));
                //TODO: Find a more clean way to handle program exit mid Invoke
                if (inv.Status != System.Windows.Threading.DispatcherOperationStatus.Completed) return 0x0f;
                return (byte)inv.Result;
            }

            bool keyBoardInterruptFired()
            {
                var res = keyboardInterruptReady;
                keyboardInterruptReady = false;
                return res;
            }

            byte[] bootrom = bootromEnabled ? Core.LoadBootROM() : null;

            var gameboy = new Core(System.IO.File.ReadAllBytes(path),
                bootrom, updateJoyPad, keyBoardInterruptFired);

            Dispatcher.BeginInvoke(new UpdateImageCb(RunGameboy),
                System.Windows.Threading.DispatcherPriority.Render);

            void update(byte[] x)
            {
                Dispatcher.BeginInvoke(new UpdateImagePixelsCb(UpdatePixels),
    System.Windows.Threading.DispatcherPriority.Render,
     x);
                Dispatcher.BeginInvoke(new UpdateLabelCb(UpdateLabel),
                    System.Windows.Threading.DispatcherPriority.Render);
            }

            gameboy.PPU.Writer = new emulator.FrameSink(update);

            StartTime = DateTime.Now;

            while (!CancelRequested)
            {
                if (!paused)
                    gameboy.Step();
                else Thread.Sleep(10);
            }
        }

        WriteableBitmap bmp;

        private void RunGameboy()
        {
            Display.Source = bmp;
            RenderOptions.SetBitmapScalingMode(Display, BitmapScalingMode.NearestNeighbor);
        }

        int frameNumber = 0;

        DateTime StartTime = new();
        private void UpdateLabel()
        {
            frameNumber++;
            var frameTime = (DateTime.Now - StartTime) / frameNumber;
            var fps = 1000 / frameTime.TotalMilliseconds;
            FPS.Content = string.Format("Frame:{0} FrameTime:{1} FPS:{2}", frameNumber, frameTime.TotalMilliseconds, fps);
        }

        private void UpdatePixels(byte[] data)
        {
            bmp.WritePixels(new Int32Rect(0, 0, (int)bmp.Width, (int)bmp.Height)
                , data, bmp.BackBufferStride, 0);
        }

        Task GameThread;
        private void LoadROM(object sender, DragEventArgs e)
        {
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                if (filenames.Count() != 1) return;

                SpinUpNewGameboy(filenames[0]);
            }

        }
        private void LoadROMPopUp(object sender, RoutedEventArgs e)
        {

            var ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".gb", Filter = "ROM Files (.gb)|*.gb" };
            var result = ofd.ShowDialog();
            if (result == false) return;

            SpinUpNewGameboy(ofd.FileName);
        }

        private void SpinUpNewGameboy(string fn)
        {
            bool br = (bool)bootromCheckbox.IsChecked;
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
                Gameboy(fn, br);
            });

            GameThread.Start();
        }

        volatile bool keyboardInterruptReady = false;
        readonly Dictionary<Key, bool> Pressed = new Dictionary<Key, bool> {
            {Key.A,    false},
            {Key.S,    false},
            {Key.D,    false},
            {Key.F,    false},
            {Key.Right,false},
            {Key.Left, false},
            {Key.Up,   false},
            {Key.Down, false},
        };

        private byte UpdateJoypadPresses(byte Flags)
        {
            var selectButtons = !Flags.GetBit(5);
            var selectArrows = !Flags.GetBit(4);

            byte joypad = 0xf;
            if (!selectButtons && !selectArrows) return (byte)((joypad & 0xf) | 0xc0);

            if (selectArrows)
            {
                if (Pressed[Key.Right]) joypad = joypad.SetBit(0, false);
                if (Pressed[Key.Left]) joypad = joypad.SetBit(1, false);
                if (Pressed[Key.Up]) joypad = joypad.SetBit(2, false);
                if (Pressed[Key.Down]) joypad = joypad.SetBit(3, false);
            }
            if (selectButtons)
            {
                if (Pressed[Key.A]) joypad = joypad.SetBit(0, false);
                if (Pressed[Key.S]) joypad = joypad.SetBit(1, false);
                if (Pressed[Key.D]) joypad = joypad.SetBit(2, false);
                if (Pressed[Key.F]) joypad = joypad.SetBit(3, false);
            }

            return (byte)((joypad & 0xf) | 0xc0);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Pressed.ContainsKey(e.Key) && Pressed[e.Key] == false)
            {
                Pressed[e.Key] = true;
                if (GameThread is not null)
                    keyboardInterruptReady = true;
            }
            if (e.Key == Key.P) paused = !paused;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Pressed.ContainsKey(e.Key))
                Pressed[e.Key] = false;
        }
    }
}
