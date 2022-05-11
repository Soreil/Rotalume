using System.ComponentModel;
using System.Windows;

namespace WPFFrontend;

/// <summary>
/// Interaction logic for Screen.xaml
/// </summary>
public partial class Screen : Window
{
    Model Model;
    public Screen(Model model)
    {
        InitializeComponent();
        Model = model;
    }

    //private void LoadROM(object sender, DragEventArgs e)
    //{
    //    // If the DataObject contains string data, extract it.
    //    if (e.Data.GetDataPresent(DataFormats.FileDrop))
    //    {
    //        string[]? fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

    //        //Check that the file isn't a folder
    //        if (fileNames is not null && fileNames.Length == 1 && File.Exists(fileNames[0]))
    //        {
    //            Model.ROM = fileNames[0];
    //        }
    //    }
    //}

    protected override void OnClosing(CancelEventArgs e)
    {
        Model.Dispose();

        //This should be triggered by the thread shutting down but it gets stuck calling back in to this thread
        //via the dispatcher before it even has a chance to acknowledge cancelrequested often.
        //If we don't stop it from polling the xboxcontroller will keep polling in the background forever.
        base.OnClosing(e);
    }
}
