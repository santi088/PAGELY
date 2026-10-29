using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public IReadOnlyList<string> Themes { get; } = ReaderSettings.Themes;

    public IReadOnlyList<string> Fonts { get; } = ReaderSettings.Fonts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontSizeLabel))]
    private double fontSize = ReaderSettings.FontSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineSpacingLabel))]
    private double lineSpacing = ReaderSettings.LineSpacing;

    [ObservableProperty]
    private string theme = ReaderSettings.Theme;

    [ObservableProperty]
    private string fontFamily = ReaderSettings.FontFamily;

    public string FontSizeLabel => $"{FontSize:0} px";

    public string LineSpacingLabel => $"{LineSpacing:0.00}";

    partial void OnFontSizeChanged(double value) =>
        ReaderSettings.FontSize = Math.Round(value);

    partial void OnLineSpacingChanged(double value) =>
        ReaderSettings.LineSpacing = Math.Round(value * 20) / 20;

    partial void OnThemeChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        ReaderSettings.Theme = value;
        ThemeService.Apply(value);
    }

    partial void OnFontFamilyChanged(string value)
    {
        if (!string.IsNullOrEmpty(value)) ReaderSettings.FontFamily = value;
    }

    [RelayCommand]
    private void Reset()
    {
        FontSize = 19;
        LineSpacing = 1.85;
        Theme = "Paper";
        FontFamily = "Serif";
    }
}