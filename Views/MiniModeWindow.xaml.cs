using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using nineth1ngs.ViewModels;

namespace nineth1ngs.Views;

public partial class MiniModeWindow : Window
{
    private readonly Action returnToNormal;

    public MiniModeWindow(
        MainViewModel viewModel,
        Action returnToNormal)
    {
        InitializeComponent();

        DataContext = viewModel;
        this.returnToNormal = returnToNormal;
        viewModel.SelectMiniTh1ng(viewModel.ActiveTh1ng);
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

    private void MiniHeaderMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private const int DwmWindowCornerPreferenceAttribute = 33;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int value,
        int valueSize);
}
