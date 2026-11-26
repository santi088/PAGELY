using Microsoft.Maui.Devices;
using Microsoft.Extensions.DependencyInjection;
using PAGELY.ViewModels;

namespace PAGELY.Views;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryViewModel _viewModel;

    public LibraryPage() : this(MauiProgram.Services.GetRequiredService<LibraryViewModel>())
    {
    }

    public LibraryPage(LibraryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Entrance animation: fade + slide up for header and cards
        var header = this.FindByName<Border>("LibraryHeader");
        var fabs = this.FindByName<Button>("ImportFab");
        if (header != null)
        {
            header.Opacity = 0;
            header.TranslationY = 20;
            await header.FadeTo(1, 220, Easing.CubicOut);
            await header.TranslateTo(0, 0, 220, Easing.CubicOut);
        }
        if (fabs != null)
        {
            fabs.Opacity = 0;
            fabs.Scale = 0.8;
            await fabs.FadeTo(1, 220, Easing.CubicOut);
            await fabs.ScaleTo(1, 220, Easing.CubicOut);
        }

        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void OnImportClicked(object? sender, EventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    }
}
