using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PAGELY.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    [RelayCommand]
    private async Task OpenStatsAsync()
    {
        await Shell.Current.GoToAsync("stats");
    }

    [RelayCommand]
    private async Task OpenHistoryAsync()
    {
        await Shell.Current.GoToAsync("history");
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        await Shell.Current.GoToAsync("settings");
    }

    [RelayCommand]
    private async Task OpenReadingListAsync()
    {
        await Shell.Current.GoToAsync("readinglist");
    }

    [RelayCommand]
    private async Task OpenReviewsAsync()
    {
        await Shell.Current.DisplayAlert("My reviews", "Reviews will be connected when the backend is available.", "OK");
    }
}
