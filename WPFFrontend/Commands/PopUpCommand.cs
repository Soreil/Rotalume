using System.Windows.Input;

namespace WPFFrontend;

public class PopUpCommand : ICommand
{
    private readonly Model Model;

#pragma warning disable 67
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

    public PopUpCommand(Model model)
    {
        Model = model;
    }

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => LoadROMPopUp();
    private void LoadROMPopUp()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".gb", Filter = "ROM Files (*.gb;*.gbc)|*.gb;*.gbc" };
        var result = ofd.ShowDialog();
        if (result == false)
        {
            return;
        }

        Model.ROM = ofd.FileName;
    }

}
