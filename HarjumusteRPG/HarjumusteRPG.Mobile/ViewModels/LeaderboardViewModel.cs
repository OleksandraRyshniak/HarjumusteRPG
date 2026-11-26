using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

// One line of the ranking, one friend and one incoming request.
public record LeaderboardRow(string RankText, string Name, string Details, string Streak, bool IsMe);
public record FriendRow(int Id, string Name, string Details);
public record RequestRow(int Id, string Name);

public partial class LeaderboardViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IFriendApi _api;
    private readonly IRealtimeService _realtime;

    // live reload: if a message arrives while a reload is running, reload once more
    private bool _refreshing;
    private bool _again;

    public LeaderboardViewModel(IAuthService auth, IFriendApi api, IRealtimeService realtime)
    {
        _auth = auth;
        _api = api;
        _realtime = realtime;
        Title = AppResources.NavLeaderboard;
    }

    public ObservableCollection<LeaderboardRow> Board { get; } = new();
    public ObservableCollection<FriendRow> Friends { get; } = new();
    public ObservableCollection<RequestRow> Requests { get; } = new();

    // false = ranking tab, true = friends tab
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowBoard))]
    private bool showFriends;

    public bool ShowBoard => !ShowFriends;

    [ObservableProperty]
    private string newFriendName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInfo))]
    private string? infoMessage;

    public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);

    [ObservableProperty]
    private bool hasRequests;

    [ObservableProperty]
    private bool noFriends;

    // Only the user itself is in the ranking.
    [ObservableProperty]
    private bool onlyMe;

    [RelayCommand]
    private void ShowBoardTab() => ShowFriends = false;

    [RelayCommand]
    private void ShowFriendsTab() => ShowFriends = true;

    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            await ReloadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        if (IsBusy)
            return;

        var name = NewFriendName.Trim();
        if (name.Length < 3)
        {
            InfoMessage = null;
            ErrorMessage = AppResources.ErrFriendName;
            return;
        }

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            InfoMessage = null;

            var result = await _api.SendRequestAsync(name);
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            NewFriendName = string.Empty;
            InfoMessage = Loc.F(AppResources.InviteSentFormat, result.Value!.Username);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Accept(RequestRow? row)
    {
        if (row is null || IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            InfoMessage = null;

            var result = await _api.AcceptAsync(row.Id);
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            InfoMessage = Loc.F(AppResources.NowFriendsFormat, row.Name);
            await ReloadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Decline(RequestRow? row)
    {
        if (row is null || IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            InfoMessage = null;

            var result = await _api.DeclineAsync(row.Id);
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            await ReloadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Remove(FriendRow? row)
    {
        if (row is null || IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            InfoMessage = null;

            var result = await _api.RemoveAsync(row.Id);
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            InfoMessage = Loc.F(AppResources.RemovedFriendFormat, row.Name);
            await ReloadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // The page calls these when it appears and disappears: the live connection is open only
    // while the screen is visible.
    public void StartLive()
    {
        _realtime.LeaderboardChanged -= OnLiveChanged;
        _realtime.LeaderboardChanged += OnLiveChanged;
        _realtime.Start();
    }

    public void StopLive()
    {
        _realtime.LeaderboardChanged -= OnLiveChanged;
        _realtime.Stop();
    }

    private void OnLiveChanged() => _ = LiveReloadAsync();

    private async Task LiveReloadAsync()
    {
        if (_refreshing)
        {
            _again = true;
            return;
        }

        _refreshing = true;
        try
        {
            do
            {
                _again = false;
                await ReloadAsync();
            }
            while (_again);
        }
        finally
        {
            _refreshing = false;
        }
    }

    // Loads the ranking, the friends and the incoming requests.
    private async Task ReloadAsync()
    {
        var board = await _api.GetLeaderboardAsync();
        if (!board.Success)
        {
            await HandleErrorAsync(board.Error!);
            return;
        }

        var friends = await _api.GetFriendsAsync();
        if (!friends.Success)
        {
            await HandleErrorAsync(friends.Error!);
            return;
        }

        var requests = await _api.GetRequestsAsync();
        if (!requests.Success)
        {
            await HandleErrorAsync(requests.Error!);
            return;
        }

        Board.Clear();
        foreach (var e in board.Value ?? new())
            Board.Add(new LeaderboardRow(
                $"{e.Rank}.",
                e.IsMe ? Loc.F(AppResources.YouFormat, e.Username) : e.Username,
                Loc.F(AppResources.LevelXpFormat, e.Level, e.Xp),
                Loc.F(AppResources.DaysShortFormat, e.CurrentStreak),
                e.IsMe));

        Friends.Clear();
        foreach (var f in friends.Value ?? new())
            Friends.Add(new FriendRow(f.Id, f.Username, Loc.F(AppResources.FriendLevelStreakFormat, f.Level, f.CurrentStreak)));

        Requests.Clear();
        foreach (var r in requests.Value ?? new())
            Requests.Add(new RequestRow(r.Id, r.Username));

        OnlyMe = Board.Count <= 1;
        NoFriends = Friends.Count == 0;
        HasRequests = Requests.Count > 0;
    }

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