using MvvmGui.Stores;
using MvvmGui.ViewModels;

using System.Windows;

namespace MvvmGui;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly NavigationStore navigationStore;

    protected override void OnStartup(StartupEventArgs e)
    {
        navigationStore.CurrentViewModel = CreateROMSelectViewModel();

        MainWindow = new MainWindow()
        {
            DataContext = new MainViewModel(navigationStore)
        };
    }

    public App() => navigationStore = new NavigationStore();

    private static ROMSelectViewModel CreateROMSelectViewModel() =>
    new(new(""));

}
