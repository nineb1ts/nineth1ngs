using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace nineth1ngs.Views;

public partial class QuickInputWindow : Window
{
    private readonly Func<string, Task> submit;
    private bool submitting;

    public QuickInputWindow(Func<string, Task> submit)
    {
        InitializeComponent();
        this.submit = submit;
        Loaded += QuickInputWindowLoaded;
    }

    private void QuickInputWindowLoaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= QuickInputWindowLoaded;
        Dispatcher.BeginInvoke(() =>
        {
            InputTextBox.Focus();
            Keyboard.Focus(InputTextBox);
        });
    }

    private async void InputTextBoxKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
            return;
        }

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await SubmitAsync();
        }
    }

    private async void SubmitClick(
        object sender,
        RoutedEventArgs e)
    {
        await SubmitAsync();
    }

    private async Task SubmitAsync()
    {
        if (submitting)
        {
            return;
        }

        submitting = true;
        IsEnabled = false;

        var text = InputTextBox.Text.Trim();

        if (text.Length > 0)
        {
            await submit(text);
        }

        Close();
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

    private const int DwmWindowCornerPreferenceAttribute = 33;

    [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int value,
        int valueSize);
}