using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using nineth1ngs.Models;

namespace nineth1ngs.Views;

public partial class SessionTimeReviewWindow : Window
{
    private readonly SessionTimeReviewViewModel viewModel;
    private bool isUpdatingSelection;

    public SessionTimeReviewWindow(
        int awaySeconds,
        IReadOnlyList<Th1ng> availableTh1ngs,
        int previouslyRunningTh1ngId)
    {
        InitializeComponent();

        viewModel = new SessionTimeReviewViewModel(
            awaySeconds,
            availableTh1ngs,
            previouslyRunningTh1ngId);

        DataContext = viewModel;
    }

    public Th1ng? SelectedTh1ng =>
        viewModel.SelectedTh1ng;

    public string NewTh1ngText =>
        viewModel.NewTh1ngText.Trim();

    private void DiscardClick(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void AddTimeClick(
        object sender,
        RoutedEventArgs e)
    {
        if (viewModel.SelectedTh1ng is null &&
            string.IsNullOrWhiteSpace(viewModel.NewTh1ngText))
        {
            return;
        }

        DialogResult = true;
    }

    private void NewTh1ngTextBoxTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (isUpdatingSelection ||
            string.IsNullOrWhiteSpace(viewModel.NewTh1ngText))
        {
            return;
        }

        isUpdatingSelection = true;

        try
        {
            viewModel.SelectedTh1ng = null;
            Th1ngList.SelectedItem = null;
        }
        finally
        {
            isUpdatingSelection = false;
        }
    }

    private void Th1ngListSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (isUpdatingSelection ||
            Th1ngList.SelectedItem is null)
        {
            return;
        }

        isUpdatingSelection = true;

        try
        {
            viewModel.NewTh1ngText = string.Empty;
            NewTh1ngTextBox.Clear();
        }
        finally
        {
            isUpdatingSelection = false;
        }
    }

    private void NewTh1ngTextBoxKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter ||
            string.IsNullOrWhiteSpace(viewModel.NewTh1ngText))
        {
            return;
        }

        e.Handled = true;
        DialogResult = true;
    }

    private sealed class SessionTimeReviewViewModel
    {
        public SessionTimeReviewViewModel(
            int awaySeconds,
            IReadOnlyList<Th1ng> availableTh1ngs,
            int previouslyRunningTh1ngId)
        {
            AvailableTh1ngs = availableTh1ngs;

            SelectedTh1ng = availableTh1ngs
                .FirstOrDefault(th1ng =>
                    th1ng.Id == previouslyRunningTh1ngId)
                ?? availableTh1ngs.FirstOrDefault();

            AwayTimeText =
                $"Windows was locked for {FormatDuration(awaySeconds)}.";
        }

        public IReadOnlyList<Th1ng> AvailableTh1ngs { get; }

        public Th1ng? SelectedTh1ng { get; set; }

        public string NewTh1ngText { get; set; } = string.Empty;

        public string AwayTimeText { get; }

        private static string FormatDuration(int totalSeconds)
        {
            var duration = TimeSpan.FromSeconds(
                Math.Max(0, totalSeconds));

            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            var seconds = duration.Seconds;

            if (hours > 0)
            {
                return $"{hours} h {minutes} min {seconds} sec";
            }

            return $"{minutes} min {seconds} sec";
        }
    }
}
