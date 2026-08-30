using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nineth1ngs.Models;
using nineth1ngs.Services;
using System.Windows;
using System.Windows.Threading;

namespace nineth1ngs.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Th1ngStore store;
    private readonly Func<Th1ng, Task<bool>> confirmDelete;
    private readonly Action<string> showError;
    private readonly DispatcherTimer timer;
    private readonly TimeFormattingService timeFormattingService = new();
    private readonly TimeCopySettingsService timeCopySettingsService;
    private readonly TimeCopySettings timeCopySettings;
    private string previousSection = "th1ngs";
    public string RoundUpThresholdHint => $"Choose a value from 1 to {BillingIntervalMinutes - 1} minutes.";

    public MainViewModel(
        Th1ngStore store,
        TimeCopySettingsService timeCopySettingsService,
        Func<Th1ng, Task<bool>>? confirmDelete = null,
        Action<string>? showError = null)
    {
        this.store = store;
        this.timeCopySettingsService = timeCopySettingsService;
        this.confirmDelete = confirmDelete ?? (_ => Task.FromResult(false));
        this.showError = showError ?? (_ => { });

        timeCopySettings = timeCopySettingsService.Load();

        var maximumThreshold = Math.Max(1, timeCopySettings.BillingIntervalMinutes - 1);

        if (timeCopySettings.RoundUpThresholdMinutes < 1 ||
            timeCopySettings.RoundUpThresholdMinutes > maximumThreshold)
        {
            timeCopySettings.RoundUpThresholdMinutes =
                Math.Clamp(timeCopySettings.RoundUpThresholdMinutes, 1, maximumThreshold);

            timeCopySettingsService.Save(timeCopySettings);
        }

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        timer.Tick += TimerTick;
        timer.Start();
    }

    public ObservableCollection<Th1ng> OpenTh1ngs { get; } = [];

    public ObservableCollection<Th1ng> DoneTh1ngs { get; } = [];

    public IReadOnlyList<int> BillingIntervals { get; } =
    [
        15,
        30,
        45,
        60
    ];

    public IReadOnlyList<int> RoundUpThresholds =>
        Enumerable.Range(1, BillingIntervalMinutes - 1).ToList();

    public IReadOnlyList<TimeOutputFormat> OutputFormats { get; } =
    [
        TimeOutputFormat.DecimalHours,
        TimeOutputFormat.HoursAndMinutes
    ];

    [ObservableProperty]
    private string newTh1ngText = string.Empty;

    [ObservableProperty]
    private string selectedSection = "th1ngs";

    public int BillingIntervalMinutes
    {
        get => timeCopySettings.BillingIntervalMinutes;
        set
        {
            if (timeCopySettings.BillingIntervalMinutes == value)
            {
                return;
            }

            timeCopySettings.BillingIntervalMinutes = value;

            var maximumThreshold = value - 1;

            if (timeCopySettings.RoundUpThresholdMinutes > maximumThreshold)
            {
                timeCopySettings.RoundUpThresholdMinutes = maximumThreshold;
                OnPropertyChanged(nameof(RoundUpThresholdMinutes));
            }

            timeCopySettingsService.Save(timeCopySettings);

            OnPropertyChanged();
            OnPropertyChanged(nameof(RoundUpThresholds));
            OnPropertyChanged(nameof(RoundUpThresholdHint));
            OnPropertyChanged(nameof(TimeCopyExample));
        }
    }

    public int RoundUpThresholdMinutes
    {
        get => timeCopySettings.RoundUpThresholdMinutes;
        set
        {
            if (value < 1 || value >= BillingIntervalMinutes)
            {
                return;
            }

            if (timeCopySettings.RoundUpThresholdMinutes == value)
            {
                return;
            }

            timeCopySettings.RoundUpThresholdMinutes = value;
            timeCopySettingsService.Save(timeCopySettings);
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimeCopyExample));
        }
    }

    public TimeOutputFormat OutputFormat
    {
        get => timeCopySettings.OutputFormat;
        set
        {
            if (timeCopySettings.OutputFormat == value)
            {
                return;
            }

            timeCopySettings.OutputFormat = value;
            timeCopySettingsService.Save(timeCopySettings);
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimeCopyExample));
        }
    }

    public string TimeCopyExample => $"1 h 17 min tracked → {timeFormattingService.FormatForCopy(77 * 60, timeCopySettings)}";

    [RelayCommand]
    private void ShowOpen()
    {
        SelectedSection = "th1ngs";
    }

    [RelayCommand]
    private void ShowDone()
    {
        SelectedSection = "done";
    }

    [RelayCommand]
    private void ShowSettings()
    {
        previousSection = SelectedSection;
        SelectedSection = "settings";
    }

    [RelayCommand]
    private void BackFromSettings()
    {
        SelectedSection = previousSection;
    }

    public async Task LoadAsync()
    {
        try
        {
            var th1ngs = await store.LoadAsync();

            OpenTh1ngs.Clear();
            DoneTh1ngs.Clear();

            var topLevelTh1ngs = th1ngs
                .Where(th1ng => !th1ng.ParentId.HasValue)
                .OrderByDescending(th1ng => th1ng.CreatedAt)
                .ToList();

            var topLevelIds = topLevelTh1ngs
                .Select(th1ng => th1ng.Id)
                .ToHashSet();

            var subTh1ngsByParent = th1ngs
                .Where(th1ng =>
                    th1ng.ParentId.HasValue &&
                    topLevelIds.Contains(th1ng.ParentId.Value))
                .GroupBy(th1ng => th1ng.ParentId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(th1ng => th1ng.CreatedAt)
                        .ToList());

            foreach (var th1ng in topLevelTh1ngs)
            {
                if (subTh1ngsByParent.TryGetValue(
                        th1ng.Id,
                        out var subTh1ngs))
                {
                    foreach (var subTh1ng in subTh1ngs)
                    {
                        th1ng.SubTh1ngs.Add(subTh1ng);
                    }
                }

                AddToSection(th1ng);
            }
        }
        catch (Exception exception)
        {
            ReportError(
                "The th1ngs could not be loaded from local storage.",
                exception);
        }
    }

    [RelayCommand]
    private async Task AddTh1ngAsync()
    {
        var text = NewTh1ngText.Trim();

        if (text.Length == 0)
        {
            return;
        }

        var th1ng = new Th1ng
        {
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await store.AddAsync(th1ng);

            OpenTh1ngs.Insert(0, th1ng);
            NewTh1ngText = string.Empty;
        }
        catch (Exception exception)
        {
            ReportError("The th1ng could not be saved.", exception);
        }
    }

    [RelayCommand]
    private async Task AddSubTh1ngAsync(Th1ng parent)
    {
        if (SelectedSection == "done" || parent.ParentId.HasValue)
        {
            return;
        }

        var text = parent.NewSubTh1ngText.Trim();

        if (text.Length == 0)
        {
            return;
        }

        var subTh1ng = new Th1ng
        {
            Text = text,
            CreatedAt = DateTime.UtcNow,
            ParentId = parent.Id
        };

        try
        {
            await store.AddAsync(subTh1ng);

            parent.SubTh1ngs.Insert(0, subTh1ng);
            parent.NewSubTh1ngText = string.Empty;
            parent.IsAddingSubTh1ng = false;
            parent.IsExpanded = true;
        }
        catch (Exception exception)
        {
            ReportError("The sub-th1ng could not be saved.", exception);
        }
    }

    [RelayCommand]
    private void BeginEditing(Th1ng th1ng)
    {
        if (SelectedSection == "done")
        {
            return;
        }

        th1ng.EditText = th1ng.Text;
        th1ng.IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveEditingAsync(Th1ng th1ng)
    {
        if (SelectedSection == "done")
        {
            th1ng.EditText = th1ng.Text;
            th1ng.IsEditing = false;
            return;
        }

        var text = th1ng.EditText.Trim();

        if (text.Length == 0 && th1ng.ParentId.HasValue)
        {
            try
            {
                await store.DeleteAsync(th1ng);
                FindParent(th1ng)?.SubTh1ngs.Remove(th1ng);
            }
            catch (Exception exception)
            {
                ReportError(
                    "The sub-th1ng could not be deleted.",
                    exception);
            }

            return;
        }

        if (text.Length == 0)
        {
            return;
        }

        var previousText = th1ng.Text;
        th1ng.Text = text;

        try
        {
            await store.UpdateAsync(th1ng);
            th1ng.IsEditing = false;
        }
        catch
        {
            th1ng.Text = previousText;
            ReportError("The edited th1ng could not be saved.");
        }
    }

    [RelayCommand]
    private static void CancelEditing(Th1ng th1ng)
    {
        th1ng.EditText = th1ng.Text;
        th1ng.IsEditing = false;
    }

    [RelayCommand]
    private void BeginAddSubTh1ng(Th1ng parent)
    {
        if (SelectedSection == "done" || parent.ParentId.HasValue)
        {
            return;
        }

        parent.IsExpanded = true;
        parent.IsAddingSubTh1ng = true;
    }

    [RelayCommand]
    private static void CancelAddSubTh1ng(Th1ng parent)
    {
        parent.NewSubTh1ngText = string.Empty;
        parent.IsAddingSubTh1ng = false;
    }

    [RelayCommand]
    private async Task ToggleCompletionAsync(Th1ng th1ng)
    {
        var previousIsCompleted = th1ng.IsCompleted;
        var previousCompletedAt = th1ng.CompletedAt;
        var previousElapsedSeconds = th1ng.ElapsedSeconds;
        var previousTimerStartedAt = th1ng.TimerStartedAt;

        th1ng.IsCompleted = !previousIsCompleted;
        th1ng.CompletedAt = th1ng.IsCompleted
            ? DateTime.UtcNow
            : null;

        if (th1ng.IsCompleted && th1ng.IsTimerRunning)
        {
            th1ng.ElapsedSeconds = th1ng.GetElapsedSeconds();
            th1ng.TimerStartedAt = null;
            th1ng.RefreshTimerDisplay();
        }

        try
        {
            await store.UpdateAsync(th1ng);

            if (!th1ng.ParentId.HasValue)
            {
                RefreshSections(th1ng);
            }
        }
        catch
        {
            th1ng.IsCompleted = previousIsCompleted;
            th1ng.CompletedAt = previousCompletedAt;
            th1ng.ElapsedSeconds = previousElapsedSeconds;
            th1ng.TimerStartedAt = previousTimerStartedAt;
            th1ng.RefreshTimerDisplay();

            ReportError("The completion state could not be saved.");
        }
    }

    [RelayCommand]
    private async Task ToggleTimerAsync(Th1ng th1ng)
    {
        if (th1ng.ParentId.HasValue || th1ng.IsCompleted)
        {
            return;
        }

        if (th1ng.IsTimerRunning)
        {
            _ = await PauseTimerAsync(th1ng);
            return;
        }

        foreach (var runningTh1ng in OpenTh1ngs
                     .Concat(DoneTh1ngs)
                     .Where(candidate =>
                         candidate.IsTimerRunning &&
                         !ReferenceEquals(candidate, th1ng)))
        {
            if (!await PauseTimerAsync(runningTh1ng))
            {
                return;
            }
        }

        var previousTimerStartedAt = th1ng.TimerStartedAt;
        th1ng.TimerStartedAt = DateTime.UtcNow;

        try
        {
            await store.UpdateAsync(th1ng);
        }
        catch (Exception exception)
        {
            th1ng.TimerStartedAt = previousTimerStartedAt;
            ReportError("The timer could not be started.", exception);
        }
    }

    public async Task PauseRunningTimersAsync()
    {
        foreach (var th1ng in OpenTh1ngs
                     .Concat(DoneTh1ngs)
                     .Where(th1ng => th1ng.IsTimerRunning))
        {
            await PauseTimerAsync(th1ng);
        }
    }

    [RelayCommand]
    private async Task CopyTrackedTimeAsync(Th1ng th1ng)
    {
        try
        {
            var elapsedSeconds = th1ng.GetElapsedSeconds();

            var formattedTime = timeFormattingService.FormatForCopy(
                elapsedSeconds,
                timeCopySettings);

            Clipboard.SetText(formattedTime);

            th1ng.IsTimeCopied = true;
            th1ng.TimeCopyToolTip = $"Copied {formattedTime}";

            await Task.Delay(1200);

            th1ng.IsTimeCopied = false;
            th1ng.TimeCopyToolTip = "Copy tracked time";
        }
        catch (Exception exception)
        {
            ReportError(
                "The tracked time could not be copied.",
                exception);
        }
    }

    [RelayCommand]
    private void CopyTh1ngText(Th1ng th1ng)
    {
        if (string.IsNullOrWhiteSpace(th1ng.Text))
        {
            return;
        }

        try
        {
            Clipboard.SetText(th1ng.Text);
        }
        catch (Exception exception)
        {
            ReportError(
                "The th1ng text could not be copied.",
                exception);
        }
    }

    [RelayCommand]
    private async Task DeleteTh1ngAsync(Th1ng th1ng)
    {
        if (!await confirmDelete(th1ng))
        {
            return;
        }

        try
        {
            await store.DeleteAsync(th1ng);

            if (th1ng.ParentId.HasValue)
            {
                FindParent(th1ng)?.SubTh1ngs.Remove(th1ng);
            }
            else
            {
                OpenTh1ngs.Remove(th1ng);
                DoneTh1ngs.Remove(th1ng);
            }
        }
        catch (Exception exception)
        {
            ReportError("The th1ng could not be deleted.", exception);
        }
    }

    [RelayCommand]
    private async Task DeleteSubTh1ngAsync(Th1ng subTh1ng)
    {
        if (!subTh1ng.ParentId.HasValue)
        {
            return;
        }

        try
        {
            await store.DeleteAsync(subTh1ng);
            FindParent(subTh1ng)?.SubTh1ngs.Remove(subTh1ng);
        }
        catch (Exception exception)
        {
            ReportError(
                "The sub-th1ng could not be deleted.",
                exception);
        }
    }

    private async Task<bool> PauseTimerAsync(Th1ng th1ng)
    {
        var previousElapsedSeconds = th1ng.ElapsedSeconds;
        var previousTimerStartedAt = th1ng.TimerStartedAt;

        th1ng.ElapsedSeconds = th1ng.GetElapsedSeconds();
        th1ng.TimerStartedAt = null;

        try
        {
            await store.UpdateAsync(th1ng);
            return true;
        }
        catch (Exception exception)
        {
            th1ng.ElapsedSeconds = previousElapsedSeconds;
            th1ng.TimerStartedAt = previousTimerStartedAt;

            ReportError("The timer could not be paused.", exception);
            return false;
        }
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        foreach (var th1ng in OpenTh1ngs.Concat(DoneTh1ngs))
        {
            if (th1ng.IsTimerRunning)
            {
                th1ng.RefreshTimerDisplay();
            }
        }
    }

    private void AddToSection(Th1ng th1ng)
    {
        if (th1ng.IsCompleted)
        {
            DoneTh1ngs.Add(th1ng);
            return;
        }

        OpenTh1ngs.Add(th1ng);
    }

    private void RefreshSections(Th1ng th1ng)
    {
        OpenTh1ngs.Remove(th1ng);
        DoneTh1ngs.Remove(th1ng);
        AddToSection(th1ng);
    }

    private Th1ng? FindParent(Th1ng subTh1ng) =>
        OpenTh1ngs
            .Concat(DoneTh1ngs)
            .FirstOrDefault(parent =>
                parent.SubTh1ngs.Contains(subTh1ng));

    private void ReportError(
        string message,
        Exception? exception = null)
    {
        var details = exception is null
            ? string.Empty
            : $"\n\n{exception.Message}";

        showError($"{message}{details}");
    }
}
