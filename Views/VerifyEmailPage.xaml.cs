using PAGELY.ViewModels;

namespace PAGELY.Views;

[QueryProperty(nameof(Email), "email")]
public partial class VerifyEmailPage : ContentPage
{
    public string Email { get; set; } = string.Empty;
    public VerifyEmailPage(VerifyEmailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is VerifyEmailViewModel vm && !string.IsNullOrWhiteSpace(Email))
            vm.Email = Uri.UnescapeDataString(Email);
    }
    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
