using System.Windows;

namespace nineth1ngs.Views;

public partial class DeleteConfirmationWindow : Window
{
    public DeleteConfirmationWindow(string th1ngText, int subTh1ngCount = 0)
    {
        InitializeComponent();
        DataContext = new DeleteConfirmationViewModel(th1ngText, subTh1ngCount);
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void DeleteClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private sealed class DeleteConfirmationViewModel(string th1ngText, int subTh1ngCount)
    {
        public string Th1ngText { get; } = th1ngText;

        public bool HasSubTh1ngs { get; } = subTh1ngCount > 0;

        public string SubTh1ngMessage { get; } = subTh1ngCount == 1
            ? "This th1ng contains 1 sub-th1ng. It will also be deleted."
            : $"This th1ng contains {subTh1ngCount} sub-th1ngs. They will also be deleted.";
    }
}
