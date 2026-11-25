using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.Services;
using System.Collections.ObjectModel;

namespace HarjumusteRPG.Mobile.ViewModels;

// One weekday button (E T K N R L P).
public partial class WeekdayItem : ObservableObject
{
    public WeekdayItem(int value)
    {
        Value = value;
        Letter = Weekdays.Letter(value);
    }

    public int Value { get; }
    public string Letter { get; }

    [ObservableProperty]
    private bool isSelected;

    [RelayCommand]
    private void Toggle() => IsSelected = !IsSelected;
}

// "habit-edit" opens a new habit, "habit-edit?id=5" edits habit 5.
[QueryProperty(nameof(HabitId), "id")]
public partial class HabitEditViewModel : BaseViewModel
{
    private readonly IHabitApi _api;
    private readonly IAuthService _auth;

    public HabitEditViewModel(IHabitApi api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
        Title = AppResources.NewHabitTitle;

        foreach (var day in Weekdays.Order)
            Days.Add(new WeekdayItem(day));

        foreach (var option in HabitIcons.Options)
            Icons.Add(new IconChoice(option, PickIcon));
    }

    public ObservableCollection<WeekdayItem> Days { get; } = new();
    public ObservableCollection<IconChoice> Icons { get; } = new();

    private string? _iconKey;

    private void PickIcon(IconChoice choice)
    {
        // tapping the chosen icon again removes it
        _iconKey = _iconKey == choice.Key ? null : choice.Key;
        SyncIcons();
    }

    private void SyncIcons()
    {
        foreach (var icon in Icons)
            icon.IsSelected = icon.Key == _iconKey;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    private string? habitId;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDays))]
    private bool isDaily = true;

    [ObservableProperty]
    private bool isActive = true;

    // First tap on AppResources.Delete asks for confirmation, second tap deletes.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeleteText))]
    private bool confirmDelete;

    public bool IsEditing => int.TryParse(HabitId, out _);
    public bool ShowDays => !IsDaily;
    public string DeleteText => ConfirmDelete ? AppResources.ConfirmDelete : AppResources.Delete;

    partial void OnHabitIdChanged(string? value) => _ = LoadAsync(value);

    private async Task LoadAsync(string? value)
    {
        if (!int.TryParse(value, out var id))
            return;

        Title = AppResources.EditHabitTitle;
        IsBusy = true;
        try
        {
            var result = await _api.GetAsync(id);
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            var habit = result.Value!;
            Name = habit.Name;
            IsDaily = habit.IsDaily;
            IsActive = habit.IsActive;
            foreach (var day in Days)
                day.IsSelected = habit.DaysOfWeek.Contains(day.Value);
            _iconKey = HabitIcons.GetKey(_auth.Username, id);
            SyncIcons();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        // Same rules as in the API.
        var trimmed = Name.Trim();
        if (trimmed.Length is < 2 or > 100)
        {
            ErrorMessage = AppResources.ErrHabitName;
            return;
        }

        var days = Days.Where(d => d.IsSelected).Select(d => d.Value).ToList();
        if (!IsDaily && days.Count == 0)
        {
            ErrorMessage = AppResources.ErrPickWeekday;
            return;
        }

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var request = new HabitRequest(trimmed, IsDaily, IsDaily ? new List<int>() : days, IsActive);
            var result = int.TryParse(HabitId, out var id)
                ? await _api.UpdateAsync(id, request)
                : await _api.CreateAsync(request);

            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            var savedId = IsEditing ? id : result.Value!.Id;
            HabitIcons.Set(_auth.Username, savedId, _iconKey);

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (!int.TryParse(HabitId, out var id))
            return;

        if (!ConfirmDelete)
        {
            ConfirmDelete = true;
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _api.DeleteAsync(id);
            if (!result.Success)
            {
                ConfirmDelete = false;
                await HandleErrorAsync(result.Error!);
                return;
            }
            HabitIcons.Set(_auth.Username, id, null);
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task Cancel() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private Task OpenHistory() => Shell.Current.GoToAsync($"habit-history?id={HabitId}");

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