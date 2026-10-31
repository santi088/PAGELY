using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly AuthService _auth;

    public MyReviewsViewModel MyReviews { get; }
    public StatsViewModel Stats { get; }
    public ReadingListViewModel ReadingList { get; }
    public HistoryViewModel History { get; }
    public SettingsViewModel Settings { get; }

    public IReadOnlyList<object> Tabs { get; }

    [ObservableProperty]
    private int selectedTab;

    public ProfileViewModel(
        AuthService auth,
        MyReviewsViewModel myReviews,
        StatsViewModel stats,
        ReadingListViewModel readingList,
        HistoryViewModel history,
        SettingsViewModel settings)
    {
        _auth = auth;
        MyReviews = myReviews;
        Stats = stats;
        ReadingList = readingList;
        History = history;
        Settings = settings;
        Tabs = new object[] { this, readingList, history, settings };
    }

    public bool IsAuthenticated => _auth.IsAuthenticated;

    [ObservableProperty]
    private string loginEmail = string.Empty;

    [ObservableProperty]
    private string loginPassword = string.Empty;

    [ObservableProperty]
    private string registerEmail = string.Empty;

    [ObservableProperty]
    private string registerPassword = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string accountEmail = string.Empty;

    public async Task InitializeAsync()
    {
        if (_auth.IsAuthenticated)
        {
            AccountEmail = _auth.Email ?? string.Empty;
            StatusMessage = $"Signed in as {AccountEmail}";
        }
        else
        {
            AccountEmail = string.Empty;
            StatusMessage = "Not signed in. Reading list and reviews require an account.";
        }

        OnPropertyChanged(nameof(IsAuthenticated));

        try
        {
            await MyReviews.LoadCommand.ExecuteAsync(null);
        }
        catch { }
        try
        {
            await Stats.LoadCommand.ExecuteAsync(null);
        }
        catch { }
        try
        {
            await ReadingList.LoadCommand.ExecuteAsync(null);
        }
        catch { }
        try
        {
            await History.LoadCommand.ExecuteAsync(null);
        }
        catch { }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            StatusMessage = "Enter email and password.";
            return;
        }

        try
        {
            IsBusy = true;
            await _auth.LoginAsync(LoginEmail, LoginPassword);
            await InitializeAsync();
            LoginPassword = string.Empty;
            await Shell.Current.DisplayAlert("Account", "Signed in successfully.", "OK");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Login failed: {ex.Message}";
            await Shell.Current.DisplayAlert("Login failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(RegisterEmail) || string.IsNullOrWhiteSpace(RegisterPassword))
        {
            StatusMessage = "Enter email and password.";
            return;
        }

        try
        {
            IsBusy = true;
            await _auth.RegisterAsync(RegisterEmail, RegisterPassword, FirstName, LastName);
            await InitializeAsync();
            RegisterPassword = string.Empty;
            await Shell.Current.DisplayAlert("Account", "Account created and signed in.", "OK");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Registration failed: {ex.Message}";
            await Shell.Current.DisplayAlert("Registration failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _auth.Logout();
        AccountEmail = string.Empty;
        StatusMessage = "Signed out.";
        OnPropertyChanged(nameof(IsAuthenticated));
    }

    [RelayCommand]
    private async Task OpenStatsAsync() => await Shell.Current.GoToAsync("stats");

    [RelayCommand]
    private async Task OpenHistoryAsync() => await Shell.Current.GoToAsync("history");

    [RelayCommand]
    private async Task OpenSettingsAsync() => await Shell.Current.GoToAsync("settings");

    [RelayCommand]
    private async Task OpenReadingListAsync() => await Shell.Current.GoToAsync("readinglist");

    [RelayCommand]
    private async Task OpenReviewsAsync() => await Shell.Current.GoToAsync("myreviews");
}
