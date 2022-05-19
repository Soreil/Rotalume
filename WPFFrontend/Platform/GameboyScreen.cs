﻿using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFFrontend;

public class GameboyScreen
{
    private const int BitmapWidth = 160;
    private const int BitmapHeight = 144;

    private byte[]? previousFrame;
    private byte[] currentFrame;

    public BitmapSource output;

    private static BitmapSource MakeBitmap(byte[] output)
    {
        var fmt = PixelFormats.Gray8;
        var width = BitmapWidth;
        var bitsPerPixel = 8;
        var height = BitmapHeight;
        var stride = ((bitsPerPixel * width) + 31) / 32 * 4;
        var dpi = 96.0;

        var Source = BitmapSource.Create(
            width,
            height,
            dpi,
            dpi,
            fmt,
            BitmapPalettes.Gray256,
            output,
            stride);

        Source.Freeze();

        return Source;
    }

    public GameboyScreen()
    {
        currentFrame = new byte[BitmapWidth * BitmapHeight];
        for (int i = 0; i < currentFrame.Length; i++)
            currentFrame[i] = 0xff;
        output = MakeBitmap(currentFrame);
    }

    public bool UseInterFrameBlending;

    internal void SaveScreenShot()
    {
        string fileName = string.Format(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
@"\Screenshot" + "_" +
DateTime.Now.ToString("(dd_MMMM_hh_mm_ss_tt)") + ".png");

        using FileStream fs = new(fileName, FileMode.Create);

        var encoder = new PngBitmapEncoder();

        encoder.Frames.Add(BitmapFrame.Create(output));
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
        output = MakeBitmap(pixels);

        OnFrameDrawn(EventArgs.Empty);
    }
}
