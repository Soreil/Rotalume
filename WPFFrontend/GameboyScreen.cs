using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFFrontend;

public class GameboyScreen
{
    private const int BitmapWidth = 160;
    private const int BitmapHeight = 144;

    public readonly WriteableBitmap buffer;
    private byte[]? previousFrame;
    public GameboyScreen(Image image)
    {
        buffer = new WriteableBitmap(BitmapWidth, BitmapHeight, 96, 96, PixelFormats.Gray8, null);
        var whitescreen = new byte[BitmapWidth * BitmapHeight];
        for (int i = 0; i < whitescreen.Length; i++) whitescreen[i] = 0xff;
        buffer.WritePixels(new Int32Rect(0, 0, BitmapWidth, BitmapHeight), whitescreen, BitmapWidth, 0);
        image.Source = buffer;
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
    }

    public bool UseInterFrameBlending { get; set; }

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

    private byte[] Blend(byte[] newPixels)
    {
        if (previousFrame is null) return newPixels;

        var output = new byte[newPixels.Length];
        //We want an even blend ratio so we just take the average all all the pixels.
        for (int i = 0; i < newPixels.Length; i++)
        {
            output[i] = (byte)((newPixels[i] + previousFrame[i]) / 2);
        }

        return output;
    }

    public void Fs_FramePushed(byte[] pixels)
    {
        if (UseInterFrameBlending) WriteOutputFrame(Blend(pixels));
        else WriteOutputFrame(pixels);

        previousFrame = pixels;
    }

    public event EventHandler? FrameDrawn;

    protected virtual void OnFrameDrawn(EventArgs e) => FrameDrawn?.Invoke(this, e);

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
        OnFrameDrawn(EventArgs.Empty);
    }
}
