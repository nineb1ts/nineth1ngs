using System.Windows;
using nineth1ngs.Models;

namespace nineth1ngs.Views;

public partial class SessionTimeReviewWindow : Window
{
    private readonly SessionTimeReviewViewModel viewModel;

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
        if (viewModel.SelectedTh1ng is null)
        {
            return;
        }

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

        public string AwayTimeText { get; }

        private static string FormatDuration(int totalSeconds)
        {
            var duration = TimeSpan.FromSeconds(totalSeconds);

            if (duration.TotalHours >= 1)
            {
                var hours = (int)duration.TotalHours;

                return duration.Minutes == 0
                    ? $"{hours} h"
                    : $"{hours} h {duration.Minutes} min";
            }

            return $"{Math.Max(1, (int)duration.TotalMinutes)} min";
        }
    }
}
