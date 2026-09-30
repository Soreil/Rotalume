using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Windows;

using WPFFrontend.Glue;
using WPFFrontend.Models;
using WPFFrontend.Platform;
using WPFFrontend.Services;
using WPFFrontend.ViewModels;
using WPFFrontend.Views;

namespace WPFFrontend;

public partial class App : Application
{
    private static IHostBuilder CreateHostBuilder(string[] args) =>
Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.
    AddSingleton<GameBoyViewModel>().
    AddSingleton<GameboyScreen>().
    AddSingleton<GameboyTimingInfo>().
    AddSingleton<Input>().
    AddSingleton<Model>().
    AddSingleton<FileService>().
    AddSingleton<ControllerIDConverter>()
    );

    private readonly IHost host;
    public App()
    {
        var hostBuilder = CreateHostBuilder([]);

        host = hostBuilder.Start();
    }
    private void OnStartup(object sender, StartupEventArgs e)
    {
        var vm = host.Services.GetRequiredService<GameBoyViewModel>();
        var model = host.Services.GetRequiredService<Model>();

        var hideMenu = e.Args.Contains("--no-menu");
        var romPath = e.Args.FirstOrDefault(arg => arg != "--no-menu");
        var mainWindow = new Screen(model, hideMenu) { DataContext = vm };

        var input = host.Services.GetRequiredService<Input>();
        KeyboardViewModelBridge.Connect(input, mainWindow);

        mainWindow.Show();

        if (romPath is not null)
        {
            model.ROM = romPath;
        }
    }
}
