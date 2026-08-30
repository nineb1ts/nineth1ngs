using nineth1ngs.Services;
using nineth1ngs.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using nineth1ngs.Models;
using System.Linq;

namespace nineth1ngs;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly WindowSettingsService settingsService;

    public MainWindow(
        Th1ngStore store,
        WindowSettingsService settingsService,
        Models.WindowSettings settings)
    {
        InitializeComponent();

        this.settingsService = settingsService;

        DataContext = new MainViewModel(
            store,
            ConfirmDeleteAsync,
            ShowError);

        Width = 520;
        Height = 720;

        if (WindowSettingsService.IsValid(settings))
        {
            Left = settings.Left;
            Top = settings.Top;
            WindowStartupLocation = WindowStartupLocation.Manual;
        }

        Loaded += MainWindowLoaded;
        Closing += MainWindowClosing;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var windowHandle = new WindowInteropHelper(this).Handle;

        var source = HwndSource.FromHwnd(windowHandle);
        source?.AddHook(WindowProc);

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        var cornerPreference = 2;

        _ = DwmSetWindowAttribute(
            windowHandle,
            DwmWindowCornerPreferenceAttribute,
            ref cornerPreference,
            sizeof(int));
    }

    private Task<bool> ConfirmDeleteAsync(Models.Th1ng th1ng)
    {
        var dialog = new Views.DeleteConfirmationWindow(
            th1ng.Text,
            th1ng.SubTh1ngs.Count)
        {
            Owner = this
        };

        return Task.FromResult(dialog.ShowDialog() == true);
    }

    private async void MainWindowLoaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= MainWindowLoaded;

        await ((MainViewModel)DataContext).LoadAsync();
    }

    private void TitleBarMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
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

    private void Th1ngRowMouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ClickCount != 1 ||
            IsInteractiveElement(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (sender is Border
            {
                DataContext: Models.Th1ng th1ng
            })
        {
            th1ng.IsExpanded = !th1ng.IsExpanded;
        }
    }

    private void MinimizeClick(
        object sender,
        RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeClick(
        object sender,
        RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void CloseClick(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private static bool IsInteractiveElement(
        DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Button or TextBox or TextBlock)
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

    private void WindowPreviewMouseDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        // Klick innerhalb eines aktuell bearbeiteten TextBox -> nichts tun
        if (e.OriginalSource is DependencyObject clickedElement)
        {
            var current = clickedElement;

            while (current is not null)
            {
                if (current is TextBox textBox &&
                    textBox.DataContext is Th1ng clickedTh1ng &&
                    clickedTh1ng.IsEditing)
                {
                    return;
                }

                current = VisualTreeHelper.GetParent(current);
            }
        }

        var editingTh1ng = viewModel.OpenTh1ngs
            .FirstOrDefault(th1ng => th1ng.IsEditing);

        editingTh1ng ??= viewModel.OpenTh1ngs
            .SelectMany(th1ng => th1ng.SubTh1ngs)
            .FirstOrDefault(sub => sub.IsEditing);

        if (editingTh1ng is null)
        {
            return;
        }

        if (viewModel.SaveEditingCommand.CanExecute(editingTh1ng))
        {
            viewModel.SaveEditingCommand.Execute(editingTh1ng);
        }
    }

    private async void MainWindowClosing(
    object? sender,
    System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            await ((MainViewModel)DataContext).PauseRunningTimersAsync();

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
            ShowError(
                $"The application state could not be saved.\n\n{exception.Message}");
        }
    }

    private void EditTextBoxIsVisibleChanged(
        object sender,
        DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        if (textBox.Visibility != Visibility.Visible)
        {
            return;
        }

        textBox.Dispatcher.BeginInvoke(() =>
        {
            textBox.Focus();
            Keyboard.Focus(textBox);
            textBox.CaretIndex = textBox.Text.Length;
        });
    }

    private static IntPtr WindowProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (msg == WmGetMinMaxInfo)
        {
            AdjustMaximizedSize(hwnd, lParam);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private static void AdjustMaximizedSize(
        IntPtr hwnd,
        IntPtr lParam)
    {
        var minMaxInfo =
            Marshal.PtrToStructure<MinMaxInfo>(lParam);

        var monitor = MonitorFromWindow(
            hwnd,
            MonitorDefaultToNearest);

        if (monitor == IntPtr.Zero)
        {
            return;
        }

        var monitorInfo = new MonitorInfo
        {
            cbSize = Marshal.SizeOf<MonitorInfo>()
        };

        if (!GetMonitorInfo(
                monitor,
                ref monitorInfo))
        {
            return;
        }

        var workArea = monitorInfo.rcWork;
        var monitorArea = monitorInfo.rcMonitor;

        minMaxInfo.ptMaxPosition.X =
            Math.Abs(workArea.Left - monitorArea.Left);

        minMaxInfo.ptMaxPosition.Y =
            Math.Abs(workArea.Top - monitorArea.Top);

        minMaxInfo.ptMaxSize.X =
            Math.Abs(workArea.Right - workArea.Left);

        minMaxInfo.ptMaxSize.Y =
            Math.Abs(workArea.Bottom - workArea.Top);

        Marshal.StructureToPtr(
            minMaxInfo,
            lParam,
            true);
    }

    private const int WmGetMinMaxInfo = 0x0024;

    private const int MonitorDefaultToNearest = 0x00000002;

    private const int DwmWindowCornerPreferenceAttribute = 33;

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public Point ptReserved;
        public Point ptMaxSize;
        public Point ptMaxPosition;
        public Point ptMinTrackSize;
        public Point ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(
        IntPtr hwnd,
        int dwFlags);

    [DllImport(
        "user32.dll",
        CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(
        IntPtr hMonitor,
        ref MonitorInfo lpmi);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int value,
        int valueSize);
}