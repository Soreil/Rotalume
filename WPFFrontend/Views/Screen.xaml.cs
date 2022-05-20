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

    protected override void OnClosing(CancelEventArgs e)
    {
        Model.Dispose();

        //This should be triggered by the thread shutting down but it gets stuck calling back in to this thread
        //via the dispatcher before it even has a chance to acknowledge cancelrequested often.
        //If we don't stop it from polling the xboxcontroller will keep polling in the background forever.
        base.OnClosing(e);
    }
}
