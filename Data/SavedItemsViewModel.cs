using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Data;
using PAGELY.Models;

namespace PAGELY.ViewModels;

public partial class SavedItemsViewModel : ObservableObject
{
    private readonly DatabaseService _database;
    private int _bookId;

    public SavedItemsViewModel(DatabaseService database)
    {
        _database = database;
    }

    public ObservableCollection<Bookmark> Bookmarks { get; } = [];

    public ObservableCollection<Annotation> Notes { get; } = [];

    [ObservableProperty]
    private string bookTitle = string.Empty;

    [ObservableProperty]
    private bool hasBookmarks;

    [ObservableProperty]
    private bool hasNotes;

    public async Task LoadAsync(int bookId)
    {
        _bookId = bookId;

        var book = await _database.GetBookAsync(bookId);
        BookTitle = book?.Title ?? string.Empty;

        var bookmarks = await _database.GetBookmarksAsync(bookId);
        Bookmarks.Clear();
        foreach (var bookmark in bookmarks)
        {
            Bookmarks.Add(bookmark);
        }

        HasBookmarks = Bookmarks.Count > 0;

        var notes = await _database.GetAnnotationsAsync(bookId);
        Notes.Clear();
        foreach (var note in notes)
        {
            Notes.Add(note);
        }

        HasNotes = Notes.Count > 0;
    }

    [RelayCommand]
    private Task OpenBookmarkAsync(Bookmark? bookmark) =>
        bookmark is null
            ? Task.CompletedTask
            : JumpToAsync(bookmark.ChapterIndex, bookmark.ScrollPercent);

    [RelayCommand]
    private Task OpenNoteAsync(Annotation? note) =>
        note is null
            ? Task.CompletedTask
            : JumpToAsync(note.ChapterIndex, note.ScrollPercent);

    [RelayCommand]
    private async Task DeleteBookmarkAsync(Bookmark? bookmark)
    {
        if (bookmark is null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete bookmark", bookmark.Label, "Delete", "Cancel");
        if (!confirm) return;

        await _database.DeleteBookmarkAsync(bookmark);
        Bookmarks.Remove(bookmark);
        HasBookmarks = Bookmarks.Count > 0;
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(Annotation? note)
    {
        if (note is null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete note", note.Label, "Delete", "Cancel");
        if (!confirm) return;

        await _database.DeleteAnnotationAsync(note);
        Notes.Remove(note);
        HasNotes = Notes.Count > 0;
    }

    [RelayCommand]
    private async Task BackAsync() => await Shell.Current.GoToAsync("..");

    /// <summary>
    /// Saves the chosen spot as the book's reading position, then goes back to the
    /// reader, which reloads the book from that saved position.
    /// </summary>
    private async Task JumpToAsync(int chapterIndex, double scrollPercent)
    {
        var book = await _database.GetBookAsync(_bookId);
        if (book is null) return;

        book.CurrentChapterIndex = chapterIndex;
        book.ChapterScrollPercent = scrollPercent;
        await _database.SaveBookAsync(book);

        await Shell.Current.GoToAsync("..");
    }
}