
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Storage.Pickers;

using WPFFrontend.Models;
using WPFFrontend.Platform;

namespace WPFFrontend.ViewModels;

public partial class GameBoyViewModel : ObservableObject, IDisposable
{
    public GameboyTimingInfo Performance { get; }
    public GameboyScreen Screen { get; }

    public IRelayCommand StopCommand { get; }
    public nint WindowHandle { get; set; }
    private Model Model { get; }
    public Input Input { get; }

    public GameBoyViewModel(GameboyScreen gameboyScreen,
        GameboyTimingInfo performance,
        Model model,
        Input input)
    {
        Screen = gameboyScreen;
        Performance = performance;
        StopCommand = new RelayCommand(model.ShutdownGameboy);
        Model = model;
        Input = input;
        Screen.FrameDrawn += Display_FrameDrawn;
        DisplayFrame = Screen.Output;
    }

    public WriteableBitmap DisplayFrame { get; }

    [RelayCommand]
    private void TogglePause() => Model.Paused = !Model.Paused;

    [RelayCommand]
    public async Task LoadROMPopUpAsync()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".gb");
        picker.FileTypeFilter.Add(".gbc");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WindowHandle);
        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            Model.ROM = file.Path;
        }
    }

    private void Display_FrameDrawn(object? sender, EventArgs e)
    {
        Performance.Update();
    }

    public bool BootRomEnabled
    {
        get => Model.BootRomEnabled;
        set => SetProperty(ref Model.BootRomEnabled, value);
    }

    public bool FpsLockEnabled
    {
        get => Model.FpsLockEnabled;
        set => SetProperty(Model.FpsLockEnabled, value, Model, (i, s) => i.FpsLockEnabled = s);
    }

    public void Dispose()
    {
        Screen.FrameDrawn -= Display_FrameDrawn;
        GC.SuppressFinalize(this);
    }
}
