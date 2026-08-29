using nineth1ngs.Services;
using nineth1ngs.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;

namespace nineth1ngs;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly WindowSettingsService settingsService;

    public MainWindow(Th1ngStore store, WindowSettingsService settingsService, Models.WindowSettings settings)
    {
        InitializeComponent();
        this.settingsService = settingsService;
        DataContext = new MainViewModel(store, ConfirmDeleteAsync, ShowError);

        if (WindowSettingsService.IsValid(settings))
        {
            Width = settings.Width;
            Height = settings.Height;
            Left = settings.Left;
            Top = settings.Top;
            WindowStartupLocation = WindowStartupLocation.Manual;
        }

        Loaded += MainWindowLoaded;
        Closed += MainWindowClosed;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        var windowHandle = new WindowInteropHelper(this).Handle;
        var cornerPreference = 2;
        _ = DwmSetWindowAttribute(
            windowHandle,
            DwmWindowCornerPreferenceAttribute,
            ref cornerPreference,
            sizeof(int));
    }

    private Task<bool> ConfirmDeleteAsync(Models.Th1ng th1ng)
    {
        var dialog = new Views.DeleteConfirmationWindow(th1ng.Text, th1ng.SubTh1ngs.Count)
        {
            Owner = this
        };

        return Task.FromResult(dialog.ShowDialog() == true);
    }

    private async void MainWindowLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindowLoaded;
        await ((MainViewModel)DataContext).LoadAsync();
    }

    private void MainWindowClosed(object? sender, EventArgs e)
    {
        try
        {
            settingsService.Save(new Models.WindowSettings
            {
                Width = Width,
                Height = Height,
                Left = Left,
                Top = Top
            });
        }
        catch (Exception exception)
        {
            ShowError($"The window settings could not be saved.\n\n{exception.Message}");
        }
    }

    private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        DragMove();
    }

    private void Th1ngRowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 1 || IsInteractiveElement(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (sender is Border { DataContext: Models.Th1ng th1ng })
        {
            th1ng.IsExpanded = !th1ng.IsExpanded;
        }
    }

    private void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeClick(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void CloseClick(object sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private static bool IsInteractiveElement(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Button or TextBox)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "nineth1ngs",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private const int DwmWindowCornerPreferenceAttribute = 33;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int value,
        int valueSize);
}