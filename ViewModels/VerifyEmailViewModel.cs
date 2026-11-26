using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class VerifyEmailViewModel : ObservableObject
{
    private readonly AuthService _auth;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    public VerifyEmailViewModel(AuthService auth) { _auth = auth; }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        if (Code.Length != 6) { StatusMessage = "Enter 6-digit code."; return; }
        try
        {
            IsBusy = true;
            await _auth.VerifyEmailAsync(Email, Code);
            await Shell.Current.DisplayAlert("Verified", "Email verified. You can now log in.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ResendAsync()
    {
        try
        {
            IsBusy = true;
            await _auth.ResendVerificationAsync(Email);
            StatusMessage = "A new code has been sent.";
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
