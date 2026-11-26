using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class ShopPage : ContentPage
{
    private readonly Grid _root;

    public ShopPage(ShopViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(ShopViewModel.Title));

        var heading = Pixel.Heading(AppResources.NavShop, 16);

        // --- coins the user has ---
        var coins = Pixel.Body(string.Empty, 18, bold: true);
        coins.VerticalOptions = LayoutOptions.Center;
        coins.SetBinding(Label.TextProperty, nameof(ShopViewModel.CoinsText));

        var coinsRow = new HorizontalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Image { Source = "ic_mynt.png", WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center },
                coins
            }
        };

        var hint = Pixel.Body(AppResources.ShopHint, 15, color: Palette.Sub);

        var info = Pixel.Body(string.Empty, 16, bold: true);
        info.SetBinding(Label.TextProperty, nameof(ShopViewModel.InfoMessage));
        info.SetBinding(IsVisibleProperty, nameof(ShopViewModel.HasInfo));

        var error = Pixel.ErrorLabel(nameof(ShopViewModel.ErrorMessage), nameof(ShopViewModel.HasError));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ShopViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(ShopViewModel.IsBusy));

        // --- list of pets ---
        var list = new VerticalStackLayout { Spacing = 12 };
        BindableLayout.SetItemsSource(list, viewModel.Items);
        BindableLayout.SetItemTemplate(list, new DataTemplate(CreateRow));

        var column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 20, 20, 12),
            Spacing = 14,
            Children = { Header.StatsRow(heading), coinsRow, hint, info, error, busy, list }
        };

        _root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center
        };
        _root.Add(new ScrollView { Content = column }, 0, 0);
        _root.Add(NavBar.Build(4), 0, 1);

        Content = _root;
    }

    // One pet: picture, name and a button (buy / choose / active).
    private View CreateRow()
    {
        var picture = new Image
        {
            WidthRequest = 56,
            HeightRequest = 56,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };
        picture.SetBinding(Image.SourceProperty, nameof(ShopItem.ImageFile));

        var name = Pixel.Body(string.Empty, 19, bold: true);
        name.VerticalOptions = LayoutOptions.Center;
        name.SetBinding(Label.TextProperty, nameof(ShopItem.Name));

        var buy = Ui.MakeButton(string.Empty, Palette.Hero, 14);
        buy.SetBinding(Button.TextProperty, nameof(ShopItem.BuyText));
        buy.SetBinding(Button.CommandProperty,
            new Binding(nameof(ShopViewModel.BuyCommand), source: BindingContext));
        buy.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);
        var buyBox = Ui.WithShadow(buy);
        buyBox.SetBinding(IsVisibleProperty, nameof(ShopItem.CanBuy));

        var choose = Ui.MakeButton(AppResources.ChoosePet, Palette.Card, 14);
        choose.SetBinding(Button.CommandProperty,
            new Binding(nameof(ShopViewModel.ChooseCommand), source: BindingContext));
        choose.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);
        var chooseBox = Ui.WithShadow(choose);
        chooseBox.SetBinding(IsVisibleProperty, nameof(ShopItem.CanChoose));

        var activeButton = Ui.MakeButton(AppResources.PetActive, Palette.Mint, 14);
        activeButton.IsEnabled = false;
        var activeBox = Ui.WithShadow(activeButton);
        activeBox.SetBinding(IsVisibleProperty, nameof(ShopItem.IsActive));

        var action = new Grid { VerticalOptions = LayoutOptions.Center, Children = { buyBox, chooseBox, activeBox } };

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };
        row.Add(picture, 0);
        row.Add(name, 1);
        row.Add(action, 2);

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

        if (BindingContext is ShopViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}