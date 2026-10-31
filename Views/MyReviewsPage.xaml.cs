using PAGELY.ViewModels;

namespace PAGELY.Views;

public partial class MyReviewsPage : ContentPage
{
    private readonly MyReviewsViewModel _viewModel;

    public MyReviewsPage(MyReviewsViewModel viewModel)
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
