using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Networking;
using PAGELY.Data;
using PAGELY.Models;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly DatabaseService _database;
    private readonly IOnlineCatalogService _catalog;
    private List<Book> _all = [];

    public SearchViewModel(DatabaseService database, IOnlineCatalogService catalog)
    {
        _database = database;
        _catalog = catalog;
        StatusFilters = new ObservableCollection<string>(ReadingStatusDisplay.FilterOptions);
        SortOptions = new ObservableCollection<string>(ReadingStatusDisplay.SortOptions);
        Genres = new ObservableCollection<string> { "All genres" };
        SelectedStatus = "All";
        SelectedGenre = "All genres";
        SelectedSort = "Title";
    }

    public ObservableCollection<Book> Books { get; } = [];
    public ObservableCollection<DiscoverBook> CommunityBooks { get; } = [];
    public ObservableCollection<string> StatusFilters { get; }
    public ObservableCollection<string> SortOptions { get; }
    public ObservableCollection<string> Genres { get; }

    [ObservableProperty]
    private string query = string.Empty;

    [ObservableProperty]
    private string selectedStatus = "All";

    [ObservableProperty]
    private string selectedGenre = "All genres";

    [ObservableProperty]
    private string selectedSort = "Title";

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private bool isFilterVisible = false;

    [ObservableProperty]
    private bool isCommunityScope = false;

    [ObservableProperty]
    private bool isCommunityBusy;

    [ObservableProperty]
    private bool isCommunityEmpty;

    [ObservableProperty]
    private string communityMessage = "Search the online catalog.";

    partial void OnQueryChanged(string value)
    {
        Apply();
        if (IsCommunityScope)
        {
            _ = SearchCommunityAsync();
        }
    }

    partial void OnSelectedStatusChanged(string value) => Apply();
    partial void OnSelectedGenreChanged(string value) => Apply();
    partial void OnSelectedSortChanged(string value) => Apply();

    partial void OnIsCommunityScopeChanged(bool value)
    {
        if (value)
        {
            _ = SearchCommunityAsync();
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            _all = await _database.GetBooksAsync();
            var genres = await _database.GetGenresAsync();
            Genres.Clear();
            Genres.Add("All genres");
            foreach (var genre in genres)
            {
                Genres.Add(genre);
            }

            if (!Genres.Contains(SelectedGenre))
            {
                SelectedGenre = "All genres";
            }

            Apply();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Could not load books: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void SetLibraryScope() => IsCommunityScope = false;

    [RelayCommand]
    private void SetCommunityScope() => IsCommunityScope = true;

    [RelayCommand]
    public async Task OpenAsync(Book? book)
    {
        if (book is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"bookdetail?BookId={book.Id}");
    }

    [RelayCommand]
    public async Task OpenCommunityAsync(DiscoverBook? book)
    {
        if (book is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"discoverbookdetail?BookId={book.Id}");
    }

    private async Task SearchCommunityAsync()
    {
        if (!IsCommunityScope)
        {
            return;
        }

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            CommunityBooks.Clear();
            IsCommunityEmpty = true;
            CommunityMessage = "You're offline. Community search requires a connection.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Query))
        {
            CommunityBooks.Clear();
            IsCommunityEmpty = true;
            CommunityMessage = "Type a title, author, genre, or keyword.";
            return;
        }

        try
        {
            IsCommunityBusy = true;
            var results = await _catalog.SearchAsync(Query);
            CommunityBooks.Clear();
            foreach (var book in results)
            {
                CommunityBooks.Add(book);
            }

            IsCommunityEmpty = CommunityBooks.Count == 0;
            CommunityMessage = IsCommunityEmpty
                ? "No community results found."
                : (CommunityBooks.Count == 1 ? "1 result" : $"{CommunityBooks.Count} results");
        }
        catch (Exception ex)
        {
            CommunityBooks.Clear();
            IsCommunityEmpty = true;
            CommunityMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsCommunityBusy = false;
        }
    }

    private void Apply()
    {
        IEnumerable<Book> result = _all;

        if (!string.IsNullOrWhiteSpace(Query))
        {
            var q = Query.Trim();
            result = result.Where(b =>
                b.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Genre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.StatusLabel.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(SelectedStatus, "All", StringComparison.OrdinalIgnoreCase))
        {
            var status = ReadingStatusDisplay.FromLabel(SelectedStatus);
            result = result.Where(b => b.ReadingStatus == status);
        }

        if (!string.Equals(SelectedGenre, "All genres", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(b => b.Genre.Equals(SelectedGenre, StringComparison.OrdinalIgnoreCase));
        }

        result = SelectedSort switch
        {
            "Author" => result.OrderBy(b => b.Author).ThenBy(b => b.Title),
            "Genre" => result.OrderBy(b => b.Genre).ThenBy(b => b.Title),
            "Status" => result.OrderBy(b => b.Status).ThenBy(b => b.Title),
            "Recently added" => result.OrderByDescending(b => b.DateAdded),
            "Recently read" => result.OrderByDescending(b => b.LastReadAt ?? DateTime.MinValue),
            "Progress" => result.OrderByDescending(b => b.ProgressPercent).ThenBy(b => b.Title),
            _ => result.OrderBy(b => b.Title)
        };

        Books.Clear();
        foreach (var book in result)
        {
            Books.Add(book);
        }

        IsEmpty = Books.Count == 0;
    }
}
