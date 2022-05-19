
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WPFFrontend;

public class GameBoyViewModel : ObservableObject
{
    private readonly Performance Performance;
    private readonly GameboyScreen Screen;

    public ICommand ScreenShotCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ControllerIDConverter ControllerIDConverter { get; }
    public Model Model { get; }
    public Input Input { get; }
    public ICommand LoadROMPopUp { get; }

    public GameBoyViewModel(GameboyScreen gameboyScreen,
        Performance performance,
        Pause pause,
        PopUp popUp,
        ControllerIDConverter controllerIDConverter,
        Model model,
        Input input)
    {
        Screen = gameboyScreen;

        Performance = performance;

        ScreenShotCommand = new RelayCommand(Screen.SaveScreenShot);

        PauseCommand = new RelayCommand(pause.Execute);

        LoadROMPopUp = new RelayCommand(popUp.LoadROMPopUp);

        StopCommand = new RelayCommand(model.ShutdownGameboy);
        ControllerIDConverter = controllerIDConverter;
        Model = model;
        Input = input;
        Screen.FrameDrawn += Display_FrameDrawn;
    }

    private BitmapSource? image;
    public BitmapSource DisplayFrame
    {
        get => image!;
        set
        {
            image = value;
            OnPropertyChanged();
        }
    }

    public void Display_FrameDrawn(object? sender, EventArgs e)
    {
        PerformanceStatus = Performance.Update();
        DisplayFrame = Screen.output;
    }

    public string PerformanceStatus
    {
        get => Performance.Label;
        set => _ = SetProperty(ref Performance.Label, value);
    }

    public bool BootRomEnabled
    {
        get => Model.BootRomEnabled;
        set => _ = SetProperty(ref Model.BootRomEnabled, value);
    }

    public bool UseInterframeBlending
    {
        get => Screen.UseInterFrameBlending;
        set => _ = SetProperty(ref Screen.UseInterFrameBlending, value);
    }

    public int SelectedController
    {
        get => Input.SelectedController;
        set => _ = SetProperty(Input.SelectedController, value, Input, (i, s) => i.SelectedController = s);
    }
    public bool FpsLockEnabled
    {
        get => Model.FpsLockEnabled;
        set
        {
            if (Model.FpsLockEnabled != value)
            {
                Model.FpsLockEnabled = value;
                OnPropertyChanged();
            }
        }
    }
}