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

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate void UpdateImageCb();
        delegate void UpdateImagePixelsCb(byte[] data);
        delegate void UpdateLabelCb(float f);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Gameboy(string path)
        {
            var gameboy = new emulator.Core(emulator.Core.LoadBootROM(),
                System.IO.File.ReadAllBytes(path).ToList());

            var step = emulator.Core.Stepper(gameboy);

            Dispatcher.BeginInvoke(new UpdateImageCb(RunGameboy),
                System.Windows.Threading.DispatcherPriority.Render);

            void update(byte[] x)
            {
                Dispatcher.BeginInvoke(new UpdateImagePixelsCb(UpdatePixels),
    System.Windows.Threading.DispatcherPriority.Render,
     x);
            }

            gameboy.PPU.Writer = new emulator.FrameSink(update);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (true) step();
            var fps = gameboy.PPU.Writer.frameCount / (stopwatch.ElapsedMilliseconds / 1000f);
            stopwatch.Stop();
            Dispatcher.BeginInvoke(new UpdateLabelCb(UpdateLabel),
                System.Windows.Threading.DispatcherPriority.Render,
                 fps);
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

        private void LoadROM(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".gb", Filter = "ROM Files (.gb)|*.gb" };
            var result = ofd.ShowDialog();
            if (result == false) return;

            bmp = new WriteableBitmap(160, 144, 96, 96, PixelFormats.Gray8, null);
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Name = "Gaming";
                Gameboy(ofd.FileName);
            }).Start();
        }
    }
}
