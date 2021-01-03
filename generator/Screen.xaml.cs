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

        byte Flags = 0;
        private void UpdateJoypadBackingRegister(byte flags)
        {
            Flags = flags;
        }

        private byte GetJoypadBackingRegister()
        {
            UpdateJoypadPresses();
            return joypad;
        }

        Action TriggerKeyboardInterrupt = () => { };
        volatile byte joypad = 0x0F;

        Dictionary<Key, bool> Pressed = new Dictionary<Key, bool> {
            { Key.A,    false },
            { Key.S,    false },
            { Key.D,    false },
            { Key.F,    false },
            { Key.L,    false },
            { Key.H,    false },
            { Key.J,    false },
            { Key.K,    false },
        };

        private void UpdateJoypadPresses()
        {
            var selectButtons = Flags.GetBit(5);
            var selectArrows = Flags.GetBit(4);

            joypad = 0x0f;

            if (selectArrows && selectButtons)
            {
                if (Pressed[Key.H] && Pressed[Key.A]) joypad = joypad.SetBit(0, false);
                if (Pressed[Key.J] && Pressed[Key.S]) joypad = joypad.SetBit(1, false);
                if (Pressed[Key.K] && Pressed[Key.D]) joypad = joypad.SetBit(2, false);
                if (Pressed[Key.L] && Pressed[Key.F]) joypad = joypad.SetBit(3, false);
            }
            else if (selectArrows)
            {
                if (Pressed[Key.H]) joypad = joypad.SetBit(0, false);
                if (Pressed[Key.J]) joypad = joypad.SetBit(1, false);
                if (Pressed[Key.K]) joypad = joypad.SetBit(2, false);
                if (Pressed[Key.L]) joypad = joypad.SetBit(3, false);
            }
            else if (selectButtons)
            {
                if (Pressed[Key.A]) joypad = joypad.SetBit(0, false);
                if (Pressed[Key.S]) joypad = joypad.SetBit(1, false);
                if (Pressed[Key.D]) joypad = joypad.SetBit(2, false);
                if (Pressed[Key.F]) joypad = joypad.SetBit(3, false);
            }


            if ((joypad & 0xF) != 0xf) TriggerKeyboardInterrupt();

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Pressed[e.Key] = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Pressed.ContainsKey(e.Key))
            {
                Pressed[e.Key] = false;
            }
        }
    }
}
