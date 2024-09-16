using System.ComponentModel;
using System.Windows;

using WPFFrontend.Models;

namespace WPFFrontend.Views;

/// <summary>
/// Interaction logic for Screen.xaml
/// </summary>
public partial class Screen : Window
{
    private readonly Model model;
    public Screen(Model model)
    {
        InitializeComponent();
        this.model = model;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        model.Dispose();

        //This should be triggered by the thread shutting down but it gets stuck calling back in to this thread
        //via the dispatcher before it even has a chance to acknowledge cancelrequested often.
        //If we don't stop it from polling the xboxcontroller will keep polling in the background forever.
        base.OnClosing(e);
    }
}
