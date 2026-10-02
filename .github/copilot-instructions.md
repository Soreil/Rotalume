# Copilot Instructions

## Project Guidelines
- The emulator uses four grayscale shades internally; expected PNG comparison images should be normalized to the same internal four-shade palette, including green-themed reference images.

## WPF to WinUI 3 Migration Guidelines
- Use Windows App SDK and Microsoft.UI.Xaml for migration.
- Utilize DispatcherQueue.TryEnqueue for UI thread operations.
- Implement AppWindow/OverlappedPresenter for window management.
- Use WindowNative.GetWindowHandle for window handle retrieval.
- Apply ThemeResource for system colors.
- Prefer x:Bind over traditional binding where possible.
- Use ListView/ItemsView for displaying collections.
- Implement TabView for tabbed interfaces.
- Use WebView2 for web content integration.
- Incorporate CommunityToolkit.WinUI.Controls.DataGrid for data presentation.
- Avoid introducing System.Windows namespaces or using Dispatcher.Invoke.
- Flag APIs without direct WinUI 3 equivalents rather than guessing.