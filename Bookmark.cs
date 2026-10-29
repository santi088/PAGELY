using SQLite;

namespace PAGELY.Models;

[Table("Bookmarks")]
public class Bookmark
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int BookId { get; set; }

    /// <summary>Page index for PDFs, chapter index for every other book.</summary>
    public int ChapterIndex { get; set; }

    public double ScrollPercent { get; set; }

    /// <summary>Position inside the chapter text, used to scroll back to the exact line.</summary>
    public int? TextOffset { get; set; }

    /// <summary>A few words from the bookmarked spot, shown in the list.</summary>
    public string? Snippet { get; set; }

    public string Label { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Ignore]
    public string CreatedLabel => CreatedAt.ToString("MMM d, yyyy h:mm tt");
}