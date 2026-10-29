using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Models;
using PAGELY.Services;

namespace PAGELY.ViewModels;

/// <summary>One genre filter button.</summary>
public partial class GenreChip : ObservableObject
{
    public GenreChip(string name, bool isSelected = false)
    {
        Name = name;
        this.isSelected = isSelected;
    }

    public string Name { get; }

    [ObservableProperty]
    private bool isSelected;
}

/// <summary>A catalog book plus whether the reader has saved it.</summary>
public partial class DiscoverItem : ObservableObject
{
    public DiscoverItem(DiscoverBook book, bool isSaved)
    {
        Book = book;
        this.isSaved = isSaved;
    }

    public DiscoverBook Book { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaveLabel))]
    private bool isSaved;

    public string SaveLabel => IsSaved ? "Saved ✓" : "+ Reading list";
}

public partial class DiscoverViewModel : ObservableObject
{
    private const string AllGenres = "All";

    private readonly IOnlineCatalogService _catalog;
    private readonly IReadingListService _readingList;
    private bool _initialized;
    private int _refreshVersion;

    public DiscoverViewModel(IOnlineCatalogService catalog, IReadingListService readingList)
    {
        _catalog = catalog;
        _readingList = readingList;
    }

    public ObservableCollection<GenreChip> Genres { get; } = [];

    public ObservableCollection<DiscoverItem> Books { get; } = [];

    public IReadOnlyList<string> SortOptions { get; } = ["Top rated", "Newest", "A to Z"];

    [ObservableProperty]
    private string selectedSort = "Top rated";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private string resultsLabel = string.Empty;

    partial void OnSelectedSortChanged(string value)
    {
        if (_initialized && !string.IsNullOrEmpty(value))
        {
            _ = RefreshAsync();
        }
    }

    /// <summary>Called every time the tab is shown, so saved states stay up to date.</summary>
    public async Task InitializeAsync()
    {
        if (!_initialized)
        {
            try
            {
                var genres = await _catalog.GetGenresAsync();

                Genres.Clear();
                Genres.Add(new GenreChip(AllGenres, true));
                foreach (var genre in genres)
                {
                    Genres.Add(new GenreChip(genre));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Discover", $"Could not load genres: {ex.Message}", "OK");
                return;
            }

            _initialized = true;
        }

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        var version = ++_refreshVersion;

        try
        {
            IsBusy = true;

            var selected = Genres.FirstOrDefault(g => g.IsSelected)?.Name;
            var genre = selected is null or AllGenres ? null : selected;

            var sort = SelectedSort switch
            {
                "Newest" => DiscoverSort.Newest,
                "A to Z" => DiscoverSort.TitleAZ,
                _ => DiscoverSort.TopRated
            };

            var books = await _catalog.GetBooksAsync(genre, sort);
            var savedIds = (await _readingList.GetAsync()).Select(b => b.Id).ToHashSet();

            // A newer refresh started while this one was loading: let that one win.
            if (version != _refreshVersion)
            {
                return;
            }

            Books.Clear();
            foreach (var book in books)
            {
                Books.Add(new DiscoverItem(book, savedIds.Contains(book.Id)));
            }

            IsEmpty = Books.Count == 0;
            ResultsLabel = Books.Count == 1 ? "1 book" : $"{Books.Count} books";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Discover", $"Could not load books: {ex.Message}", "OK");
        }
        finally
        {
            if (version == _refreshVersion)
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private async Task SelectGenreAsync(GenreChip? chip)
    {
        if (chip is null)
        {
            return;
        }

        foreach (var genre in Genres)
        {
            genre.IsSelected = ReferenceEquals(genre, chip);
        }

        await RefreshAsync();
    }

    [RelayCommand]
    private async Task ToggleSaveAsync(DiscoverItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (item.IsSaved)
        {
            await _readingList.RemoveAsync(item.Book.Id);
            item.IsSaved = false;
        }
        else
        {
            await _readingList.AddAsync(item.Book);
            item.IsSaved = true;
        }
    }

    [RelayCommand]
    private async Task OpenAsync(DiscoverItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"discoverbookdetail?BookId={item.Book.Id}");
    }
}