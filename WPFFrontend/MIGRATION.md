# WinUI 3 frontend

The existing `WPFFrontend` project path, assembly name, and namespaces are retained for solution compatibility. The frontend now uses Microsoft.UI.Xaml and Windows App SDK 2.5.1, targets `net11.0-windows10.0.19041.0`, and builds as an unpackaged x64 desktop application. The emulator and audio/controller libraries are unchanged.

No WPF migration skill or scenario was available in the installed agent catalog; this migration used repository inspection and an explicit implementation plan instead.

## Build and run

```powershell
dotnet build emulator.sln
dotnet run --project WPFFrontend/WPFFrontend.csproj -p:Platform=x64
dotnet run --project WPFFrontend/WPFFrontend.csproj -p:Platform=x64 -- --no-menu "C:\games\game.gb"
dotnet test Tests/Tests.csproj
```

Windows App SDK is self-contained; .NET 11 remains framework-dependent. Windows 10 version 1809 or later is required. Both existing solution platform choices map the frontend to x64. The application manifest enables per-monitor-v2 DPI awareness. The boot ROM is copied beside the executable, so it is no longer resolved relative to the current working directory.

## Preserved behavior

- File/Open and Stop; FPS limiter, boot ROM and inter-frame blending settings; optional performance text; four controller selections.
- X/Z, Shift, arrows and Enter for Game Boy input; S for screenshot, D for screenshot beside the ROM, P for pause; Ctrl+O for the ROM picker.
- `--no-menu` and an optional ROM path at startup.
- Four-shade grayscale emulator bytes are copied exactly into opaque BGRA pixels. Optional blending still averages two consecutive frames. PNG screenshots retain native 160x144 resolution. Existing PNG comparison fixtures and their palette normalization are not modified.

## Mappings without direct equivalents

| WPF feature | WinUI implementation / limitation |
| --- | --- |
| Application.Startup and Show | Application.OnLaunched and Window.Activate; host and model disposed when the window closes. |
| Window.DataContext, Window.Resources | Strongly typed window ViewModel for x:Bind; resources belong to content elements or Application. |
| Menu/MenuItem and InputBindings | MenuBar/MenuFlyoutItem/ToggleMenuFlyoutItem/RadioMenuFlyoutItem; routed key events and KeyboardAccelerator. |
| Label.Style DataTrigger | TextBlock and explicit visibility update for performance metrics. |
| Window.SizeToContent | Measure content and call AppWindow.ResizeClient; OverlappedPresenter disables resize/maximize to preserve the fixed display layout. Native minimize/restore remains available. |
| Dispatcher.Invoke with Render priority | DispatcherQueue.TryEnqueue with one outstanding frame and a cancellation-aware producer wait. No WinUI Render-priority equivalent is assumed. FPS/pause flags are read without UI dispatch. Closing, stopping and changing ROMs release the producer without waiting for queued UI work. |
| BitmapSource.Create, Gray8, Freeze | UI-thread-owned WriteableBitmap with explicit opaque BGRA conversion; no Freeze/indexed-palette equivalent. |
| RenderOptions.BitmapScalingMode.NearestNeighbor | Pre-expand pixels four times without interpolation; WinUI Image has no direct nearest-neighbor property. OS/compositor scaling on non-100% DPI displays still needs visual verification; this does not claim exact WPF scaling behavior at every DPI. |
| PngBitmapEncoder | Windows.Graphics.Imaging.BitmapEncoder, using a snapshot of the native-resolution grayscale frame. |
| Microsoft.Win32.OpenFileDialog | Windows.Storage.Pickers.FileOpenPicker initialized with the HWND from WinRT.Interop.WindowNative.GetWindowHandle. |
| MarkupExtension/ValueConversion converter | Typed controller-selection predicates and selection events; WinUI Window-level x:Bind converter lookup generated an invalid FrameworkElement cast. No WPF markup-extension behavior is assumed. |
| System colors | ThemeResource brushes for content background and performance text. |

TwoWay x:Bind is explicit for editable options; OneWay is explicit for performance text and controller-selection predicates. Immutable image and command references use the default OneTime mode. Controller click events write the selected integer explicitly, preserving radio-group behavior without a converter returning strings.

There are no ListBox, TabControl, WebBrowser, DataGrid, PresentationSource, or WindowState usages in this frontend. No unused ListView/ItemsView, TabView, WebView2, or CommunityToolkit.WinUI.Controls.DataGrid dependencies were added. HWND access is used where needed for the picker rather than as a mechanical PresentationSource replacement.

## Validation

Completed validation:

- Debug and Release solution builds: zero warnings and errors. Final IDE solution build also passed.
- Full Debug test suite: 188 passed, 259 failed, 447 total. All 14 new frontend pixel cases passed.
- Original HEAD tested in an isolated temporary checkout with identical workspace ROM/reference assets: 174 passed, 259 failed, 433 total. All 433 pre-existing outcomes match; no new failing tests. Existing emulator timing/test failures remain outside this migration's scope.
- Windows smoke checks: default startup and `--no-menu` with dmg-acid2 ROM both produced a responsive Rotalume window and closed with exit code zero.
- Tracked frontend source contains no WPF namespaces, UseWPF, or Dispatcher.Invoke/BeginInvoke; generated files from older builds are not source and may remain in ignored obj/bin directories.
- Repaired pre-existing missing emulator/WAV solution build mappings that prevented Release dependencies from building. Resolved test-project nullability/unused-result warnings without suppression or changing palette normalization.

`FrontendFramePixelsTests` links the actual platform-independent production pixel conversion source into the existing test project. It verifies all four emulator shades, opacity, nearest-neighbor row/pixel repetition, screenshot buffer dimensions, blending, input immutability, and invalid dimensions/frame lengths.

Interactive acceptance checks still required: ROM picker/cancel, keyboard/gamepad input, optional performance text and controller selection, both screenshot destinations, pause/stop/close while emulating, boot ROM option, and display behavior on different DPI monitors. Build/test results are recorded by the migration assistant; compilation alone does not prove interactive behavior.
