using System.Windows.Input;

namespace WPFFrontend;

public class StopCommand : ICommand
{
    private readonly Model Model;

#pragma warning disable 67
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

    public StopCommand(Model model)
    {
        Model = model;
    }

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter)
    {
        Model.ShutdownGameboy();
    }
}
