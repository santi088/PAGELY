using SQLite;

namespace PAGELY.Models;

[Table("Annotations")]
public class Annotation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int BookId { get; set; }

    /// <summary>Page index for PDFs, chapter index for every other book.</summary>
    public int ChapterIndex { get; set; }

    public double ScrollPercent { get; set; }

    /// <summary>Note, Highlight or Underline.</summary>
    public string Kind { get; set; } = "Note";

    /// <summary>Yellow, Green, Pink, Blue or Orange.</summary>
    public string? Color { get; set; }

    public string? QuotedText { get; set; }

    /// <summary>Where the quoted text starts inside the chapter text.</summary>
    public int? TextOffset { get; set; }

    public string NoteText { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Ignore]
    public string CreatedLabel => CreatedAt.ToString("MMM d, yyyy h:mm tt");

    [Ignore]
    public string KindLabel => Kind switch
    {
        "Highlight" => "Highlight",
        "Underline" => "Underline",
        _ => "Note"
    };

    [Ignore]
    public string Heading => $"{KindLabel} · {Label}";

    [Ignore]
    public bool HasNoteText => !string.IsNullOrWhiteSpace(NoteText);

    [Ignore]
    public string SwatchHex => Kind == "Note"
        ? "#A98654"
        : (Color ?? string.Empty) switch
        {
            "Green" => "#51CF66",
            "Pink" => "#F783AC",
            "Blue" => "#4DABF7",
            "Orange" => "#FFA94D",
            _ => "#FFD43B"
        };

    [Ignore]
    public Microsoft.Maui.Graphics.Color SwatchColor =>
        Microsoft.Maui.Graphics.Color.FromArgb(SwatchHex);
}