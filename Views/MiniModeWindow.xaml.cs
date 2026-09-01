using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using nineth1ngs.Services;
using nineth1ngs.ViewModels;

namespace nineth1ngs.Views;

public partial class MiniModeWindow : Window
{
    private readonly Action returnToNormal;
    private readonly Action closeApplication;

    public MiniModeWindow(
        MainViewModel viewModel,
        Action returnToNormal,
        Action closeApplication)
    {
        InitializeComponent();

        DataContext = viewModel;
        this.returnToNormal = returnToNormal;
        this.closeApplication = closeApplication;
        viewModel.SelectMiniTh1ng(
            viewModel.MiniDisplayedTh1ng ?? viewModel.OpenTh1ngs.FirstOrDefault());
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        var cornerPreference = 2;

        _ = DwmSetWindowAttribute(
            new WindowInteropHelper(this).Handle,
            DwmWindowCornerPreferenceAttribute,
            ref cornerPreference,
            sizeof(int));
    }

    private void ReturnToNormalClick(
        object sender,
        RoutedEventArgs e)
    {
        returnToNormal();
    }

    private void CloseApplicationClick(
        object sender,
        RoutedEventArgs e)
    {
        closeApplication();
    }

    private void RailHoverZoneMouseEnter(
        object sender,
        MouseEventArgs e)
    {
        RailControls.IsHitTestVisible = true;
        AnimateRail(1, 0);
    }

    private void RailHoverZoneMouseLeave(
        object sender,
        MouseEventArgs e)
    {
        AnimateRail(0, -8);
        RailControls.IsHitTestVisible = false;
    }

    private void AnimateRail(
        double opacity,
        double offset)
    {
        var duration = new Duration(TimeSpan.FromMilliseconds(160));

        RailControls.BeginAnimation(
            OpacityProperty,
            new DoubleAnimation(opacity, duration));

        if (RailControls.RenderTransform is TranslateTransform transform)
        {
            transform.BeginAnimation(
                TranslateTransform.XProperty,
                new DoubleAnimation(offset, duration));
        }
    }

    private void MiniDragAreaMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            try
            {
                DragMove();
                KeepOnWorkingArea();
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    public void KeepOnWorkingArea()
    {
        var position = MiniModeLayoutService.SnapToWorkingArea(
            new Point(Left, Top),
            new Size(ActualWidth > 0 ? ActualWidth : Width,
                     ActualHeight > 0 ? ActualHeight : Height),
            GetWorkingArea());

        Left = position.X;
        Top = position.Y;
    }

    public QuickInputWindow CreateQuickInput(Func<string, Task> submit)
    {
        var inputWindow = new QuickInputWindow(submit);
        var miniBounds = new System.Windows.Rect(
            Left,
            Top,
            ActualWidth > 0 ? ActualWidth : Width,
            ActualHeight > 0 ? ActualHeight : Height);
        var inputPosition = MiniModeLayoutService.GetQuickInputPosition(
            miniBounds,
            new Size(inputWindow.Width, inputWindow.Height),
            GetWorkingArea());

        inputWindow.Left = inputPosition.X;
        inputWindow.Top = inputPosition.Y;
        return inputWindow;
    }

    private System.Windows.Rect GetWorkingArea()
    {
        var monitor = MonitorFromWindow(
            new WindowInteropHelper(this).Handle,
            MonitorDefaultToNearest);

        var monitorInfo = new MonitorInfo
        {
            cbSize = Marshal.SizeOf<MonitorInfo>()
        };

        if (monitor != IntPtr.Zero &&
            GetMonitorInfo(monitor, ref monitorInfo))
        {
            return new System.Windows.Rect(
                monitorInfo.rcWork.Left,
                monitorInfo.rcWork.Top,
                monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
                monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
        }

        return SystemParameters.WorkArea;
    }

    private const int DwmWindowCornerPreferenceAttribute = 33;
    private const int MonitorDefaultToNearest = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
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
        public NativeRect rcMonitor;
        public NativeRect rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(
        IntPtr windowHandle,
        int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(
        IntPtr monitor,
        ref MonitorInfo monitorInfo);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int value,
        int valueSize);
}
