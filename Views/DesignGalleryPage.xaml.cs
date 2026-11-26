using PAGELY.Services;

namespace PAGELY.Views;

public partial class DesignGalleryPage : ContentPage
{
    public DesignGalleryPage()
    {
        InitializeComponent();
    }

    private void OnSystemClicked(object? sender, EventArgs e) => ThemeService.ApplyAppTheme("System");
    private void OnLightClicked(object? sender, EventArgs e) => ThemeService.ApplyAppTheme("Light");
    private void OnDarkClicked(object? sender, EventArgs e) => ThemeService.ApplyAppTheme("Dark");
}
