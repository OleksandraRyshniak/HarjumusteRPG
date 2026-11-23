using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

// One icon in the picker of the habit screen.
public partial class IconChoice : ObservableObject
{
    private readonly Action<IconChoice> _pick;

    public IconChoice(HabitIconOption option, Action<IconChoice> pick)
    {
        Key = option.Key;
        ImageFile = option.File;
        _pick = pick;
    }

    public string Key { get; }
    public string ImageFile { get; }

    [ObservableProperty]
    private bool isSelected;

    [RelayCommand]
    private void Pick() => _pick(this);
}