using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFFrontend;

public class GameboyScreen
{
    const int BitmapWidth = 160;
    const int BitmapHeight = 144;

    public readonly WriteableBitmap buffer;

    public GameboyScreen(Image image)
    {
        buffer = new WriteableBitmap(BitmapWidth, BitmapHeight, 96, 96, PixelFormats.Gray8, null);
        var whitescreen = new byte[BitmapWidth * BitmapHeight];
        for (int i = 0; i < whitescreen.Length; i++) whitescreen[i] = 0xff;
        buffer.WritePixels(new Int32Rect(0, 0, BitmapWidth, BitmapHeight), whitescreen, BitmapWidth, 0);
        image.Source = buffer;
    }

    internal void SaveScreenShot()
    {
        string fileName = string.Format(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
@"\Screenshot" + "_" +
DateTime.Now.ToString("(dd_MMMM_hh_mm_ss_tt)") + ".png");
        using FileStream fs = new(fileName, FileMode.Create);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(buffer));
        encoder.Save(fs);
    }

    public void Fs_FramePushed(object? sender, EventArgs e)
    {
        if (sender is null) throw new Exception();

        var frameSink = (emulator.FrameSink)sender;

        if (frameSink is null) throw new Exception();

        var pixels = frameSink.GetFrame();

        WriteOutputFrame(pixels);

    }

    private void WriteOutputFrame(byte[] pixels)
    {
        try
        {
            // Reserve the back buffer for updates.
            buffer.Lock();

            unsafe
            {
                // Get a pointer to the back buffer.
                IntPtr pBackBuffer = buffer.BackBuffer;

                Marshal.Copy(pixels, 0, pBackBuffer, BitmapWidth * BitmapHeight);
            }

            // Specify the area of the bitmap that changed.
            buffer.AddDirtyRect(new Int32Rect(0, 0, BitmapWidth, BitmapHeight));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            buffer.Unlock();
        }
    }
}
