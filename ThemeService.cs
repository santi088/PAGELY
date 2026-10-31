namespace PAGELY.Services;

/// <summary>
/// Swaps the app's colors while it is running, so choosing a theme in Settings
/// restyles every screen at once.
/// </summary>
public static class ThemeService
{
    private static readonly Dictionary<string, string> PaperTheme = new()
    {
        ["Primary"] = "#7A6244",
        ["PrimaryDark"] = "#5C4A34",
        ["PrimaryLight"] = "#A78F66",
        ["Secondary"] = "#F4EDE0",
        ["SecondaryAlt"] = "#EBE1D1",
        ["Surface"] = "#FFFCF6",
        ["SurfaceAlt"] = "#F3ECE0",
        ["Paper"] = "#FBF6EA",
        ["Card"] = "#FFFDF9",
        ["PageBackdrop"] = "#E9DFCE",
        ["Ink"] = "#402A18",
        ["InkSoft"] = "#5F4732",
        ["Muted"] = "#7A6A56",
        ["Divider"] = "#E8DDCB",
        ["Shadow"] = "#402A18",
        ["Spine"] = "#8A7351",
        ["Accent"] = "#A98654",
        ["Success"] = "#6C8A6E",
        ["Danger"] = "#A2544A",
        ["StatusTbr"] = "#7C93A3",
        ["StatusReading"] = "#A98654",
        ["StatusCompleted"] = "#6C8A6E",
        ["StatusDropped"] = "#A5766A",
        ["Highlight"] = "#E9D7A4",
        ["HighlightSoft"] = "#F2E7C9",
        ["WarmAccent"] = "#C79A5B",
        ["Gray100"] = "#F3EEE4",
        ["Gray200"] = "#E8DDCB",
        ["Gray300"] = "#D6C9B3",
        ["Gray400"] = "#A2917A",
        ["Gray500"] = "#7A6A56",
        ["Gray600"] = "#5F4732",
        ["Gray900"] = "#402A18",
        ["Gray950"] = "#2A1C10"
    };

    // White backgrounds, with the same brown buttons and cream accents as the other themes.
    private static readonly Dictionary<string, string> WhiteTheme = new()
    {
        ["Primary"] = "#7A6244",
        ["PrimaryDark"] = "#5C4A34",
        ["PrimaryLight"] = "#A78F66",
        ["Secondary"] = "#F4EDE0",
        ["SecondaryAlt"] = "#EBE1D1",
        ["Surface"] = "#FFFFFF",
        ["SurfaceAlt"] = "#F3ECE0",
        ["Paper"] = "#FFFFFF",
        ["Card"] = "#FFFFFF",
        ["PageBackdrop"] = "#EDEAE4",
        ["Ink"] = "#222222",
        ["InkSoft"] = "#4A4A4A",
        ["Muted"] = "#6F6A62",
        ["Divider"] = "#E8E2D6",
        ["Shadow"] = "#000000",
        ["Spine"] = "#8A7351",
        ["Accent"] = "#A98654",
        ["Success"] = "#6C8A6E",
        ["Danger"] = "#A2544A",
        ["StatusTbr"] = "#7C93A3",
        ["StatusReading"] = "#A98654",
        ["StatusCompleted"] = "#6C8A6E",
        ["StatusDropped"] = "#A5766A",
        ["Highlight"] = "#E9D7A4",
        ["HighlightSoft"] = "#F2E7C9",
        ["WarmAccent"] = "#C79A5B",
        ["Gray100"] = "#F5F3EE",
        ["Gray200"] = "#E8E2D6",
        ["Gray300"] = "#D6CFC0",
        ["Gray400"] = "#A29A8A",
        ["Gray500"] = "#6F6A62",
        ["Gray600"] = "#4A4A4A",
        ["Gray900"] = "#222222",
        ["Gray950"] = "#111111"
    };

