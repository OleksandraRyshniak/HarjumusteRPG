using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

public enum DayState { Missed, Today, Done, Future }

// One square of the 4-week calendar.
public record CalendarDay(string Number, DayState State, double Opacity);

// One line of the list of completions.
public record HistoryRow(string DateText, string XpText);

// "habit-history?id=5" shows the history of habit 5.
[QueryProperty(nameof(HabitId), "id")]
public partial class HabitHistoryViewModel : BaseViewModel
{
    private static CultureInfo Estonian => Loc.Culture;

    private readonly IHabitApi _habits;
    private readonly IGameApi _api;
    private readonly IAuthService _auth;

    public HabitHistoryViewModel(IHabitApi habits, IGameApi api, IAuthService auth)
    {
        _habits = habits;
        _api = api;
        _auth = auth;
        Title = AppResources.History;
    }

    public ObservableCollection<CalendarDay> Days { get; } = new();
    public ObservableCollection<HistoryRow> Rows { get; } = new();

    [ObservableProperty]
    private string? habitId;

    [ObservableProperty]
    private string habitName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIcon))]
    private string? iconFile;

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    private bool isEmpty;

    public bool HasIcon => !string.IsNullOrEmpty(IconFile);

    partial void OnHabitIdChanged(string? value) => _ = LoadAsync(value);

    private async Task LoadAsync(string? value)
    {
        if (!int.TryParse(value, out var id))
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;

            var habit = await _habits.GetAsync(id);
            if (!habit.Success)
            {
                await HandleErrorAsync(habit.Error!);
                return;
            }
            HabitName = habit.Value!.Name;
            IconFile = HabitIcons.GetFile(_auth.Username, id);

            // The calendar shows 4 weeks, Monday first, the last row is the current week.
            var today = AppClock.Today;
            var sinceMonday = ((int)today.DayOfWeek + 6) % 7;
            var start = today.AddDays(-sinceMonday - 21);
            var daysBack = today.DayNumber - start.DayNumber + 1;

            var history = await _api.GetHistoryAsync(id, daysBack);
            if (!history.Success)
            {
                await HandleErrorAsync(history.Error!);
                return;
            }

            var done = history.Value!.Select(c => c.Date).ToHashSet();

            Days.Clear();
            for (var i = 0; i < 28; i++)
            {
                var date = start.AddDays(i);
                var state = date > today ? DayState.Future
                          : done.Contains(date) ? DayState.Done
                          : date == today ? DayState.Today
                          : DayState.Missed;
                Days.Add(new CalendarDay(date.Day.ToString(), state, state == DayState.Future ? 0.45 : 1));
            }

            Rows.Clear();
            foreach (var c in history.Value!.OrderByDescending(c => c.Date))
                Rows.Add(new HistoryRow(
                    c.Date.ToDateTime(TimeOnly.MinValue).ToString("dddd, d. MMM", Estonian),
                    $"+{c.XpEarned} XP"));

            Summary = Loc.F(AppResources.HistorySummaryFormat, daysBack, Rows.Count);
            IsEmpty = Rows.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task Back() => Nav.GoAsync("..");

    // Session expired -> back to login, other errors are shown on the screen.
    private async Task HandleErrorAsync(string error)
    {
        if (error.StartsWith(AppResources.ErrSessionExpired))
        {
            _auth.Logout();
            await Task.Delay(300);
            await Nav.GoAsync("//login");
            return;
        }
        ErrorMessage = error;
    }
}