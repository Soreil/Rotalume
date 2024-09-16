
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Windows.Input;
using System.Windows.Media.Imaging;

using WPFFrontend.Models;
using WPFFrontend.Platform;
using WPFFrontend.Views;

namespace WPFFrontend.ViewModels;

public partial class GameBoyViewModel : ObservableObject, IDisposable
{
    public GameboyTimingInfo Performance { get; }
    public GameboyScreen Screen { get; }

    public ICommand StopCommand { get; }
    public ControllerIDConverter ControllerIDConverter { get; }
    private Model Model { get; }
    public Input Input { get; }

    public GameBoyViewModel(GameboyScreen gameboyScreen,
        GameboyTimingInfo performance,
        ControllerIDConverter controllerIDConverter,
        Model model,
        Input input)
    {
        Screen = gameboyScreen;
        Performance = performance;
        StopCommand = new RelayCommand(model.ShutdownGameboy);
        ControllerIDConverter = controllerIDConverter;
        Model = model;
        Input = input;
        Screen.FrameDrawn += Display_FrameDrawn;
    }

    [ObservableProperty]
    private BitmapSource? displayFrame;

    [RelayCommand]
    private void TogglePause() => Model.Paused = !Model.Paused;

    [RelayCommand]
    public void LoadROMPopUp()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".gb", Filter = "ROM Files (*.gb;*.gbc)|*.gb;*.gbc" };
        var result = ofd.ShowDialog();
        if (result == true)
        {
            Model.ROM = ofd.FileName;
        }
    }

    private void Display_FrameDrawn(object? sender, EventArgs e)
    {
        Performance.Update();
        DisplayFrame = Screen.output;
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
