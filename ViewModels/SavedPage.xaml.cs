using PAGELY.ViewModels;

namespace PAGELY.Views;

[QueryProperty(nameof(BookId), nameof(BookId))]
public partial class SavedPage : ContentPage
{
    private readonly SavedItemsViewModel _viewModel;

    public SavedPage(SavedItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public string BookId { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (int.TryParse(BookId, out var id))
        {
            await _viewModel.LoadAsync(id);
        }
    }
}