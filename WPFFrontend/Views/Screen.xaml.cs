using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.Graphics;
using Windows.System;

using WPFFrontend.Platform;
using WPFFrontend.ViewModels;

namespace WPFFrontend.Views;

/// <summary>
/// Interaction logic for Screen.xaml
/// </summary>
public partial class Screen : Window
{
    public GameBoyViewModel ViewModel { get; }
    private readonly Input input;

    public Screen(GameBoyViewModel viewModel, Input input, bool hideMenu = false)
    {
        ViewModel = viewModel;
        this.input = input;
        InitializeComponent();
        ViewModel.WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }
        if (hideMenu)
        {
            MainMenu.Visibility = Visibility.Collapsed;
        }
        RootPanel.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown), true);
        RootPanel.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler(input.KeyUpHandler), true);
        RootPanel.Loaded += (_, _) =>
        {
            ResizeToContent();
            _ = RootPanel.Focus(FocusState.Programmatic);
        };
        Activated += (_, e) =>
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
                input.ReleaseKeys();
        };
        Closed += (_, _) => input.ReleaseKeys();
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        input.KeyDownHandler(sender, e);
        if (e.KeyStatus.WasKeyDown) return;
        IRelayCommand? command = e.Key switch
        {
            VirtualKey.S => ViewModel.Screen.ScreenShotCommand,
            VirtualKey.D => ViewModel.Screen.DebugScreenShotCommand,
            VirtualKey.P => ViewModel.TogglePauseCommand,
            _ => null
        };
        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
            e.Handled = true;
        }
    }

    private void Metrics_Click(object sender, RoutedEventArgs e)
    {
        FPS.Visibility = FPSDisplayEnable.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        ResizeToContent();
    }

    public bool IsControllerSelected(int selected, int controller) => selected == controller;

    private void Controller_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioMenuFlyoutItem { IsChecked: true, Tag: string text }
            && int.TryParse(text, out int controller))
            ViewModel.Input.SelectedController = controller;
    }

    private void ResizeToContent()
    {
        //WinUI has no Window.SizeToContent. Measure content and resize the native client area in pixels.
        RootPanel.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
        double scale = RootPanel.XamlRoot.RasterizationScale;
        AppWindow.ResizeClient(new SizeInt32(
            (int)Math.Ceiling(RootPanel.DesiredSize.Width * scale),
            (int)Math.Ceiling(RootPanel.DesiredSize.Height * scale)));
    }
}
