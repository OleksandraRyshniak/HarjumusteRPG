using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

// One bar of the chart: Height and Width are in device units.
public record BarItem(string Label, string ValueText, double Height, double Width, bool IsToday);

public partial class StatisticsViewModel : BaseViewModel
{
    private static CultureInfo Estonian => Loc.Culture;
    private const double MaxBarHeight = 130;

    private readonly IAuthService _auth;
    private readonly IHabitApi _habits;
    private readonly IGameApi _game;

    // completions of the last 30 days of all habits (the week is a part of it)
    private List<CompletionInfo> _all = new();

    public StatisticsViewModel(IAuthService auth, IHabitApi habits, IGameApi game)
    {
        _auth = auth;
        _habits = habits;
        _game = game;
        Title = AppResources.NavStats;
    }

    public ObservableCollection<BarItem> Bars { get; } = new();

    // false = week, true = month
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowWeek))]
    private bool showMonth;

    public bool ShowWeek => !ShowMonth;

    [ObservableProperty] private string chartTitle = AppResources.Last7Days;
    [ObservableProperty] private string rangeText = string.Empty;
    [ObservableProperty] private string totalText = "0";
    [ObservableProperty] private string xpText = "0";
    [ObservableProperty] private string daysText = "0";
    [ObservableProperty] private string bestText = "-";
    [ObservableProperty] private bool isEmpty;

    [RelayCommand]
    private void ShowWeekTab()
    {
        ShowMonth = false;
        Rebuild();
    }

    [RelayCommand]
    private void ShowMonthTab()
    {
        ShowMonth = true;
        Rebuild();
    }

    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;

            var page = await _habits.GetPageAsync(null, null, true, false, 1, 50);
            if (!page.Success)
            {
                await HandleErrorAsync(page.Error!);
                return;
            }

            // one history request per habit, all at once
            var results = await Task.WhenAll(
                (page.Value?.Items ?? new()).Select(h => _game.GetHistoryAsync(h.Id, 30)));

            var failed = results.FirstOrDefault(r => !r.Success);
            if (failed is not null)
            {
                await HandleErrorAsync(failed.Error!);
                return;
            }

            _all = results.SelectMany(r => r.Value ?? new()).ToList();
            Rebuild();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Builds the bars and the numbers for the chosen period.
    private void Rebuild()
    {
        var days = ShowMonth ? 30 : 7;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddDays(-(days - 1));

        var perDay = Enumerable.Range(0, days)
            .Select(i => start.AddDays(i))
            .Select(d => (Date: d, Count: _all.Count(c => c.Date == d)))
            .ToList();

        var max = Math.Max(1, perDay.Max(p => p.Count));

        Bars.Clear();
        foreach (var (date, count) in perDay)
        {
            var height = count == 0 ? 4 : 10 + (MaxBarHeight - 10) * count / max;
            Bars.Add(new BarItem(
                ShowMonth ? string.Empty : Weekdays.Letter((int)date.DayOfWeek),
                !ShowMonth && count > 0 ? count.ToString() : string.Empty,
                height,
                ShowMonth ? 6 : 34,
                date == today));
        }

        var inRange = _all.Where(c => c.Date >= start && c.Date <= today).ToList();

        ChartTitle = ShowMonth ? AppResources.Last30Days : AppResources.Last7Days;
        RangeText = $"{Format(start)} - {Format(today)}";
        TotalText = inRange.Count.ToString();
        XpText = inRange.Sum(c => c.XpEarned).ToString();
        DaysText = $"{perDay.Count(p => p.Count > 0)} / {days}";

        var best = perDay.MaxBy(p => p.Count);
        BestText = best.Count == 0 ? "-" : $"{Format(best.Date)} ({best.Count})";

        IsEmpty = inRange.Count == 0;
    }

    private static string Format(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue).ToString("d. MMM", Estonian);

    // Session expired -> back to login, other errors are shown on the screen.
    private async Task HandleErrorAsync(string error)
    {
        if (error.StartsWith(AppResources.ErrSessionExpired))
        {
            _auth.Logout();
            await Task.Delay(300);
            await Shell.Current.GoToAsync("//login");
            return;
        }
        ErrorMessage = error;
    }
}