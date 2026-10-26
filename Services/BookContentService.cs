using System.Text.RegularExpressions;
using PAGELY.Models;
using VersOne.Epub;

namespace PAGELY.Services;

public record BookParseResult(
    string Title,
    string Author,
    string Genre,
    int ChapterCount,
    byte[]? CoverBytes,
    List<string> Chapters);

public class BookContentService
{
    public async Task<BookParseResult> ParseAsync(string filePath, string fileType, string? originalFileName = null)
    {
        return fileType.ToLowerInvariant() switch
        {
            "epub" => await ParseEpubAsync(filePath),
            "html" or "htm" => await ParsePlainAsync(filePath, wrapHtml: false),
            "pdf" => ParsePdf(filePath, originalFileName),
            _ => await ParsePlainAsync(filePath, wrapHtml: true)
        };
    }

    public async Task<List<string>> LoadChaptersAsync(Book book)
    {
        try
        {
            if (!File.Exists(book.FilePath))
            {
                return ["<p>This file is no longer on the device. Import it again from the library.</p>"];
            }

            if (string.Equals(book.FileType, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ["__PDF_VIEWER__"];
            }

            var parsed = await ParseAsync(book.FilePath, book.FileType);
            return parsed.Chapters.Count == 0
                ? ["<p>No readable text was found in this file.</p>"]
                : parsed.Chapters;
        }
        catch (Exception ex)
        {
            return [$"<p>Error loading book content: {ex.Message}</p>"];
        }
    }

    /// <summary>
    /// Wraps book HTML in the reader's stylesheet. The colors, text size, line
    /// spacing and font come from the choices saved in the Settings page.
    /// </summary>
    public static string WrapHtml(string body)
    {
        var p = ReaderSettings.GetPalette();
        var size = ReaderSettings.FontSize.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        var spacing = ReaderSettings.LineSpacing.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        var font = ReaderSettings.FontStack;

        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <style>
            html, body {
              margin: 0;
              padding: 0;
              background: {{p.Background}};
              color: {{p.Text}};
            }
            body {
              font-family: {{font}};
              font-size: {{size}}px;
              line-height: {{spacing}};
              padding: 22px 22px 64px;
              max-width: 42em;
              margin: 0 auto;
              -webkit-text-size-adjust: 100%;
              text-rendering: optimizeLegibility;
              word-wrap: break-word;
              overflow-wrap: break-word;
            }
            p { margin: 0 0 1.15em; }
            h1, h2, h3, h4 { font-weight: 600; line-height: 1.3; margin: 1.6em 0 0.6em; }
            h1:first-child, h2:first-child, h3:first-child, h4:first-child { margin-top: 0; }
            a { color: {{p.Link}}; text-decoration: underline; }
            img, svg, video {
              max-width: 100%;
              height: auto;
              display: block;
              margin: 1.2em auto;
              border-radius: 8px;
            }
            blockquote {
              margin: 1.4em 0;
              padding: 0.1em 0 0.1em 1em;
              border-left: 3px solid {{p.Accent}};
              color: {{p.Quote}};
            }
            hr { border: 0; border-top: 1px solid {{p.Rule}}; margin: 2em 0; }
            ul, ol { padding-left: 1.3em; margin: 0 0 1.15em; }
            li { margin: 0.3em 0; }
            pre {
              white-space: pre-wrap;
              font-family: inherit;
              font-size: 1em;
              line-height: inherit;
              margin: 0 0 1.15em;
            }
            code, kbd, samp { font-family: Menlo, Consolas, monospace; font-size: 0.9em; }
            table { width: 100%; border-collapse: collapse; margin: 1.2em 0; }
            th, td { padding: 6px 8px; border-bottom: 1px solid {{p.Rule}}; text-align: left; }
          </style>
        </head>
        <body>{{body}}</body>
        </html>
        """;
    }

    private static BookParseResult ParsePdf(string filePath, string? originalFileName = null)
    {
        var info = PdfService.ReadInfo(filePath, originalFileName);

        // The page count is not known here: the document has to be opened with the
        // platform PDF engine for that. BookImportService does exactly that right
        // after parsing and overwrites this placeholder chapter count with the real
        // page count from IPdfRendererService.
        return new BookParseResult(info.Title, info.Author, "Uncategorized", 1, info.CoverBytes, ["__PDF_VIEWER__"]);
    }

    private static async Task<BookParseResult> ParseEpubAsync(string filePath)
    {
        var book = await EpubReader.ReadBookAsync(filePath);
        var title = string.IsNullOrWhiteSpace(book.Title) ? Path.GetFileNameWithoutExtension(filePath) : book.Title;
        var author = book.AuthorList is { Count: > 0 }
            ? string.Join(", ", book.AuthorList)
            : (string.IsNullOrWhiteSpace(book.Author) ? "Unknown author" : book.Author);

        var genre = "Uncategorized";
        var subjects = book.Schema?.Package?.Metadata?.Subjects;
        if (subjects is { Count: > 0 } && !string.IsNullOrWhiteSpace(subjects[0].Subject))
        {
            genre = subjects[0].Subject;
        }

        // Pictures live as separate files inside the EPUB. Collect them once so each
        // chapter's <img> tags can be pointed at the real image data.
        var images = BuildImageLookup(book);

        var chapters = new List<string>();
        foreach (var item in book.ReadingOrder)
        {
            var html = item.Content;
            if (string.IsNullOrWhiteSpace(html))
            {
                continue;
            }

            html = ExtractBody(html);
            html = InlineImages(html, item.Key, images);
            chapters.Add(html);
        }

        if (chapters.Count == 0)
        {
            chapters.Add($"<h1>{System.Net.WebUtility.HtmlEncode(title)}</h1><p>This EPUB did not include readable HTML chapters.</p>");
        }

        return new BookParseResult(title, author, genre, chapters.Count, book.CoverImage, chapters);
    }

    private sealed class ImageLookup
    {
        public Dictionary<string, (byte[] Data, string Mime)> ByPath { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, (byte[] Data, string Mime)> ByFileName { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }

    private static ImageLookup BuildImageLookup(EpubBook book)
    {
        var lookup = new ImageLookup();

        foreach (var image in book.Content.Images.Local)
        {
            if (image.Content is null || image.Content.Length == 0)
            {
                continue;
            }

            var key = NormalizePath(image.Key);
            var entry = (image.Content, GuessMime(key));
            lookup.ByPath[key] = entry;
            lookup.ByFileName[Path.GetFileName(key)] = entry;
        }

        return lookup;
    }

    /// <summary>Keeps only what is inside &lt;body&gt;, so the reader's own page is not nested in another one.</summary>
    private static string ExtractBody(string html)
    {
        var match = Regex.Match(
            html,
            @"<body[^>]*>(.*?)</body>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : html;
    }

    /// <summary>
    /// Replaces image references (img src and SVG image href) with the picture data
    /// itself, so the web view can show it without needing the EPUB's internal files.
    /// </summary>
    private static string InlineImages(string html, string chapterKey, ImageLookup images)
    {
        var normalizedKey = NormalizePath(chapterKey);
        var slash = normalizedKey.LastIndexOf('/');
        var chapterDir = slash >= 0 ? normalizedKey[..slash] : "";

        string Replace(Match m)
        {
            var src = m.Groups[3].Value;
            if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return m.Value;
            }

            var relative = Uri.UnescapeDataString(src.Split('#')[0]);
            var combined = chapterDir.Length == 0 ? relative : chapterDir + "/" + relative;
            var resolved = NormalizePath(combined);

            if (!images.ByPath.TryGetValue(resolved, out var image) &&
                !images.ByFileName.TryGetValue(Path.GetFileName(resolved), out image))
            {
                return m.Value;
            }

            var quote = m.Groups[2].Value;
            return $"{m.Groups[1].Value}{quote}data:{image.Mime};base64,{Convert.ToBase64String(image.Data)}{quote}";
        }

        const RegexOptions options = RegexOptions.Singleline | RegexOptions.IgnoreCase;
        html = Regex.Replace(html, @"(<img\b[^>]*?\bsrc\s*=\s*)([""'])(.*?)\2", Replace, options);
        html = Regex.Replace(html, @"(<image\b[^>]*?\bhref\s*=\s*)([""'])(.*?)\2", Replace, options);
        return html;
    }

    /// <summary>Turns "Text/../Images/a.jpg" style paths into one clean "Images/a.jpg" form.</summary>
    private static string NormalizePath(string path)
    {
        var parts = new List<string>();
        foreach (var part in path.Replace('\\', '/').Split('/'))
        {
            if (part.Length == 0 || part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }
            }
            else
            {
                parts.Add(part);
            }
        }

        return string.Join("/", parts);
    }

    private static string GuessMime(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

    private static async Task<BookParseResult> ParsePlainAsync(string filePath, bool wrapHtml)
    {
        var text = await File.ReadAllTextAsync(filePath);
        var title = Path.GetFileNameWithoutExtension(filePath);
        var chunks = ChunkText(text);
        var chapters = chunks
            .Select(chunk => wrapHtml
                ? $"<pre>{System.Net.WebUtility.HtmlEncode(chunk)}</pre>"
                : chunk)
            .ToList();

        return new BookParseResult(title, "Unknown author", "Uncategorized", chapters.Count, null, chapters);
    }

    private static List<string> ChunkText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ["This file is empty."];
        }

        const int size = 4000;
        var chunks = new List<string>();
        var remaining = text.Replace("\r\n", "\n");
        while (remaining.Length > 0)
        {
            if (remaining.Length <= size)
            {
                chunks.Add(remaining.Trim());
                break;
            }

            var cut = remaining.LastIndexOf('\n', size);
            if (cut < size / 2)
            {
                cut = size;
            }

            chunks.Add(remaining[..cut].Trim());
            remaining = remaining[cut..];
        }

        return chunks.Where(c => c.Length > 0).DefaultIfEmpty("This file is empty.").ToList();
    }
}