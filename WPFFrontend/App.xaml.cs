using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using WPFFrontend.Models;
using WPFFrontend.Platform;
using WPFFrontend.Services;
using WPFFrontend.ViewModels;
using WPFFrontend.Views;

namespace WPFFrontend;

public partial class App : Application
{
    private static IHostBuilder CreateHostBuilder(string[] args, DispatcherQueue dispatcherQueue) =>
Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.
    AddSingleton(dispatcherQueue).
    AddSingleton<GameBoyViewModel>().
    AddSingleton<GameboyScreen>().
    AddSingleton<GameboyTimingInfo>().
    AddSingleton<Input>().
    AddSingleton<Model>().
    AddSingleton<FileService>()
    );

    private IHost? host;
    private Screen? mainWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var commandLine = Environment.GetCommandLineArgs().Skip(1).ToArray();
        host = CreateHostBuilder([], DispatcherQueue.GetForCurrentThread()).Start();
        var vm = host.Services.GetRequiredService<GameBoyViewModel>();
        var model = host.Services.GetRequiredService<Model>();
        var hideMenu = commandLine.Contains("--no-menu");
        var romPath = commandLine.FirstOrDefault(arg => arg != "--no-menu");
        var input = host.Services.GetRequiredService<Input>();
        mainWindow = new Screen(vm, input, hideMenu);
        mainWindow.Closed += (_, _) =>
        {
            host.Dispose();
            host = null;
            mainWindow = null;
        };
        mainWindow.Activate();

        if (romPath is not null)
        {
            model.ROM = romPath;
        }
    }
}
