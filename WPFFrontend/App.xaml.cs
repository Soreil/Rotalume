using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Windows;

namespace WPFFrontend;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IHostBuilder CreateHostBuilder(string[] args) =>
Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.
    AddSingleton<GameBoyViewModel>().
    AddSingleton<GameboyScreen>().
    AddSingleton<Performance>().
    AddSingleton<Input>().
    AddSingleton<ScreenShotCommand>().
    AddSingleton<Pause>().
    AddSingleton<Model>().
    AddSingleton<PopUp>().
    AddSingleton<StopCommand>().
    AddSingleton<ControllerIDConverter>()

    );

    private readonly IHost host;
    public App()
    {
        var hostBuilder = CreateHostBuilder(Array.Empty<string>());

        host = hostBuilder.Start();
    }
    private void OnStartup(object sender, StartupEventArgs e)
    {
        var vm = host.Services.GetRequiredService<GameBoyViewModel>();
        var model = host.Services.GetRequiredService<Model>();

        var mainWindow = new Screen(model) { DataContext = vm };

        var input = host.Services.GetRequiredService<Input>();
        KeyboardViewModelBridge.Connect(input, mainWindow);

        mainWindow.Show();
    }
}
