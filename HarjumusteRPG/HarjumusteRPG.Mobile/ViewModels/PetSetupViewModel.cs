using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

// One animal in the selection grid.
public partial class PetChoice : ObservableObject
{
    public PetChoice(PetOption option)
    {
        ImageKey = option.Image;
        ImageFile = option.Image + ".png";
        Label = option.Image == "kutsikas" ? AppResources.PetDog : AppResources.PetGreyCat;
    }

    public string ImageKey { get; }
    public string ImageFile { get; }
    public string Label { get; }

    [ObservableProperty]
    private bool isSelected;
}

public partial class PetSetupViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    public PetSetupViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Lemmik";

        foreach (var option in PetStore.Options)
            Pets.Add(new PetChoice(option));

        Pets[0].IsSelected = true;
    }

    public ObservableCollection<PetChoice> Pets { get; } = new();

    [ObservableProperty] private string petName = string.Empty;

    [RelayCommand]
    private void Select(PetChoice? pet)
    {
        if (pet is null)
            return;

        foreach (var p in Pets)
            p.IsSelected = p == pet;
    }

    [RelayCommand]
    private async Task Save()
    {
        var name = PetName.Trim();
        if (name.Length is < 2 or > 20)
        {
            ErrorMessage = AppResources.ErrPetName;
            return;
        }

        var chosen = Pets.First(p => p.IsSelected);
        PetStore.Save(_auth.Username, chosen.ImageKey, name);

        ErrorMessage = null;
        await Shell.Current.GoToAsync("//main");
    }
}