    private static readonly Dictionary<string, string> SepiaTheme = new()
    {
        ["Primary"] = "#7A5A2E",
        ["PrimaryDark"] = "#5A4222",
        ["PrimaryLight"] = "#A5834F",
        ["Secondary"] = "#EADBB8",
        ["SecondaryAlt"] = "#E0CFA5",
        ["Surface"] = "#F7EBD0",
        ["SurfaceAlt"] = "#E8D8B3",
        ["Paper"] = "#F0E1C0",
        ["Card"] = "#F7ECD3",
        ["PageBackdrop"] = "#E3D2AA",
        ["Ink"] = "#3F2E1A",
        ["InkSoft"] = "#5A4326",
        ["Muted"] = "#7A6644",
        ["Divider"] = "#DCCAA0",
        ["Shadow"] = "#3F2E1A",
        ["Spine"] = "#8A6B3E",
        ["Accent"] = "#A07A3F",
        ["Success"] = "#647F5E",
        ["Danger"] = "#9A4B3F",
        ["StatusTbr"] = "#6F8796",
        ["StatusReading"] = "#A07A3F",
        ["StatusCompleted"] = "#647F5E",
        ["StatusDropped"] = "#9C6A5A",
        ["Highlight"] = "#E5CF90",
        ["HighlightSoft"] = "#EEDDAF",
        ["WarmAccent"] = "#BE9150",
        ["Gray100"] = "#EFE0BE",
        ["Gray200"] = "#E3D1A8",
        ["Gray300"] = "#D0BC8E",
        ["Gray400"] = "#A08C62",
        ["Gray500"] = "#7A6644",
        ["Gray600"] = "#5A4326",
        ["Gray900"] = "#3F2E1A",
        ["Gray950"] = "#2A1D10"
    };

    private static readonly Dictionary<string, string> DarkTheme = new()
    {
        ["Primary"] = "#C2A878",
        ["PrimaryDark"] = "#A38A5F",
        ["PrimaryLight"] = "#D6C096",
        ["Secondary"] = "#2F2C27",
        ["SecondaryAlt"] = "#38342E",
        ["Surface"] = "#25231F",
        ["SurfaceAlt"] = "#2F2C27",
        ["Paper"] = "#1B1A18",
        ["Card"] = "#2A2824",
        ["PageBackdrop"] = "#141311",
        ["Ink"] = "#E8DFD0",
        ["InkSoft"] = "#CFC4B1",
        ["Muted"] = "#A39883",
        ["Divider"] = "#3A342C",
        ["Shadow"] = "#000000",
        ["Spine"] = "#B39B6E",
        ["Accent"] = "#C9A66B",
        ["Success"] = "#86A888",
        ["Danger"] = "#CF7B70",
        ["StatusTbr"] = "#8FA9BA",
        ["StatusReading"] = "#C9A66B",
        ["StatusCompleted"] = "#86A888",
        ["StatusDropped"] = "#C98F80",
        ["Highlight"] = "#6B5B2E",
        ["HighlightSoft"] = "#4A4128",
        ["WarmAccent"] = "#D3A869",
        ["Gray100"] = "#2F2C27",
        ["Gray200"] = "#38342E",
        ["Gray300"] = "#4A4439",
        ["Gray400"] = "#7A705E",
        ["Gray500"] = "#A39883",
        ["Gray600"] = "#CFC4B1",
        ["Gray900"] = "#E8DFD0",
        ["Gray950"] = "#F3EBDD"
    };

    public static void Apply(string theme)
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
        {
            return;
        }

        var palette = theme switch
        {
            "White" => WhiteTheme,
            "Sepia" => SepiaTheme,
            "Dark" => DarkTheme,
            _ => PaperTheme
        };

        foreach (var (key, hex) in palette)
        {
            var color = Color.FromArgb(hex);
            resources[key] = color;
            resources[key + "Brush"] = new SolidColorBrush(color);
        }
    }
}