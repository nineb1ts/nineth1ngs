using System.Windows;

namespace nineth1ngs.Views;

public partial class DeleteConfirmationWindow : Window
{
    public DeleteConfirmationWindow(string th1ngText)
    {
        InitializeComponent();
        DataContext = new DeleteConfirmationViewModel(th1ngText);
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void DeleteClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private sealed class DeleteConfirmationViewModel(string th1ngText)
    {
        public string Th1ngText { get; } = th1ngText;
    }
}
