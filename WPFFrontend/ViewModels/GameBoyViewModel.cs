
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WPFFrontend;

public class GameBoyViewModel : INotifyPropertyChanged
{
    private readonly Performance Performance;
    private readonly GameboyScreen Screen;

    public ICommand ScreenShotCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ControllerIDConverter ControllerIDConverter { get; }
    public Model Model { get; }
    public ICommand LoadROMPopUp { get; }

    public GameBoyViewModel(GameboyScreen gameboyScreen,
        Performance performance,
        ScreenShotCommand screenShotCommand,
        PauseCommand pauseCommand,
        PopUpCommand popUpCommand,
        StopCommand stopCommand,
        ControllerIDConverter controllerIDConverter,
        Model model)
    {
        Screen = gameboyScreen;


        this.Performance = performance;

        ScreenShotCommand = screenShotCommand;

        PauseCommand = pauseCommand;

        LoadROMPopUp = popUpCommand;

        StopCommand = stopCommand;
        ControllerIDConverter = controllerIDConverter;
        Model = model;
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
        set
        {
            if (Performance.Label != value)
            {
                Performance.Label = value;
                OnPropertyChanged();
            }
        }
    }

    public bool FpsLockEnabled
    {
        get => Model.FpsLockEnabled;
        set => Model.FpsLockEnabled = value;
    }
    public bool BootRomEnabled
    {
        get => Model.BootRomEnabled;
        set => Model.BootRomEnabled = value;
    }

    public bool ShowPerformanceData
    {
        get;
        set;
    }

    public bool UseInterframeBlending
    {
        get => Screen.UseInterFrameBlending;
        set
        {
            if (Screen.UseInterFrameBlending != value)
            {
                Screen.UseInterFrameBlending = value;
                OnPropertyChanged();
            }
        }
    }

    public int SelectedController
    {
        get => Model.SelectedController;
        set
        {
            if (Model.SelectedController != value)
            {
                Model.SelectedController = value;
                OnPropertyChanged();
            }
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}