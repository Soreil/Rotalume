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
        delegate void UpdateJoypadCb(byte b);
        delegate byte GetJoypadCb();
        delegate void UpdateImagePixelsCb(byte[] data);
        delegate void UpdateLabelCb(float f);
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Gameboy(string path)
        {
            void updateJoyPadSelection(byte x)
            {
                Dispatcher.BeginInvoke(new UpdateJoypadCb(UpdateJoypadBackingRegister),
    System.Windows.Threading.DispatcherPriority.Render,
     x);
            }
            byte updateJoyPad()
            {
                return (byte)Dispatcher.Invoke(new GetJoypadCb(GetJoypadBackingRegister));
            }

            var gameboy = new emulator.Core(emulator.Core.LoadBootROM(),
                System.IO.File.ReadAllBytes(path).ToList(), updateJoyPadSelection, updateJoyPad);

            Dispatcher.BeginInvoke(new UpdateImageCb(RunGameboy),
                System.Windows.Threading.DispatcherPriority.Render);

            void update(byte[] x)
            {
                Dispatcher.BeginInvoke(new UpdateImagePixelsCb(UpdatePixels),
    System.Windows.Threading.DispatcherPriority.Render,
     x);
            }

            gameboy.PPU.Writer = new emulator.FrameSink(update);

            //var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (true) gameboy.Step();
            //var fps = gameboy.PPU.Writer.frameCount / (stopwatch.ElapsedMilliseconds / 1000f);
            //stopwatch.Stop();
            //Dispatcher.BeginInvoke(new UpdateLabelCb(UpdateLabel),
            //    System.Windows.Threading.DispatcherPriority.Render,
            //     fps);
        }

        WriteableBitmap bmp;

        private void RunGameboy()
        {
            Display.Source = bmp;
        }

        private void UpdateLabel(float f) => FPS.Content = string.Format("FPS:{0}", f);

        private void UpdatePixels(byte[] data)
        {
            bmp.WritePixels(new Int32Rect(0, 0, (int)bmp.Width, (int)bmp.Height)
                , data, bmp.BackBufferStride, 0);
        }

        Task GameThread;
        private void LoadROM(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".gb", Filter = "ROM Files (.gb)|*.gb" };
            var result = ofd.ShowDialog();
            if (result == false) return;

            bmp = new WriteableBitmap(160, 144, 96, 96, PixelFormats.Gray8, null);
            GameThread = new Task(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Name = "Gaming";
                Gameboy(ofd.FileName);
            });
            GameThread.Start();
        }


        private void UpdateJoypadBackingRegister(byte flags)
        {
            joypad = (byte)((joypad & 0xf) | (flags & 0x30));
        }

        private byte GetJoypadBackingRegister()
        {
            UpdateJoypadPresses();
            return joypad;
        }

        Action TriggerKeyboardInterrupt = () => { };
        volatile byte joypad = 0x3F;

        Dictionary<Key, DateTime> LastSeen = new Dictionary<Key, DateTime> {
            { Key.A,    DateTime.MinValue },
            { Key.S,    DateTime.MinValue },
            { Key.D,    DateTime.MinValue },
            { Key.F,    DateTime.MinValue },
            { Key.L,DateTime.MinValue },
            { Key.H, DateTime.MinValue },
            { Key.J,   DateTime.MinValue },
            { Key.K, DateTime.MinValue },
        };

        private void UpdateJoypadPresses()
        {
            var selectButtons = joypad.GetBit(4);
            var selectArrows = joypad.GetBit(5);

            joypad = (byte)(joypad & 0x30);
            joypad |= 0xf;
            const int keyTimeOut = 100;
            DateTime now = DateTime.Now;

            if (selectArrows && !selectButtons)
            {
                var Adiff = now - LastSeen[Key.L];
                if (Adiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(0, false);
                var Bdiff = now - LastSeen[Key.H];
                if (Bdiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(1, false);
                var Selectdiff = now - LastSeen[Key.J];
                if (Selectdiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(2, false);
                var Startdiff = now - LastSeen[Key.K];
                if (Startdiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(3, false);
            }
            if (selectButtons && !selectArrows)
            {
                var Adiff = now - LastSeen[Key.A];
                if (Adiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(0, false);
                var Bdiff = now - LastSeen[Key.S];
                if (Bdiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(1, false);
                var Selectdiff = now - LastSeen[Key.D];
                if (Selectdiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(2, false);
                var Startdiff = now - LastSeen[Key.F];
                if (Startdiff.TotalMilliseconds < keyTimeOut)
                    joypad = joypad.SetBit(3, false);
            }

            if ((joypad & 0xF) != 0xf) TriggerKeyboardInterrupt();

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (LastSeen.ContainsKey(e.Key))
                LastSeen[e.Key] = DateTime.Now;
        }
    }

}
