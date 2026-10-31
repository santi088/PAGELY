using PAGELY.Models;
using SQLite;

namespace PAGELY.Data;

public class DatabaseService
{
    private SQLiteAsyncConnection? _connection;

    /// <summary>
    /// Raised whenever something that History shows has changed: a reading event
    /// was recorded or a book (and its events) was removed. Views that cache
    /// history use this to reload only when there is actually something new.
    /// </summary>
    public event EventHandler? HistoryChanged;

    private void RaiseHistoryChanged() => HistoryChanged?.Invoke(this, EventArgs.Empty);

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        var path = Path.Combine(FileSystem.AppDataDirectory, "pagely.db3");
        _connection = new SQLiteAsyncConnection(path);
        await _connection.CreateTableAsync<Book>();
        await _connection.CreateTableAsync<ReadingEvent>();
        await _connection.CreateTableAsync<Bookmark>();
        await _connection.CreateTableAsync<Annotation>();

        // Migration: add RenderedPagesDir if missing
        try
        {
            await _connection.ExecuteAsync(
                "ALTER TABLE Books ADD COLUMN RenderedPagesDir TEXT");
        }
        catch
        {
            // Column already exists, ignore
        }

        return _connection;
    }

    public async Task<List<Book>> GetBooksAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Book>().OrderBy(b => b.Title).ToListAsync();
    }

    public async Task<Book?> GetBookAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.FindAsync<Book>(id);
    }

    public async Task<int> SaveBookAsync(Book book)
    {
        var db = await GetConnectionAsync();
        if (book.Id != 0)
        {
            return await db.UpdateAsync(book);
        }

        return await db.InsertAsync(book);
    }

    public async Task DeleteBookAsync(Book book)
    {
        var db = await GetConnectionAsync();
        await db.Table<ReadingEvent>().Where(e => e.BookId == book.Id).DeleteAsync();
        await db.Table<Bookmark>().Where(b => b.BookId == book.Id).DeleteAsync();
        await db.Table<Annotation>().Where(a => a.BookId == book.Id).DeleteAsync();
        await db.DeleteAsync(book);
        RaiseHistoryChanged();
    }

    public async Task<List<ReadingEvent>> GetHistoryAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<ReadingEvent>()
            .OrderByDescending(e => e.ReadAt)
            .ToListAsync();
    }

    public async Task AddHistoryAsync(Book book)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(new ReadingEvent
        {
            BookId = book.Id,
            BookTitle = book.Title,
            BookAuthor = book.Author,
            ChapterIndex = book.CurrentChapterIndex,
            ProgressPercent = book.ProgressPercent,
            ReadAt = DateTime.Now
        });
        RaiseHistoryChanged();
    }

    public async Task<List<string>> GetGenresAsync()
    {
        var books = await GetBooksAsync();
        return books
            .Select(b => string.IsNullOrWhiteSpace(b.Genre) ? "Uncategorized" : b.Genre.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g)
            .ToList();
    }

    // ---- Bookmarks ----

    public async Task<List<Bookmark>> GetBookmarksAsync(int bookId)
    {
        var db = await GetConnectionAsync();
        var list = await db.Table<Bookmark>().Where(b => b.BookId == bookId).ToListAsync();
        return list.OrderBy(b => b.ChapterIndex).ThenBy(b => b.ScrollPercent).ToList();
    }

    public async Task AddBookmarkAsync(Bookmark bookmark)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(bookmark);
    }

    public async Task DeleteBookmarkAsync(Bookmark bookmark)
    {
        var db = await GetConnectionAsync();
        await db.DeleteAsync(bookmark);
    }

    // ---- Annotations (notes now, highlights and underlines in step 2) ----

    public async Task<List<Annotation>> GetAnnotationsAsync(int bookId)
    {
        var db = await GetConnectionAsync();
        var list = await db.Table<Annotation>().Where(a => a.BookId == bookId).ToListAsync();
        return list.OrderBy(a => a.ChapterIndex).ThenBy(a => a.ScrollPercent).ToList();
    }

    public async Task AddAnnotationAsync(Annotation annotation)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(annotation);
    }

    public async Task DeleteAnnotationAsync(Annotation annotation)
    {
        var db = await GetConnectionAsync();
        await db.DeleteAsync(annotation);
    }
}