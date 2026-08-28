using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace nineth1ngs.Models;

public partial class Th1ng : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private bool isCompleted;

    [ObservableProperty]
    private DateTime createdAt;

    [ObservableProperty]
    private DateTime? completedAt;

    [ObservableProperty]
    private int elapsedSeconds;

    [ObservableProperty]
    private DateTime? timerStartedAt;

    private bool isEditing;

    private string editText = string.Empty;

    [NotMapped]
    public bool IsEditing
    {
        get => isEditing;
        set => SetProperty(ref isEditing, value);
    }

    [NotMapped]
    public string EditText
    {
        get => editText;
        set => SetProperty(ref editText, value);
    }

    [NotMapped]
    public bool IsTimerRunning => TimerStartedAt.HasValue;

    [NotMapped]
    public string ElapsedTimeText => FormatElapsedTime(GetElapsedSeconds());

    public int GetElapsedSeconds(DateTime? nowUtc = null)
    {
        var runningSeconds = TimerStartedAt.HasValue
            ? Math.Max(0, (int)(AsUtc(nowUtc ?? DateTime.UtcNow) - AsUtc(TimerStartedAt.Value)).TotalSeconds)
            : 0;

        return ElapsedSeconds + runningSeconds;
    }

    public void RefreshTimerDisplay()
    {
        OnPropertyChanged(nameof(ElapsedTimeText));
    }

    partial void OnElapsedSecondsChanged(int value)
    {
        OnPropertyChanged(nameof(ElapsedTimeText));
    }

    partial void OnTimerStartedAtChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(IsTimerRunning));
        OnPropertyChanged(nameof(ElapsedTimeText));
    }

    private static string FormatElapsedTime(int totalSeconds)
    {
        var time = TimeSpan.FromSeconds(totalSeconds);
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
            : time.ToString(@"m\:ss");
    }

    private static DateTime AsUtc(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
