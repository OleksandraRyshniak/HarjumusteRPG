
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.Services;

using System.Collections.ObjectModel;

namespace HarjumusteRPG.Mobile.ViewModels;

// One pet in the shop.
public partial class ShopItem : ObservableObject
{
    public ShopItem(ShopPet pet, bool isActive)
    {
        Key = pet.Key;
        ImageFile = pet.Key + ".png";
        Name = PetStore.Label(pet.Key);
        BuyText = Loc.F(AppResources.BuyFormat, pet.Price);
        owned = pet.Owned;
        this.isActive = isActive;
    }

    public string Key { get; }
    public string ImageFile { get; }
    public string Name { get; }
    public string BuyText { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBuy), nameof(CanChoose))]
    private bool owned;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChoose))]
    private bool isActive;

    public bool CanBuy => !Owned;
    public bool CanChoose => Owned && !IsActive;
}

public partial class ShopViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IGameApi _api;

    public ShopViewModel(IAuthService auth, IGameApi api)
    {
        _auth = auth;
        _api = api;
        Title = AppResources.NavShop;
    }

    public ObservableCollection<ShopItem> Items { get; } = new();

    [ObservableProperty] private string coinsText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInfo))]
    private string? infoMessage;

    public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);

    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            InfoMessage = null;

            var progress = await _api.GetProgressAsync();
            if (!progress.Success)
            {
                await HandleErrorAsync(progress.Error!);
                return;
            }
            CoinsText = Loc.F(AppResources.ShopCoinsFormat, progress.Value!.Coins);

            var pets = await _api.GetShopPetsAsync();
            if (!pets.Success)
            {
                await HandleErrorAsync(pets.Error!);
                return;
            }

            var active = PetStore.Image(_auth.Username);
            Items.Clear();
            foreach (var pet in pets.Value!)
                Items.Add(new ShopItem(pet, pet.Key + ".png" == active));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Buy(ShopItem? item)
    {
        if (item is null || item.Owned || IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            InfoMessage = null;

            var result = await _api.BuyPetAsync(item.Key);
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            item.Owned = true;
            CoinsText = Loc.F(AppResources.ShopCoinsFormat, result.Value!.Coins);
            InfoMessage = Loc.F(AppResources.PetBoughtFormat, item.Name);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Makes a bought pet the active one.
    [RelayCommand]
    private void Choose(ShopItem? item)
    {
        if (item is null || !item.Owned)
            return;

        PetStore.SetImage(_auth.Username, item.Key);
        foreach (var i in Items)
            i.IsActive = i == item;

        ErrorMessage = null;
        InfoMessage = Loc.F(AppResources.PetChosenFormat, item.Name);
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