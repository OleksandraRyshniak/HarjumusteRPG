using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Login";
    }

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    // If a saved session exists, skip the login screen.
    private bool _restoring;

    // If a saved session exists, skip the login screen.
    [RelayCommand]
    private async Task RestoreSession()
    {
        // OnAppearing can fire twice on Windows: do not navigate twice at the same time.
        if (_restoring)
            return;

        _restoring = true;
        try
        {
            if (await _auth.RestoreSessionAsync())
                await Shell.Current.GoToAsync("//main");
        }
        finally
        {
            _restoring = false;
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = null;

        var mail = Email.Trim();
        if (mail.Length == 0)
        {
            ErrorMessage = "Sisesta e-posti aadress.";
            return;
        }
        if (!new EmailAddressAttribute().IsValid(mail))
        {
            ErrorMessage = "E-posti aadress ei ole korrektne.";
            return;
        }
        if (Password.Length == 0)
        {
            ErrorMessage = "Sisesta parool.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _auth.LoginAsync(mail, Password);
            if (!result.Success)
            {
                ErrorMessage = result.Error;
                return;
            }

            Password = string.Empty;
            await Shell.Current.GoToAsync("//main");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task GoToRegister()
    {
        ErrorMessage = null;
        return Shell.Current.GoToAsync("//register");
    }
}