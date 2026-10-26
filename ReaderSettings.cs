namespace PAGELY.Services;

public sealed record ReaderPalette(
    string Background,
    string Text,
    string Link,
    string Accent,
    string Rule,
    string Quote);

/// <summary>Reader appearance choices, saved on the device.</summary>
public static class ReaderSettings
{
    public static readonly string[] Themes = ["Paper", "Sepia", "Dark"];
    public static readonly string[] Fonts = ["Serif", "Sans-serif"];

    public static double FontSize
    {
        get => Preferences.Default.Get("reader_font_size", 19.0);
        set => Preferences.Default.Set("reader_font_size", value);
    }

    public static double LineSpacing
    {
        get => Preferences.Default.Get("reader_line_spacing", 1.85);
        set => Preferences.Default.Set("reader_line_spacing", value);
    }

    public static string Theme
    {
        get => Preferences.Default.Get("reader_theme", "Paper");
        set => Preferences.Default.Set("reader_theme", value);
    }

    public static string FontFamily
    {
        get => Preferences.Default.Get("reader_font", "Serif");
        set => Preferences.Default.Set("reader_font", value);
    }

    public static string FontStack =>
        FontFamily == "Sans-serif"
            ? "Roboto, \"Helvetica Neue\", Arial, sans-serif"
            : "Georgia, \"Times New Roman\", serif";

    public static ReaderPalette GetPalette() =>
        Theme switch
        {
            "Sepia" => new ReaderPalette("#f0e1c0", "#3f2e1a", "#7a5a2e", "#d6c196", "#dccaa0", "#5a4326"),
            "Dark" => new ReaderPalette("#1b1a18", "#e8dfd0", "#d2b27f", "#4a4237", "#3a342c", "#bfb5a4"),
            _ => new ReaderPalette("#fbf6ea", "#402a18", "#7a6244", "#e0d3bd", "#e8ddcb", "#5f4732")
        };
}