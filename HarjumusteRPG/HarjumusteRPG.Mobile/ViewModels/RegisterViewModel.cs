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
            await Nav.GoAsync("//pet");
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
        return Nav.GoAsync("//login");
    }

    // Same rules as in the API: username 3-50, email up to 100, password 6-100.
    // Same rules as in the API: username 3-50, email up to 100,
    // password 6-100 with a small letter, a capital letter, a number and a special character.
    private string? Validate()
    {
        var name = Username.Trim();
        var mail = Email.Trim();

        if (name.Length is < 3 or > 50) return AppResources.ErrUsernameLength;
        if (mail.Length == 0) return AppResources.ErrEnterEmail;
        if (mail.Length > 100) return AppResources.ErrEmailTooLong;
        if (!new EmailAddressAttribute().IsValid(mail)) return AppResources.ErrEmailInvalid;
        if (Password.Length is < 6 or > 100) return AppResources.ErrPasswordLength;
        if (!Password.Any(char.IsLower)) return AppResources.ErrPasswordLower;
        if (!Password.Any(char.IsUpper)) return AppResources.ErrPasswordUpper;
        if (!Password.Any(char.IsDigit)) return AppResources.ErrPasswordDigit;
        if (Password.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))) return AppResources.ErrPasswordSpecial;
        if (Password != ConfirmPassword) return AppResources.ErrPasswordsMismatch;
        return null;
    }
}