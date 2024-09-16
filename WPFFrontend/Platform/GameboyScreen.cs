using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using WPFFrontend.Services;

namespace WPFFrontend.Platform;

public partial class GameboyScreen : ObservableObject
{
    private const int BitmapWidth = 160;
    private const int BitmapHeight = 144;
    private const int BitsPerPixel = 8;
    private const double Dpi = 96.0;
    private static readonly int Stride = ((BitsPerPixel * BitmapWidth) + 31) / 32 * 4;

    private byte[]? previousFrame;
    private readonly byte[] currentFrame;

    public BitmapSource output;

    private static BitmapSource MakeBitmap(byte[] output)
    {
        var fmt = PixelFormats.Gray8;

        var source = BitmapSource.Create(
            BitmapWidth,
            BitmapHeight,
            Dpi,
            Dpi,
            fmt,
            BitmapPalettes.Gray256,
            output,
            Stride);

        source.Freeze();

        return source;
    }

    public GameboyScreen(FileService fileService, ILogger<GameboyScreen> logger)
    {
        currentFrame = new byte[BitmapWidth * BitmapHeight];
        Array.Fill(currentFrame, (byte)0xff);

        output = MakeBitmap(currentFrame);
        FileService = fileService;
        Logger = logger;
    }

    [ObservableProperty]
    private bool useInterFrameBlending;

    public FileService FileService { get; }
    public ILogger<GameboyScreen> Logger { get; }

    [RelayCommand]
    public void DebugScreenShot()
    {
        var romPath = FileService.ROMPath;
        if (romPath is null) return;

        //This is ugly
        var path = Path.ChangeExtension(romPath, ".png");
        WriteScreenShot(path);
    }

    [RelayCommand]
    public void ScreenShot()
    {
        //All of the parameters here should come from configuration
        string fileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            $"Screenshot_{DateTime.Now:dd_MMMM_hh_mm_ss_tt}.png");
        WriteScreenShot(fileName);
    }

    private void WriteScreenShot(string fileName)
    {
        using FileStream fs = new(fileName, FileMode.Create);

        var encoder = new PngBitmapEncoder();

        encoder.Frames.Add(BitmapFrame.Create(output));
        encoder.Save(fs);
    }

    //The reason blending frames is done is so better emulate the look of a gameboy screen.
    //There is quite a large amount of image retention on a normal gameboy screen and blending two frames gives
    //a decently convincing level of blurriness. Some games also rely on this behaviour to look correct.
    private byte[] Blend(byte[] newPixels)
    {
        if (previousFrame is null) return newPixels;

        var output = new byte[newPixels.Length];
        //We want an even blend ratio so we just take the average all all the pixels.
        for (int i = 0; i < newPixels.Length; i++)
        {
            output[i] = (byte)((newPixels[i] + previousFrame![i]) / 2);
        }

        return output;
    }

    public void Fs_FramePushed(byte[] pixels)
    {
        WriteOutputFrame(UseInterFrameBlending ? Blend(pixels) : pixels);

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
