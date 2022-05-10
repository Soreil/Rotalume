using System.Windows.Input;

namespace WPFFrontend;

public class PauseCommand : ICommand
{
    private readonly Model Model;

#pragma warning disable 67
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

    public PauseCommand(Model model)
    {
        Model = model;
    }

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => Model.Paused = !Model.Paused;
}
