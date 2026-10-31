using PAGELY.ViewModels;

namespace PAGELY.Views;

[QueryProperty(nameof(BookId), nameof(BookId))]
public partial class DiscoverBookDetailPage : ContentPage
{
    private readonly DiscoverBookDetailViewModel _viewModel;

    public DiscoverBookDetailPage(DiscoverBookDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public string BookId { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrWhiteSpace(BookId))
        {
            await _viewModel.LoadAsync(BookId);
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
