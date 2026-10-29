using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Data;
using PAGELY.Models;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class ReaderViewModel : ObservableObject, IDisposable
{
    private readonly DatabaseService _database;
    private readonly BookContentService _content;
    private readonly IPdfRendererService? _pdfRenderer;
    private List<string> _chapters = [];
    private bool _disposed;

    public ReaderViewModel(DatabaseService database, BookContentService content)
    {
        _database = database;
        _content = content;
    }

    public ReaderViewModel(DatabaseService database, BookContentService content, IPdfRendererService pdfRenderer)
    {
        _database = database;
        _content = content;
        _pdfRenderer = pdfRenderer;
    }

    [ObservableProperty]
    private Book? book;

    [ObservableProperty]
    private HtmlWebViewSource pageSource = new() { Html = BookContentService.WrapHtml("<p>Opening book…</p>") };

    [ObservableProperty]
    private string header = "Reader";

    [ObservableProperty]
    private string progressLabel = "";

    [ObservableProperty]
    private bool canGoPrevious;

    [ObservableProperty]
    private bool canGoNext;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChapterBar))]
    [NotifyPropertyChangedFor(nameof(ShowPdfMenuButton))]
    private bool isPdfMode;

    [ObservableProperty]
    private List<string> pdfPagePaths = [];

    /// <summary>Source PDF whose pages are rasterised on demand by the native viewer.</summary>
    [ObservableProperty]
    private string? pdfSourcePath;

    /// <summary>
    /// Width in pixels pages are rasterised at, from <see cref="PdfPageFiles.ComputeRenderWidth"/>.
    /// The native viewer renders at exactly this width.
    /// </summary>
    [ObservableProperty]
    private int renderWidth;

    [ObservableProperty]
    private int currentPdfPage;

    /// <summary>The header and bottom bars start hidden so the book fills the screen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChapterBar))]
    [NotifyPropertyChangedFor(nameof(ShowPdfMenuButton))]
    private bool areBarsVisible;

    [ObservableProperty]
    private bool isMenuOpen;

    [ObservableProperty]
    private bool hasBookmarks;

    [ObservableProperty]
    private bool hasNotes;

    public ObservableCollection<Bookmark> Bookmarks { get; } = [];

    /// <summary>Notes, highlights and underlines for the open book.</summary>
    public ObservableCollection<Annotation> Notes { get; } = [];

    /// <summary>Previous/Next bar: only for text books, and only while the bars are shown.</summary>
    public bool ShowChapterBar => AreBarsVisible && !IsPdfMode;

    /// <summary>Small floating menu button for PDFs while the bars are hidden.</summary>
    public bool ShowPdfMenuButton => IsPdfMode && !AreBarsVisible;

    /// <summary>Raised when a bookmark or note asks the PDF viewer to scroll to a page.</summary>
    public event EventHandler<int>? PdfJumpRequested;

    /// <summary>Raised when a highlight, underline or note is deleted, so the page can remove its mark.</summary>
    public event EventHandler<int>? MarkRemoved;

    /// <summary>Raised when a bookmark or spot-note is deleted, so the page can remove its pin.</summary>
    public event EventHandler<string>? PinRemoved;

    public int PendingScrollPercent { get; private set; }

    /// <summary>Annotation the page should scroll to and flash once it has loaded.</summary>
    public int? PendingFocusAnnotationId { get; private set; }

    /// <summary>Text position the page should scroll to once it has loaded.</summary>
    public int? PendingGotoOffset { get; private set; }

    [RelayCommand]
    public async Task LoadAsync(int id)
    {
        try
        {
            IsBusy = true;
            Book = await _database.GetBookAsync(id);
            if (Book is null)
            {
                PageSource = new HtmlWebViewSource { Html = BookContentService.WrapHtml("<p>This book could not be found.</p>") };
                return;
            }

            _chapters = await _content.LoadChaptersAsync(Book);

            var isPdf = string.Equals(Book.FileType, "pdf", StringComparison.OrdinalIgnoreCase);

            // A PDF's _chapters list only holds one placeholder item, so its saved page
            // must not be checked against it: doing so reset every PDF to page 1 each
            // time it was opened. PDFs get their real page count further down.
            if (!isPdf)
            {
                Book.TotalChapters = Math.Max(1, _chapters.Count);
                if (Book.CurrentChapterIndex >= _chapters.Count)
                {
                    Book.CurrentChapterIndex = Math.Max(0, _chapters.Count - 1);
                }
            }

            if (Book.ReadingStatus == ReadingStatus.ToBeRead)
            {
                Book.ReadingStatus = ReadingStatus.CurrentlyReading;
            }

            if (isPdf)
            {
                IsPdfMode = true;
                Header = Book.Title;

                var pagesDirectory = EnsurePdfPagesDirectory(Book);

                // Decide the page resolution before anything is rendered: it is handed to
                // the native viewer, so the JPEG on disk and the bitmap on screen are
                // never at different resolutions.
                RenderWidth = PdfPageFiles.ComputeRenderWidth(GetShorterDisplaySidePixels());

                // Count first, then drop pages left by another width: the count falls back
                // to the pages already on disk when the document cannot be opened, and
                // that fallback is only worth having while those pages are still there.
                var pageCount = await ResolvePdfPageCountAsync(Book.FilePath, pagesDirectory);
                await PreparePageDirectoryAsync(pagesDirectory, RenderWidth);

                // Tracked so the folder is cleaned up even if the document cannot be opened.
                Book.RenderedPagesDir = pagesDirectory;

                if (pageCount > 0)
                {
                    // Only the page count is needed here: each page image is rasterised
                    // by the native viewer the first time it scrolls into view, so
                    // opening a document never loads the whole thing into memory.
                    Book.TotalChapters = pageCount;
                    PdfSourcePath = Book.FilePath;
                    CanGoPrevious = false;
                    CanGoNext = false;

                    var savedPage = Math.Clamp(Book.CurrentChapterIndex, 0, pageCount - 1);
                    Book.CurrentChapterIndex = savedPage;

                    // Restore the position before the page list is published so the
                    // native viewer can jump straight to it.
                    CurrentPdfPage = savedPage;
                    PdfPagePaths = PdfPageFiles.PathsFor(pagesDirectory, pageCount);

                    RecalculateProgress();
                    ProgressLabel = $"Page {savedPage + 1} of {pageCount} · {Book.ProgressPercent:0}%";
                }
                else
                {
                    PageSource = new HtmlWebViewSource
                    {
                        Html = BookContentService.WrapHtml(
                            "<h2>Could not load PDF pages</h2>" +
                            "<p>This PDF could not be rendered. It may be protected, corrupted, or use an unsupported format.</p>" +
                            "<p>Try re-importing the file.</p>")
                    };
                    Book.TotalChapters = 1;
                    CanGoPrevious = false;
                    CanGoNext = false;
                    Book.ProgressPercent = 0;
                    ProgressLabel = "Render failed";
                }
            }
            else
            {
                IsPdfMode = false;
                PendingScrollPercent = (int)Math.Clamp(Book.ChapterScrollPercent, 0, 100);
                await ShowCurrentChapterAsync();
            }

            await SaveProgressAsync(recordHistory: false);
        }
        catch (Exception ex)
        {
            PageSource = new HtmlWebViewSource { Html = BookContentService.WrapHtml($"<p>Error loading book: {ex.Message}</p>") };
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Directory holding the rasterised pages of a book. The directory created at
    /// import time is reused so previously rendered pages are never thrown away.
    /// </summary>
    private static string EnsurePdfPagesDirectory(Book book)
    {
        var directory = book.RenderedPagesDir;

        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = Path.Combine(FileSystem.AppDataDirectory, "pages", Guid.NewGuid().ToString("N"));
        }

        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>
    /// Brings a page directory in line with the current screen: pages rasterised at
    /// another width are dropped, and page files a killed process left half written are
    /// removed. Off the UI thread, because it can delete every page of a long document.
    /// </summary>
    private static async Task PreparePageDirectoryAsync(string pagesDirectory, int renderWidth)
    {
        try
        {
            await Task.Run(() =>
            {
                PdfPageFiles.EnsureRenderWidth(pagesDirectory, renderWidth);
                PdfPageFiles.CleanStaleTempFiles(pagesDirectory);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"READER: preparing page directory failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Shorter side of the display in physical pixels, or 0 when the platform will not
    /// say. The shorter side is used on purpose: the display width swaps on rotation,
    /// and keying the page resolution off it would make opening the reader in landscape
    /// discard and re-rasterise every page at a new width.
    /// </summary>
    private static int GetShorterDisplaySidePixels()
    {
        try
        {
            var display = DeviceDisplay.MainDisplayInfo;
            return (int)Math.Round(Math.Min(display.Width, display.Height));
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Number of pages to present for a PDF: read from the document itself when it
    /// can be opened, otherwise whatever has already been rasterised to disk.
    /// </summary>
    private async Task<int> ResolvePdfPageCountAsync(string filePath, string pagesDirectory)
    {
        if (_pdfRenderer is not null)
        {
            try
            {
                var count = await _pdfRenderer.GetPageCountAsync(filePath);
                if (count > 0)
                {
                    return count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"READER: PDF page count failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        return PdfPageFiles.CountRendered(pagesDirectory);
    }

    [RelayCommand]
    public async Task NextAsync()
    {
        if (Book is null || Book.CurrentChapterIndex >= _chapters.Count - 1)
            return;

        Book.CurrentChapterIndex++;
        Book.ChapterScrollPercent = 0;
        PendingScrollPercent = 0;
        PendingFocusAnnotationId = null;
        PendingGotoOffset = null;
        await ShowCurrentChapterAsync();
        await SaveProgressAsync(recordHistory: false);
    }

    [RelayCommand]
    public async Task PreviousAsync()
    {
        if (Book is null || Book.CurrentChapterIndex <= 0)
            return;

        Book.CurrentChapterIndex--;
        Book.ChapterScrollPercent = 0;
        PendingScrollPercent = 0;
        PendingFocusAnnotationId = null;
        PendingGotoOffset = null;
        await ShowCurrentChapterAsync();
        await SaveProgressAsync(recordHistory: false);
    }

    public async Task PersistPositionAsync(double scrollPercent, bool recordHistory)
    {
        if (Book is null) return;
        Book.ChapterScrollPercent = Math.Clamp(scrollPercent, 0, 100);
        await SaveProgressAsync(recordHistory);
    }

    public async Task MarkFinishedIfNeededAsync(double scrollPercent)
    {
        if (Book is null || IsPdfMode) return;

        var lastChapter = Book.CurrentChapterIndex >= _chapters.Count - 1;
        if (lastChapter && scrollPercent >= 92)
        {
            Book.ReadingStatus = ReadingStatus.Completed;
            Book.ProgressPercent = 100;
            await _database.SaveBookAsync(Book);
        }
    }

    private async Task ShowCurrentChapterAsync()
    {
        if (Book is null || _chapters.Count == 0) return;

        var index = Math.Clamp(Book.CurrentChapterIndex, 0, _chapters.Count - 1);
        var chapterContent = _chapters[index];

        // The page draws its own marks and restores its own position as it loads, so
        // everything it needs is handed over inside the page itself.
        var init = await BuildPageInitJsonAsync(index);
        PendingFocusAnnotationId = null;
        PendingGotoOffset = null;

        PageSource = new HtmlWebViewSource { Html = BookContentService.WrapHtml(chapterContent, init) };

        Header = Book.Title;
        ProgressLabel = $"{Book.ChapterLabel} · {Book.ProgressPercent:0}%";
        CanGoPrevious = index > 0;
        CanGoNext = index < _chapters.Count - 1;
        RecalculateProgress();
        ProgressLabel = $"{Book.ChapterLabel} · {Book.ProgressPercent:0}%";
    }

    /// <summary>
    /// What the page needs when it loads: this chapter's highlights, underlines, notes
    /// and pins, plus where to open it.
    /// </summary>
    private async Task<string> BuildPageInitJsonAsync(int chapterIndex)
    {
        var marks = new List<object>();
        var pins = new List<object>();

        if (Book is not null && !IsPdfMode)
        {
            try
            {
                var annotations = await _database.GetAnnotationsAsync(Book.Id);
                foreach (var a in annotations.Where(a => a.ChapterIndex == chapterIndex))
                {
                    if (!string.IsNullOrWhiteSpace(a.QuotedText))
                    {
                        marks.Add(ToMark(a));
                    }
                    else if (a.TextOffset.HasValue && a.TextOffset.Value >= 0)
                    {
                        pins.Add(new { type = "note", id = a.Id, offset = a.TextOffset.Value });
                    }
                }

                var bookmarks = await _database.GetBookmarksAsync(Book.Id);
                foreach (var b in bookmarks.Where(b => b.ChapterIndex == chapterIndex))
                {
                    if (b.TextOffset.HasValue && b.TextOffset.Value >= 0)
                    {
                        pins.Add(new { type = "bookmark", id = b.Id, offset = b.TextOffset.Value });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"READER: loading marks failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            marks,
            pins,
            focus = PendingFocusAnnotationId,
            gotoOffset = PendingGotoOffset,
            scroll = PendingScrollPercent
        });
    }

    private static object ToMark(Annotation a) => new
    {
        id = a.Id,
        kind = a.Kind,
        color = a.Color ?? string.Empty,
        text = a.QuotedText,
        offset = a.TextOffset ?? -1
    };

    /// <summary>JSON the page uses to draw marks: id, kind, color, text and position.</summary>
    public string BuildMarksJson(IEnumerable<Annotation> items) =>
        System.Text.Json.JsonSerializer.Serialize(
            items.Where(a => !string.IsNullOrWhiteSpace(a.QuotedText)).Select(ToMark).ToList());

    /// <summary>JSON the page uses to draw one pin (a bookmark or spot-note marker).</summary>
    public string BuildPinJson(string type, int id, int offset) =>
        System.Text.Json.JsonSerializer.Serialize(new[] { new { type, id, offset } });

    public void UpdatePdfPage(int pageIndex)
    {
        if (Book is null || !IsPdfMode || PdfPagePaths.Count == 0) return;

        var clamped = Math.Clamp(pageIndex, 0, PdfPagePaths.Count - 1);
        if (clamped == Book.CurrentChapterIndex && ProgressLabel.Length > 0)
        {
            // Nothing about the visible page has changed: skip the recalculation so
            // repeated or redundant scroll reports never touch the toolbar.
            return;
        }

        Book.CurrentChapterIndex = clamped;
        CurrentPdfPage = clamped;
        RecalculateProgress();
        ProgressLabel = $"Page {clamped + 1} of {PdfPagePaths.Count} · {Book.ProgressPercent:0}%";
    }

    public async Task PersistPdfPositionAsync(bool recordHistory)
    {
        if (Book is null || !IsPdfMode) return;
        await SaveProgressAsync(recordHistory);
        if (PdfPagePaths.Count > 0 && Book.CurrentChapterIndex >= PdfPagePaths.Count - 1)
        {
            Book.ReadingStatus = ReadingStatus.Completed;
            Book.ProgressPercent = 100;
            await _database.SaveBookAsync(Book);
        }
    }

    // ---- Bars and menu ----

    [RelayCommand]
    public void ToggleBars() => AreBarsVisible = !AreBarsVisible;

    [RelayCommand]
    public async Task OpenMenuAsync()
    {
        await RefreshSavedAsync();
        IsMenuOpen = true;
    }

    [RelayCommand]
    public void CloseMenu() => IsMenuOpen = false;

    /// <summary>Reloads the bookmark and note lists shown in the menu.</summary>
    public async Task RefreshSavedAsync()
    {
        if (Book is null) return;

        var bookmarks = await _database.GetBookmarksAsync(Book.Id);
        Bookmarks.Clear();
        foreach (var bookmark in bookmarks)
        {
            Bookmarks.Add(bookmark);
        }

        HasBookmarks = Bookmarks.Count > 0;

        var notes = await _database.GetAnnotationsAsync(Book.Id);
        Notes.Clear();
        foreach (var note in notes)
        {
            Notes.Add(note);
        }

        HasNotes = Notes.Count > 0;
    }

    [RelayCommand]
    private async Task OpenBookmarkAsync(Bookmark? bookmark)
    {
        if (bookmark is null) return;
        await JumpToAsync(bookmark.ChapterIndex, bookmark.ScrollPercent, null, bookmark.TextOffset);
    }

    [RelayCommand]
    private async Task OpenNoteAsync(Annotation? note)
    {
        if (note is null) return;

        var focusId = string.IsNullOrWhiteSpace(note.QuotedText) ? (int?)null : note.Id;
        await JumpToAsync(note.ChapterIndex, note.ScrollPercent, focusId, note.TextOffset);
    }

    [RelayCommand]
    private async Task DeleteBookmarkAsync(Bookmark? bookmark)
    {
        if (bookmark is null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete bookmark", bookmark.Label, "Delete", "Cancel");
        if (!confirm) return;

        await RemoveBookmarkAsync(bookmark);
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(Annotation? note)
    {
        if (note is null) return;

        var confirm = await Shell.Current.DisplayAlert(
            $"Delete {note.KindLabel.ToLowerInvariant()}", note.Label, "Delete", "Cancel");
        if (!confirm) return;

        await RemoveAnnotationAsync(note);
    }

    /// <summary>Deletes a bookmark and asks the page to remove its pin.</summary>
    public async Task RemoveBookmarkAsync(Bookmark bookmark)
    {
        await _database.DeleteBookmarkAsync(bookmark);

        var inList = Bookmarks.FirstOrDefault(b => b.Id == bookmark.Id);
        if (inList is not null)
        {
            Bookmarks.Remove(inList);
        }

        HasBookmarks = Bookmarks.Count > 0;
        PinRemoved?.Invoke(this, $"bookmark-{bookmark.Id}");
    }

    /// <summary>Deletes a note, highlight or underline and asks the page to remove its mark.</summary>
    public async Task RemoveAnnotationAsync(Annotation annotation)
    {
        await _database.DeleteAnnotationAsync(annotation);

        var inList = Notes.FirstOrDefault(n => n.Id == annotation.Id);
        if (inList is not null)
        {
            Notes.Remove(inList);
        }

        HasNotes = Notes.Count > 0;
        MarkRemoved?.Invoke(this, annotation.Id);
        PinRemoved?.Invoke(this, $"note-{annotation.Id}");
    }

    public async Task<Annotation?> GetAnnotationAsync(int id)
    {
        if (Book is null) return null;

        var all = await _database.GetAnnotationsAsync(Book.Id);
        return all.FirstOrDefault(a => a.Id == id);
    }

    public async Task<Bookmark?> GetBookmarkAsync(int id)
    {
        if (Book is null) return null;

        var all = await _database.GetBookmarksAsync(Book.Id);
        return all.FirstOrDefault(b => b.Id == id);
    }

    /// <summary>Moves the reader to a saved spot without leaving the reading screen.</summary>
    private async Task JumpToAsync(int chapterIndex, double scrollPercent, int? focusId, int? gotoOffset)
    {
        if (Book is null) return;

        if (IsPdfMode)
        {
            if (PdfPagePaths.Count == 0) return;

            var page = Math.Clamp(chapterIndex, 0, PdfPagePaths.Count - 1);
            Book.CurrentChapterIndex = page;
            CurrentPdfPage = page;
            RecalculateProgress();
            ProgressLabel = $"Page {page + 1} of {PdfPagePaths.Count} · {Book.ProgressPercent:0}%";
            PdfJumpRequested?.Invoke(this, page);
        }
        else
        {
            if (_chapters.Count == 0) return;

            Book.CurrentChapterIndex = Math.Clamp(chapterIndex, 0, _chapters.Count - 1);
            Book.ChapterScrollPercent = Math.Clamp(scrollPercent, 0, 100);
            PendingScrollPercent = (int)Book.ChapterScrollPercent;
            PendingFocusAnnotationId = focusId;
            PendingGotoOffset = gotoOffset;
            await ShowCurrentChapterAsync();
        }

        IsMenuOpen = false;
        AreBarsVisible = false;
        await SaveProgressAsync(recordHistory: false);
    }

    // ---- Bookmarks and notes: adding ----

    /// <summary>Saves a bookmark at the current spot.</summary>
    public async Task<(string Message, Bookmark? Created)> AddBookmarkAsync(
        double scrollPercent,
        int? textOffset,
        string? snippet)
    {
        if (Book is null)
        {
            return ("Open a book first.", null);
        }

        var scroll = CaptureScroll(scrollPercent);

        var existing = await _database.GetBookmarksAsync(Book.Id);
        var duplicate = existing.Any(b =>
            b.ChapterIndex == Book.CurrentChapterIndex &&
            (IsPdfMode || Math.Abs(b.ScrollPercent - scroll) < 3));

        if (duplicate)
        {
            return ("This spot is already bookmarked.", null);
        }

        var label = BuildPositionLabel();
        var shortSnippet = string.IsNullOrWhiteSpace(snippet)
            ? null
            : (snippet.Length > 90 ? snippet[..90].TrimEnd() : snippet.Trim());

        var bookmark = new Bookmark
        {
            BookId = Book.Id,
            ChapterIndex = Book.CurrentChapterIndex,
            ScrollPercent = scroll,
            TextOffset = IsPdfMode ? null : textOffset,
            Snippet = IsPdfMode ? null : shortSnippet,
            Label = label,
            CreatedAt = DateTime.Now
        };

        await _database.AddBookmarkAsync(bookmark);
        return ($"Bookmarked: {label}", bookmark);
    }

    /// <summary>Saves a note at the current spot (with an optional quoted passage).</summary>
    public async Task<(string Message, Annotation? Created)> AddNoteAsync(
        string text,
        string? quotedText,
        double scrollPercent,
        int? textOffset)
    {
        if (Book is null)
        {
            return ("Open a book first.", null);
        }

        var scroll = CaptureScroll(scrollPercent);
        var label = BuildPositionLabel();

        var annotation = new Annotation
        {
            BookId = Book.Id,
            ChapterIndex = Book.CurrentChapterIndex,
            ScrollPercent = scroll,
            Kind = "Note",
            QuotedText = quotedText,
            TextOffset = IsPdfMode ? null : textOffset,
            NoteText = text,
            Label = label,
            CreatedAt = DateTime.Now
        };

        await _database.AddAnnotationAsync(annotation);
        return ($"Note saved: {label}", annotation);
    }

    /// <summary>Saves a highlight, underline or note on selected text. Text books only.</summary>
    public async Task<Annotation?> AddMarkAsync(
        string kind,
        string color,
        string text,
        int offset,
        string noteText,
        double scrollPercent)
    {
        if (Book is null || IsPdfMode)
        {
            return null;
        }

        var scroll = CaptureScroll(scrollPercent);

        var annotation = new Annotation
        {
            BookId = Book.Id,
            ChapterIndex = Book.CurrentChapterIndex,
            ScrollPercent = scroll,
            Kind = kind,
            Color = string.IsNullOrWhiteSpace(color) ? null : color,
            QuotedText = text,
            TextOffset = offset >= 0 ? offset : null,
            NoteText = noteText,
            Label = BuildPositionLabel(),
            CreatedAt = DateTime.Now
        };

        await _database.AddAnnotationAsync(annotation);
        await RefreshSavedAsync();
        return annotation;
    }

    private double CaptureScroll(double scrollPercent)
    {
        if (Book is null || IsPdfMode)
        {
            return 0;
        }

        Book.ChapterScrollPercent = Math.Clamp(scrollPercent, 0, 100);
        RecalculateProgress();
        return Book.ChapterScrollPercent;
    }

    private string BuildPositionLabel()
    {
        if (Book is null)
        {
            return string.Empty;
        }

        return IsPdfMode
            ? $"Page {Book.CurrentChapterIndex + 1} of {PdfPagePaths.Count} · {Book.ProgressPercent:0}%"
            : $"{Book.ChapterLabel} · {Book.ProgressPercent:0}%";
    }

    private void RecalculateProgress()
    {
        if (Book is null) return;

        if (IsPdfMode && PdfPagePaths.Count > 0)
        {
            var pageIndex = Math.Clamp(Book.CurrentChapterIndex, 0, PdfPagePaths.Count - 1);
            Book.ProgressPercent = Math.Clamp(
                ((pageIndex + 1.0) / PdfPagePaths.Count) * 100.0,
                0, 100);
            return;
        }

        if (_chapters.Count == 0) return;

        var chapterShare = 100.0 / _chapters.Count;
        Book.ProgressPercent = Math.Clamp(
            (Book.CurrentChapterIndex * chapterShare) + (Book.ChapterScrollPercent / 100.0 * chapterShare),
            0, 100);
    }

    private async Task SaveProgressAsync(bool recordHistory)
    {
        if (Book is null) return;

        RecalculateProgress();
        Book.LastReadAt = DateTime.Now;
        await _database.SaveBookAsync(Book);
        if (recordHistory)
        {
            await _database.AddHistoryAsync(Book);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    ~ReaderViewModel()
    {
        Dispose(false);
    }
}