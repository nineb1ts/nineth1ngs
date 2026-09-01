using nineth1ngs.Models;
using nineth1ngs.Services;
using nineth1ngs.ViewModels;

namespace nineth1ngs.Tests;

public sealed class MiniModeViewModelTests
{
    [Fact]
    public void SelectNextMiniTh1ng_CyclesThroughOpenTh1ngs()
    {
        var viewModel = CreateViewModel();
        var first = new Th1ng { Text = "First" };
        var second = new Th1ng { Text = "Second" };

        viewModel.OpenTh1ngs.Add(first);
        viewModel.OpenTh1ngs.Add(second);
        viewModel.SelectMiniTh1ng(first);

        viewModel.SelectNextMiniTh1ng();
        Assert.Same(second, viewModel.MiniDisplayedTh1ng);

        viewModel.SelectNextMiniTh1ng();
        Assert.Same(first, viewModel.MiniDisplayedTh1ng);
    }

    [Fact]
    public async Task AddTopLevelTh1ngAsync_WithEmptyTextCreatesNothing()
    {
        var viewModel = CreateViewModel();

        var result = await viewModel.AddTopLevelTh1ngAsync("  ");

        Assert.Null(result);
        Assert.Empty(viewModel.OpenTh1ngs);
    }

    [Fact]
    public async Task ToggleSelectedMiniTimerAsync_StartsAndStopsSelectedTimer()
    {
        var viewModel = CreateViewModel();
        var th1ng = new Th1ng { Text = "Tracked" };

        viewModel.OpenTh1ngs.Add(th1ng);
        viewModel.SelectMiniTh1ng(th1ng);

        await viewModel.ToggleSelectedMiniTimerAsync();
        Assert.True(th1ng.IsTimerRunning);

        await viewModel.ToggleSelectedMiniTimerAsync();
        Assert.False(th1ng.IsTimerRunning);
    }

    private static MainViewModel CreateViewModel() =>
        new(
            new Th1ngStore(),
            new TimeCopySettingsService(),
            updateTh1ng: _ => Task.CompletedTask);
}