using System.Windows.Input;

namespace WPFFrontend;

public class ScreenShotCommand : ICommand
{
    private readonly GameboyScreen GameboyScreen;

#pragma warning disable 67
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

    public ScreenShotCommand(GameboyScreen gameboy)
    {
        GameboyScreen = gameboy;
    }

    private void SaveScreenShot() => GameboyScreen.SaveScreenShot();

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => SaveScreenShot();
}
