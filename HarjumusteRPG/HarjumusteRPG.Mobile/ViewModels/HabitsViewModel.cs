using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

// One row of the habit list.
public record HabitRow(int Id, string Name, string Initial, string Subtitle);

public partial class HabitsViewModel : BaseViewModel
{
    private const int PageSize = 5;

    private readonly IHabitApi _api;
    private readonly IAuthService _auth;

    public HabitsViewModel(IHabitApi api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
        Title = "Harjumused";
    }

    public ObservableCollection<HabitRow> Habits { get; } = new();

    [ObservableProperty]
    private string search = string.Empty;

    // "all", "active" or "inactive"
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllSelected))]
    [NotifyPropertyChangedFor(nameof(ActiveSelected))]
    [NotifyPropertyChangedFor(nameof(InactiveSelected))]
    private string filter = "all";

    // true: by name A-Z, false: newest first
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortText))]
    private bool sortByName = true;

    [ObservableProperty]
    private int page = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string pageText = "Lk 1/1";

    [ObservableProperty]
    private string countText = string.Empty;

    [ObservableProperty]
    private bool isEmpty;

    public bool AllSelected => Filter == "all";
    public bool ActiveSelected => Filter == "active";
    public bool InactiveSelected => Filter == "inactive";
    public string SortText => SortByName ? AppResources.SortName : AppResources.SortNewest;

    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy)
            return;

        var reload = false;
        IsBusy = true;
        try
        {
            reload = await LoadPageAsync();
        }
        finally
        {
            IsBusy = false;
        }

        // The last page became empty (e.g. after a delete): load the new last page.
        if (reload)
            await Load();
    }

    // Returns true when the current page does not exist any more and must be loaded again.
    private async Task<bool> LoadPageAsync()
    {
        ErrorMessage = null;

        bool? active = Filter switch { "active" => true, "inactive" => false, _ => null };
        var result = await _api.GetPageAsync(Search.Trim(), active, SortByName, !SortByName, Page, PageSize);
        if (!result.Success)
        {
            await HandleErrorAsync(result.Error!);
            return false;
        }

        var data = result.Value!;
        TotalPages = Math.Max(1, (int)Math.Ceiling(data.TotalCount / (double)PageSize));
        if (Page > TotalPages)
        {
            Page = TotalPages;
            return true;
        }

        Habits.Clear();
        foreach (var h in data.Items)
            Habits.Add(new HabitRow(h.Id, h.Name, h.Name.Length == 0 ? "?" : h.Name[..1].ToUpperInvariant(), Describe(h)));

        IsEmpty = Habits.Count == 0;
        PageText = Loc.F(AppResources.PageFormat, Page, TotalPages);
        CountText = Loc.F(AppResources.TotalFormat, data.TotalCount);
        return false;
    }

    private static string Describe(HabitInfo h)
    {
        var text = h.IsDaily
            ? AppResources.EveryDay
            : string.Join(", ", Weekdays.Order.Where(h.DaysOfWeek.Contains).Select(Weekdays.Letter));

        return h.IsActive ? text : $"{text} · {AppResources.Paused}";
    }

    [RelayCommand]
    private async Task RunSearch()
    {
        Page = 1;
        await Load();
    }

    [RelayCommand]
    private async Task SetFilter(string? value)
    {
        Filter = value ?? "all";
        Page = 1;
        await Load();
    }

    [RelayCommand]
    private async Task ToggleSort()
    {
        SortByName = !SortByName;
        Page = 1;
        await Load();
    }

    [RelayCommand]
    private async Task NextPage()
    {
        if (Page >= TotalPages)
            return;

        Page++;
        await Load();
    }

    [RelayCommand]
    private async Task PrevPage()
    {
        if (Page <= 1)
            return;

        Page--;
        await Load();
    }

    [RelayCommand]
    private Task Edit(HabitRow? row) =>
        row is null ? Task.CompletedTask : Shell.Current.GoToAsync($"habit-edit?id={row.Id}");

    [RelayCommand]
    private Task Add() => Shell.Current.GoToAsync("habit-edit");

    [RelayCommand]
    private Task Back() => Shell.Current.GoToAsync("//main");

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