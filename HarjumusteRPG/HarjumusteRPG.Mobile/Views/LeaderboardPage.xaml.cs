using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class LeaderboardPage : ContentPage
{
    private readonly Grid _root;

    public LeaderboardPage(LeaderboardViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(LeaderboardViewModel.Title));

        var heading = Pixel.Heading(AppResources.LeaderboardTitle, 16);

        // --- two tabs: ranking / friends ---
        var tabs = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
        };
        tabs.Add(Ui.CommandButton(AppResources.NavLeaderboard, nameof(LeaderboardViewModel.ShowBoardTabCommand),
            selectedPath: nameof(LeaderboardViewModel.ShowBoard)), 0);
        tabs.Add(Ui.CommandButton(AppResources.TabFriends, nameof(LeaderboardViewModel.ShowFriendsTabCommand),
            selectedPath: nameof(LeaderboardViewModel.ShowFriends)), 1);

        var error = Pixel.ErrorLabel(nameof(LeaderboardViewModel.ErrorMessage), nameof(LeaderboardViewModel.HasError));

        var info = Pixel.Body(string.Empty, 16, bold: true, color: Palette.Sub);
        info.SetBinding(Label.TextProperty, nameof(LeaderboardViewModel.InfoMessage));
        info.SetBinding(IsVisibleProperty, nameof(LeaderboardViewModel.HasInfo));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(LeaderboardViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(LeaderboardViewModel.IsBusy));

        // --- ranking tab ---
        var board = new VerticalStackLayout { Spacing = 12 };
        BindableLayout.SetItemsSource(board, viewModel.Board);
        BindableLayout.SetItemTemplate(board, new DataTemplate(CreateBoardRow));

        var onlyMe = Pixel.Body(AppResources.OnlyMeHint, 16, color: Palette.Sub);
        onlyMe.SetBinding(IsVisibleProperty, nameof(LeaderboardViewModel.OnlyMe));

        var boardSection = new VerticalStackLayout { Spacing = 12, Children = { board, onlyMe } };
        boardSection.SetBinding(IsVisibleProperty, nameof(LeaderboardViewModel.ShowBoard));

        // --- friends tab ---
        var addTitle = Pixel.Heading(AppResources.AddFriendTitle, 12);
        var nameField = Pixel.Field(AppResources.FriendNameField, nameof(LeaderboardViewModel.NewFriendName),
            returnType: ReturnType.Done, returnCommandPath: nameof(LeaderboardViewModel.SendRequestCommand));
        var sendButton = Pixel.PrimaryButton(AppResources.SendInvite, nameof(LeaderboardViewModel.SendRequestCommand));

        var requestsTitle = Pixel.Heading(AppResources.InvitesTitle, 12);
        var requests = new VerticalStackLayout { Spacing = 12 };
        BindableLayout.SetItemsSource(requests, viewModel.Requests);
        BindableLayout.SetItemTemplate(requests, new DataTemplate(CreateRequestRow));

        var requestsSection = new VerticalStackLayout { Spacing = 12, Children = { requestsTitle, requests } };
        requestsSection.SetBinding(IsVisibleProperty, nameof(LeaderboardViewModel.HasRequests));

        var friendsTitle = Pixel.Heading(AppResources.FriendsTitle, 12);
        var friends = new VerticalStackLayout { Spacing = 12 };
        BindableLayout.SetItemsSource(friends, viewModel.Friends);
        BindableLayout.SetItemTemplate(friends, new DataTemplate(CreateFriendRow));

        var noFriends = Pixel.Body(AppResources.NoFriends, 16, color: Palette.Sub);
        noFriends.SetBinding(IsVisibleProperty, nameof(LeaderboardViewModel.NoFriends));

        var friendsSection = new VerticalStackLayout
        {
            Spacing = 14,
            Children = { addTitle, nameField, sendButton, requestsSection, friendsTitle, friends, noFriends }
        };
        friendsSection.SetBinding(IsVisibleProperty, nameof(LeaderboardViewModel.ShowFriends));

        var column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 20, 20, 12),
            Spacing = 14,
            Children = { Header.Row(heading), tabs, error, info, busy, boardSection, friendsSection }
        };

        _root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center
        };
        _root.Add(new ScrollView { Content = column }, 0, 0);
        _root.Add(NavBar.Build(3), 0, 1);

        Content = _root;
    }

    // One ranking line: place, name with details and streak. The user's own line is highlighted.
    private static View CreateBoardRow()
    {
        var rank = Pixel.Heading(string.Empty, 12, center: true);
        rank.WidthRequest = 48;
        rank.VerticalOptions = LayoutOptions.Center;
        rank.SetBinding(Label.TextProperty, nameof(LeaderboardRow.RankText));

        var name = Pixel.Body(string.Empty, 19, bold: true);
        name.SetBinding(Label.TextProperty, nameof(LeaderboardRow.Name));

        var details = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        details.SetBinding(Label.TextProperty, nameof(LeaderboardRow.Details));

        var texts = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { name, details }
        };

        var streak = Pixel.Body(string.Empty, 16, bold: true);
        streak.VerticalOptions = LayoutOptions.Center;
        streak.SetBinding(Label.TextProperty, nameof(LeaderboardRow.Streak));

        var streakBox = new HorizontalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Image { Source = "ic_tuli.png", WidthRequest = 20, HeightRequest = 20, VerticalOptions = LayoutOptions.Center },
                streak
            }
        };

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8
        };
        row.Add(rank, 0);
        row.Add(texts, 1);
        row.Add(streakBox, 2);

        var card = Pixel.Card(row, padding: new Thickness(12));
        ((Border)card.Children[1]).SetBinding(BackgroundColorProperty,
            new Binding(nameof(LeaderboardRow.IsMe), converter: new BoolToColorConverter(Palette.Hero, Palette.Card)));
        return card;
    }

    // One friend: name, level and a button to remove.
    private View CreateFriendRow()
    {
        var name = Pixel.Body(string.Empty, 19, bold: true);
        name.SetBinding(Label.TextProperty, nameof(FriendRow.Name));

        var details = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        details.SetBinding(Label.TextProperty, nameof(FriendRow.Details));

        var texts = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { name, details }
        };

        var remove = Ui.MakeButton(AppResources.Remove, Palette.Card, 14);
        remove.SetBinding(Button.CommandProperty,
            new Binding(nameof(LeaderboardViewModel.RemoveCommand), source: BindingContext));
        remove.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        row.Add(texts, 0);
        row.Add(Ui.WithShadow(remove), 1);

        return Pixel.Card(row, padding: new Thickness(12));
    }

    // One incoming request: name and buttons Jah / Ei.
    private View CreateRequestRow()
    {
        var name = Pixel.Body(string.Empty, 19, bold: true);
        name.VerticalOptions = LayoutOptions.Center;
        name.SetBinding(Label.TextProperty, nameof(RequestRow.Name));

        var accept = Ui.MakeButton(AppResources.Yes, Palette.Hero, 14);
        accept.SetBinding(Button.CommandProperty,
            new Binding(nameof(LeaderboardViewModel.AcceptCommand), source: BindingContext));
        accept.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);

        var decline = Ui.MakeButton(AppResources.No, Palette.Card, 14);
        decline.SetBinding(Button.CommandProperty,
            new Binding(nameof(LeaderboardViewModel.DeclineCommand), source: BindingContext));
        decline.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);

        var buttons = new HorizontalStackLayout
        {
            Spacing = 8,
            Children = { Ui.WithShadow(accept), Ui.WithShadow(decline) }
        };

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        row.Add(name, 0);
        row.Add(buttons, 1);

        return Pixel.Card(row, padding: new Thickness(12));
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _root.WidthRequest = Math.Min(width, 420);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LeaderboardViewModel vm)
        {
            vm.LoadCommand.Execute(null);
            vm.StartLive();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is LeaderboardViewModel vm)
            vm.StopLive();
    } 
}