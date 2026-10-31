using System.Text.Json.Serialization;

namespace PAGELY.Models;

/// <summary>
/// A book from the online catalog. Plain data only, so the real backend can fill it
/// later without any screen having to change.
/// </summary>
public class DiscoverBook
{
    private static readonly string[] CoverPalette =
        ["#8A7351", "#6C8A6E", "#7C93A3", "#A5766A", "#A98654", "#7A6244", "#5F7A8A"];

    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double Rating { get; set; }

    public int RatingCount { get; set; }

    public int Year { get; set; }

    public string? CoverUrl { get; set; }

    [JsonIgnore]
    public string Initial =>
        string.IsNullOrWhiteSpace(Title) ? "P" : Title.Trim()[..1].ToUpperInvariant();

    [JsonIgnore]
    public string RatingLabel => $"★ {Rating:0.0}";

    [JsonIgnore]
    public string RatingCountLabel =>
        RatingCount == 1 ? "1 rating" : $"{RatingCount:N0} ratings";

    /// <summary>A steady color per genre, shown behind the book's first letter.</summary>
    [JsonIgnore]
    public Color CoverColor
    {
        get
        {
            var seed = 0;
            foreach (var c in Genre)
            {
                seed += c;
            }

            return Color.FromArgb(CoverPalette[seed % CoverPalette.Length]);
        }
    }
}