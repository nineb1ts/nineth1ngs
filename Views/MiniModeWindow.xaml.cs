using System.Windows;
using System.Windows.Input;
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
}
