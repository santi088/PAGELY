using Microsoft.Maui.Devices;
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

        // Entrance animation: fade in header and content
        var header = this.FindByName<Border>("BookDetailHeader");
        if (header != null)
        {
            header.Opacity = 0;
            header.TranslationY = 20;
            await header.FadeTo(1, 220, Easing.CubicOut);
            await header.TranslateTo(0, 0, 220, Easing.CubicOut);
        }

        if (!string.IsNullOrWhiteSpace(BookId))
        {
            await _viewModel.LoadAsync(BookId);
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnSaveClicked(object? sender, EventArgs e) =>
        SafeHaptic();

    private void OnSubmitClicked(object? sender, EventArgs e) =>
        SafeHaptic();

    private static void SafeHaptic()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            // Haptics not supported on this device; ignore
        }
    }

    private void OnLikeClicked(object? sender, EventArgs e)
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            // Haptics not supported on this device; ignore
        }
    }
}
