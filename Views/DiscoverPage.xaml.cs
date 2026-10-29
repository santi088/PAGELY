using Microsoft.Extensions.DependencyInjection;
using PAGELY.ViewModels;

namespace PAGELY.Views;

public partial class DiscoverPage : ContentPage
{
    private readonly DiscoverViewModel _viewModel;

    public DiscoverPage() : this(MauiProgram.Services.GetRequiredService<DiscoverViewModel>())
    {
    }

    public DiscoverPage(DiscoverViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}