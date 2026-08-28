using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nineth1ngs.Models;
using nineth1ngs.Services;
using System.Windows.Threading;

namespace nineth1ngs.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Th1ngStore store;
    private readonly Func<Th1ng, Task<bool>> confirmDelete;
    private readonly Action<string> showError;
    private readonly DispatcherTimer timer;

    public MainViewModel(
        Th1ngStore store,
        Func<Th1ng, Task<bool>>? confirmDelete = null,
        Action<string>? showError = null)
    {
        this.store = store;
        this.confirmDelete = confirmDelete ?? (_ => Task.FromResult(false));
        this.showError = showError ?? (_ => { });
        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += TimerTick;
        timer.Start();
    }

    public ObservableCollection<Th1ng> OpenTh1ngs { get; } = [];

    public ObservableCollection<Th1ng> DoneTh1ngs { get; } = [];

    [ObservableProperty]
    private string newTh1ngText = string.Empty;

    [ObservableProperty]
    private string selectedSection = "th1ngs";

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

    public async Task LoadAsync()
    {
        try
        {
            var th1ngs = await store.LoadAsync();
            OpenTh1ngs.Clear();
            DoneTh1ngs.Clear();

            foreach (var th1ng in th1ngs)
            {
                AddToSection(th1ng);
            }
        }
        catch (Exception exception)
        {
            ReportError("The th1ngs could not be loaded from local storage.", exception);
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
    private async Task ToggleCompletionAsync(Th1ng th1ng)
    {
        var previousIsCompleted = th1ng.IsCompleted;
        var previousCompletedAt = th1ng.CompletedAt;

        th1ng.IsCompleted = !previousIsCompleted;
        th1ng.CompletedAt = th1ng.IsCompleted ? DateTime.UtcNow : null;

        try
        {
            await store.UpdateAsync(th1ng);
            RefreshSections(th1ng);
        }
        catch
        {
            th1ng.IsCompleted = previousIsCompleted;
            th1ng.CompletedAt = previousCompletedAt;
            ReportError("The completion state could not be saved.");
        }
    }

    [RelayCommand]
    private async Task ToggleTimerAsync(Th1ng th1ng)
    {
        if (th1ng.IsTimerRunning)
        {
            _ = await PauseTimerAsync(th1ng);
            return;
        }

        foreach (var runningTh1ng in OpenTh1ngs.Concat(DoneTh1ngs)
                     .Where(candidate => candidate.IsTimerRunning && !ReferenceEquals(candidate, th1ng)))
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
            OpenTh1ngs.Remove(th1ng);
            DoneTh1ngs.Remove(th1ng);
        }
        catch (Exception exception)
        {
            ReportError("The th1ng could not be deleted.", exception);
        }
    }

    [RelayCommand]
    private void BeginEditing(Th1ng th1ng)
    {
        th1ng.EditText = th1ng.Text;
        th1ng.IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveEditingAsync(Th1ng th1ng)
    {
        var text = th1ng.EditText.Trim();
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

    private void ReportError(string message, Exception? exception = null)
    {
        var details = exception is null ? string.Empty : $"\n\n{exception.Message}";
        showError($"{message}{details}");
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
}
