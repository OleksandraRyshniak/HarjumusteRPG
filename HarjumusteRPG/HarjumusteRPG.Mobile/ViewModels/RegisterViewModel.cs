using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    public RegisterViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Register";
    }

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [RelayCommand]
    private async Task Register()
    {
        ErrorMessage = Validate();
        if (ErrorMessage is not null)
            return;

        IsBusy = true;
        try
        {
            var result = await _auth.RegisterAsync(Username.Trim(), Email.Trim(), Password);
            if (!result.Success)
            {
                ErrorMessage = result.Error;
                return;
            }

            Password = string.Empty;
            ConfirmPassword = string.Empty;
            await Shell.Current.GoToAsync("//pet");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task GoToLogin()
    {
        ErrorMessage = null;
        return Shell.Current.GoToAsync("//login");
    }

    // Same rules as in the API: username 3-50, email up to 100, password 6-100.
    private string? Validate()
    {
        var name = Username.Trim();
        var mail = Email.Trim();

        if (name.Length is < 3 or > 50) return "Kasutajanimi peab olema 3–50 märki.";
        if (mail.Length == 0) return "Sisesta e-posti aadress.";
        if (mail.Length > 100) return "E-post võib olla kuni 100 märki.";
        if (!new EmailAddressAttribute().IsValid(mail)) return "E-posti aadress ei ole korrektne.";
        if (Password.Length is < 6 or > 100) return "Parool peab olema 6–100 märki.";
        if (Password != ConfirmPassword) return "Paroolid ei kattu.";
        return null;
    }
}