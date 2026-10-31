using PAGELY.Models;

namespace PAGELY.Services;

/// <summary>
/// Ensures a PDF has a cached first-page thumbnail to use as its cover.
/// </summary>
public sealed class PdfCoverService
{
    private readonly IPdfRendererService _pdfRenderer;

    public PdfCoverService(IPdfRendererService pdfRenderer)
    {
        _pdfRenderer = pdfRenderer;
    }

    public async Task<string?> EnsureCoverAsync(Book book, CancellationToken cancellationToken = default)
    {
        if (book is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(book.CoverPath) && File.Exists(book.CoverPath))
        {
            return book.CoverPath;
        }

        if (!string.Equals(book.FileType, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(book.FilePath) || !File.Exists(book.FilePath))
        {
            return null;
        }

        try
        {
            var coversDir = Path.Combine(FileSystem.AppDataDirectory, "covers");
            Directory.CreateDirectory(coversDir);

            var baseName = Path.GetFileNameWithoutExtension(book.FilePath);
            var outputPath = Path.Combine(coversDir, $"{baseName}_cover.jpg");

            if (File.Exists(outputPath))
            {
                return outputPath;
            }

            var rendered = await _pdfRenderer.RenderPageToFileAsync(
                book.FilePath,
                0,
                outputPath,
                240,
                cancellationToken);

            return rendered && File.Exists(outputPath) ? outputPath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PDF COVER: generation failed: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }
}
