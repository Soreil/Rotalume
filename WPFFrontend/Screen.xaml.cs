using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFFrontend;

/// <summary>
/// Interaction logic for Screen.xaml
/// </summary>
public partial class Screen : Window
{
    private readonly GameBoyViewModel viewModel;

    public Screen(GameBoyViewModel vm)
    {
        InitializeComponent();

        viewModel = vm;
    }

    private void LoadROM(object sender, DragEventArgs e)
    {
        // If the DataObject contains string data, extract it.
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[]? fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            //Check that the file isn't a folder
            if (fileNames is not null && fileNames.Length == 1 && File.Exists(fileNames[0]))
            {
                viewModel.SpinUpNewGameboy(fileNames[0]);
            }
        }

    }
    private void LoadROMPopUp(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".gb", Filter = "ROM Files (*.gb;*.gbc)|*.gb;*.gbc" };
        var result = ofd.ShowDialog();
        if (result == false)
        {
            return;
        }

        viewModel.SpinUpNewGameboy(ofd.FileName);
    }
    private void CloseGameboyRequest(object sender, RoutedEventArgs e) => viewModel.ShutdownGameboy();
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.P)
        {
            viewModel.Pause();
        }
        if (e.Key == Key.S)
        {
            viewModel.SaveScreenShot();
        }
    }
    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is null) return;
        var li = (RadioButton)sender;
        viewModel.SelectedController = li.Content switch
        {
            "1" => 1,
            "2" => 2,
            "3" => 3,
            "4" => 4,
            _ => 0,
        };
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        viewModel.Dispose();

        //This should be triggered by the thread shutting down but it gets stuck calling back in to this thread
        //via the dispatcher before it even has a chance to acknowledge cancelrequested often.
        //If we don't stop it from polling the xboxcontroller will keep polling in the background forever.
        base.OnClosing(e);
    }
}
