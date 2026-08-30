using nineth1ngs.Models;
using nineth1ngs.Services;
using nineth1ngs.ViewModels;
using System.Linq;
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
    private bool globalHotkeyRegistered;
    private System.Windows.Point dragStartPoint;
    private Th1ng? draggedTh1ng;
    private Border? currentDropTarget;
    private Border? currentDropIndicator;
    private bool currentInsertAfter;
    private bool sessionNotificationsRegistered;
    private DateTime? sessionLockedAtUtc;
    private Th1ng? sessionLockedTimerTh1ng;
    private bool sessionTimeReviewOpen;

    public MainWindow(
        Th1ngStore store,
        WindowSettingsService settingsService,
        Models.WindowSettings settings)
    {
        InitializeComponent();

        this.settingsService = settingsService;

        var timeCopySettingsService = new TimeCopySettingsService();

        DataContext = new MainViewModel(
            store,
            timeCopySettingsService,
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

        sessionNotificationsRegistered =
            WTSRegisterSessionNotification(
                windowHandle,
                NotifyForThisSession);

        if (!sessionNotificationsRegistered)
        {
            ShowError(
                "Windows session notifications could not be registered.\n\n" +
                "Automatic timer handling when the screen is locked will be unavailable.");
        }

        globalHotkeyRegistered = RegisterHotKey(
            windowHandle,
            GlobalHotkeyId,
            ModControl | ModAlt,
            VirtualKeyN);

        if (!globalHotkeyRegistered)
        {
            ShowError(
                "The global shortcut Ctrl + Alt + N could not be registered.\n\n" +
                "Another application may already be using it.");
        }

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

    protected override void OnClosed(EventArgs e)
    {
        var windowHandle = new WindowInteropHelper(this).Handle;

        if (sessionNotificationsRegistered)
        {
            _ = WTSUnRegisterSessionNotification(windowHandle);
            sessionNotificationsRegistered = false;
        }

        if (globalHotkeyRegistered)
        {
            _ = UnregisterHotKey(windowHandle, GlobalHotkeyId);
            globalHotkeyRegistered = false;
        }

        base.OnClosed(e);
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

    private async Task HandleSessionLockAsync()
    {
        if (sessionLockedAtUtc.HasValue ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var lockedAtUtc = DateTime.UtcNow;
        sessionLockedAtUtc = lockedAtUtc;

        sessionLockedTimerTh1ng =
            await viewModel.PauseRunningTimerForSessionLockAsync(
                lockedAtUtc);

        if (sessionLockedTimerTh1ng is null)
        {
            sessionLockedAtUtc = null;
        }
    }

    private async Task HandleSessionUnlockAsync()
    {
        if (sessionTimeReviewOpen ||
            !sessionLockedAtUtc.HasValue ||
            sessionLockedTimerTh1ng is null ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var lockedAtUtc = sessionLockedAtUtc.Value;
        var previouslyRunningTh1ng = sessionLockedTimerTh1ng;

        sessionLockedAtUtc = null;
        sessionLockedTimerTh1ng = null;

        var lockedSeconds = Math.Max(
            0,
            (int)(DateTime.UtcNow - lockedAtUtc).TotalSeconds);

        if (lockedSeconds < 60)
        {
            return;
        }

        var availableTh1ngs = viewModel.OpenTh1ngs
            .Where(th1ng =>
                !th1ng.ParentId.HasValue &&
                !th1ng.IsCompleted)
            .ToList();

        if (availableTh1ngs.Count == 0)
        {
            return;
        }

        sessionTimeReviewOpen = true;

        try
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Show();
            Activate();

            var dialog = new Views.SessionTimeReviewWindow(
                lockedSeconds,
                availableTh1ngs,
                previouslyRunningTh1ng.Id)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true &&
                dialog.SelectedTh1ng is not null)
            {
                await viewModel.AddElapsedTimeAsync(
                    dialog.SelectedTh1ng,
                    lockedSeconds);
            }
        }
        finally
        {
            sessionTimeReviewOpen = false;
        }
    }

    private void FocusNewTh1ngInput()
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.ShowOpenCommand.Execute(null);
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Show();
        Activate();

        Dispatcher.BeginInvoke(() =>
        {
            NewTh1ngTextBox.Focus();
            Keyboard.Focus(NewTh1ngTextBox);
            NewTh1ngTextBox.CaretIndex = NewTh1ngTextBox.Text.Length;
        });
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

    private void Th1ngCardPreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        dragStartPoint = e.GetPosition(this);
        draggedTh1ng = null;

        if (sender is not Border
            {
                DataContext: Th1ng th1ng
            } ||
            th1ng.ParentId.HasValue ||
            th1ng.IsCompleted ||
            IsDragBlockedElement(e.OriginalSource as DependencyObject))
        {
            return;
        }

        draggedTh1ng = th1ng;
    }

    private void Th1ngCardPreviewMouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed ||
            draggedTh1ng is null)
        {
            return;
        }

        var currentPosition = e.GetPosition(this);

        if (Math.Abs(currentPosition.X - dragStartPoint.X) <
                SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(currentPosition.Y - dragStartPoint.Y) <
                SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var source = draggedTh1ng;
        draggedTh1ng = null;

        _ = DragDrop.DoDragDrop(
            (DependencyObject)sender,
            new DataObject(typeof(Th1ng), source),
            DragDropEffects.Move);

        ClearDropTarget();
    }

    private void ContentScrollViewerPreviewDragOver(
        object sender,
        DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(Th1ng)) ||
            e.Data.GetData(typeof(Th1ng)) is not Th1ng th1ng ||
            th1ng.IsCompleted ||
            th1ng.ParentId.HasValue)
        {
            return;
        }

        var pointer = e.GetPosition(ContentScrollViewer);

        const double scrollZone = 72;
        const double minimumStep = 4;
        const double maximumStep = 18;

        if (pointer.Y < scrollZone)
        {
            var strength = 1 - Math.Clamp(
                pointer.Y / scrollZone,
                0,
                1);

            var step =
                minimumStep +
                ((maximumStep - minimumStep) * strength);

            ContentScrollViewer.ScrollToVerticalOffset(
                Math.Max(
                    0,
                    ContentScrollViewer.VerticalOffset - step));
        }
        else if (pointer.Y >
                 ContentScrollViewer.ActualHeight - scrollZone)
        {
            var distanceFromBottom =
                ContentScrollViewer.ActualHeight - pointer.Y;

            var strength = 1 - Math.Clamp(
                distanceFromBottom / scrollZone,
                0,
                1);

            var step =
                minimumStep +
                ((maximumStep - minimumStep) * strength);

            ContentScrollViewer.ScrollToVerticalOffset(
                Math.Min(
                    ContentScrollViewer.ScrollableHeight,
                    ContentScrollViewer.VerticalOffset + step));
        }
    }

    private void Th1ngCardDragOver(
        object sender,
        DragEventArgs e)
    {
        if (sender is not Border targetCard ||
            targetCard.DataContext is not Th1ng target ||
            !e.Data.GetDataPresent(typeof(Th1ng)) ||
            e.Data.GetData(typeof(Th1ng)) is not Th1ng source ||
            ReferenceEquals(source, target) ||
            source.ParentId.HasValue ||
            target.ParentId.HasValue ||
            source.IsCompleted ||
            target.IsCompleted)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            ClearDropTarget();
            return;
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;

        var pointer = e.GetPosition(targetCard);
        var insertAfter =
            pointer.Y > targetCard.ActualHeight / 2;

        if (!ReferenceEquals(currentDropTarget, targetCard) ||
            currentInsertAfter != insertAfter)
        {
            ClearDropTarget();

            currentDropTarget = targetCard;
            currentInsertAfter = insertAfter;

            if (targetCard.Parent is Grid container)
            {
                currentDropIndicator = container.Children
                    .OfType<Border>()
                    .FirstOrDefault(border =>
                        border.Name == (insertAfter
                            ? "BottomDropIndicator"
                            : "TopDropIndicator"));
            }

            if (currentDropIndicator is not null)
            {
                currentDropIndicator.Visibility = Visibility.Visible;
            }


        }
    }

    private void Th1ngCardDragLeave(
        object sender,
        DragEventArgs e)
    {
        if (ReferenceEquals(
                sender,
                currentDropTarget))
        {
            ClearDropTarget();
        }
    }

    private void Th1ngCardDrop(
        object sender,
        DragEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel ||
            sender is not Border targetCard ||
            targetCard.DataContext is not Th1ng target ||
            !e.Data.GetDataPresent(typeof(Th1ng)) ||
            e.Data.GetData(typeof(Th1ng)) is not Th1ng source)
        {
            ClearDropTarget();
            return;
        }

        var pointer = e.GetPosition(targetCard);
        var insertAfter =
            pointer.Y > targetCard.ActualHeight / 2;

        viewModel.MoveOpenTh1ng(
            source,
            target,
            insertAfter);

        e.Handled = true;
        ClearDropTarget();
    }

    private void ClearDropTarget()
    {
        if (currentDropIndicator is not null)
        {
            currentDropIndicator.Visibility = Visibility.Collapsed;
            currentDropIndicator = null;
        }

        if (currentDropTarget is not null)
        {
            currentDropTarget = null;
        }

        currentInsertAfter = false;
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

    private static bool IsDragBlockedElement(
        DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Button or TextBox)
            {
                return true;
            }

            if (source is TextBlock textBlock)
            {
                var hasSingleClickBinding = textBlock.InputBindings
                    .OfType<MouseBinding>()
                    .Any(binding =>
                        binding.MouseAction == MouseAction.LeftClick);

                if (hasSingleClickBinding)
                {
                    return true;
                }
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private static bool IsInteractiveElement(
        DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Button or TextBox)
            {
                return true;
            }

            if (source is TextBlock textBlock &&
                textBlock.InputBindings.Count > 0)
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

    private IntPtr WindowProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (msg == WmWtsSessionChange)
        {
            var sessionReason = wParam.ToInt32();

            if (sessionReason == WtsSessionLock)
            {
                Dispatcher.BeginInvoke(
                    async () => await HandleSessionLockAsync());
            }
            else if (sessionReason == WtsSessionUnlock)
            {
                Dispatcher.BeginInvoke(
                    async () => await HandleSessionUnlockAsync());
            }
        }

        if (msg == WmHotkey &&
            wParam.ToInt32() == GlobalHotkeyId)
        {
            FocusNewTh1ngInput();
            handled = true;
            return IntPtr.Zero;
        }

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

    private const int GlobalHotkeyId = 9001;

    private const int WmHotkey = 0x0312;
    private const int WmGetMinMaxInfo = 0x0024;
    private const int WmWtsSessionChange = 0x02B1;

    private const int WtsSessionLock = 0x7;
    private const int WtsSessionUnlock = 0x8;
    private const int NotifyForThisSession = 0;

    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint VirtualKeyN = 0x4E;

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
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(
        IntPtr hWnd,
        int id);

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

    [DllImport("wtsapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WTSRegisterSessionNotification(
        IntPtr hWnd,
        int dwFlags);

    [DllImport("wtsapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WTSUnRegisterSessionNotification(
        IntPtr hWnd);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int value,
        int valueSize);
}
