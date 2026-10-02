using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media.Imaging;

using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Graphics.Imaging;

using WPFFrontend.Services;

namespace WPFFrontend.Platform;

public partial class GameboyScreen : ObservableObject
{
    private byte[]? previousFrame;
    private byte[] currentFrame;

    public WriteableBitmap Output { get; }

    public GameboyScreen(FileService fileService, ILogger<GameboyScreen> logger)
    {
        currentFrame = new byte[FramePixels.Width * FramePixels.Height];
        Array.Fill(currentFrame, (byte)0xff);

        Output = new WriteableBitmap(FramePixels.Width * FramePixels.DisplayScale,
            FramePixels.Height * FramePixels.DisplayScale);
        FileService = fileService;
        Logger = logger;
        WriteOutputFrame(currentFrame);
    }

    [ObservableProperty]
    public partial bool UseInterFrameBlending { get; set; }
    public FileService FileService { get; }
    public ILogger<GameboyScreen> Logger { get; }

    [RelayCommand]
    public async Task DebugScreenShotAsync()
    {
        var romPath = FileService.ROMPath;
        if (romPath is null) return;

        //This is ugly
        var path = Path.ChangeExtension(romPath, ".png");
        await WriteScreenShotAsync(path);
    }

    [RelayCommand]
    public async Task ScreenShotAsync()
    {
        //All of the parameters here should come from configuration
        string fileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            $"Screenshot_{DateTime.Now:dd_MMMM_hh_mm_ss_tt}.png");
        await WriteScreenShotAsync(fileName);
    }

    private async Task WriteScreenShotAsync(string fileName)
    {
        //Snapshot before awaiting: emulation may publish another frame during encoding.
        byte[] pixels = FramePixels.ToBgra(currentFrame, FramePixels.Width, FramePixels.Height);
        try
        {
            using FileStream fs = new(fileName, FileMode.Create);
            using var stream = fs.AsRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                FramePixels.Width, FramePixels.Height, 96, 96, pixels);
            await encoder.FlushAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException)
        {
            Logger.LogError(ex, "Could not save screenshot to {FileName}", fileName);
        }
    }

    public void Fs_FramePushed(byte[] pixels)
    {
        //Average consecutive frames to preserve the optional LCD retention effect.
        WriteOutputFrame(UseInterFrameBlending && previousFrame is not null
            ? FramePixels.Blend(pixels, previousFrame) : pixels);

        previousFrame = pixels;
    }

    public event EventHandler? FrameDrawn;

    protected virtual void OnFrameDrawn(EventArgs e) => FrameDrawn?.Invoke(this, e);

    private void WriteOutputFrame(byte[] pixels)
    {
        var bgra = FramePixels.ToBgra(pixels, FramePixels.Width, FramePixels.Height, FramePixels.DisplayScale);
        using (var stream = Output.PixelBuffer.AsStream())
            stream.Write(bgra);
        currentFrame = pixels;
        Output.Invalidate();
        OnFrameDrawn(EventArgs.Empty);
    }
}
