namespace WPFFrontend;

public class PopUp
{
    private readonly Model Model;

    public PopUp(Model model)
    {
        Model = model;
    }

    public void LoadROMPopUp()
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
