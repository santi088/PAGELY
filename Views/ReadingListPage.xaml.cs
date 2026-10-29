using PAGELY.ViewModels;

namespace PAGELY.Views;

public partial class ReadingListPage : ContentPage
{
    private readonly ReadingListViewModel _viewModel;

    public ReadingListPage(ReadingListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
