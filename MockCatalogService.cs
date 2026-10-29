using PAGELY.Models;

namespace PAGELY.Services;

/// <summary>
/// Sample catalog used until the online database is ready. Swap this class for the
/// real one in MauiProgram.cs and nothing else needs to change.
/// </summary>
public class MockCatalogService : IOnlineCatalogService
{
    private static readonly List<DiscoverBook> Books =
    [
        new() { Id = "b01", Title = "Pride and Prejudice", Author = "Jane Austen", Genre = "Romance", Year = 1813, Rating = 4.6, RatingCount = 1280,
            Description = "A quick-witted young woman and a proud, wealthy gentleman keep misjudging each other while family pressure and social rank get in the way." },
        new() { Id = "b02", Title = "Frankenstein", Author = "Mary Shelley", Genre = "Science Fiction", Year = 1818, Rating = 4.2, RatingCount = 940,
            Description = "A young scientist brings a creature to life and then runs from what he has made, with tragic results for everyone." },
        new() { Id = "b03", Title = "Dracula", Author = "Bram Stoker", Genre = "Horror", Year = 1897, Rating = 4.1, RatingCount = 870,
            Description = "Told through letters and diary entries, a group of friends races to stop an ancient count who is bringing his darkness to England." },
        new() { Id = "b04", Title = "The Adventures of Sherlock Holmes", Author = "Arthur Conan Doyle", Genre = "Mystery", Year = 1892, Rating = 4.5, RatingCount = 1120,
            Description = "A collection of short cases in which a brilliant detective and his loyal friend solve puzzles that baffle everyone else." },
        new() { Id = "b05", Title = "Alice's Adventures in Wonderland", Author = "Lewis Carroll", Genre = "Fantasy", Year = 1865, Rating = 4.3, RatingCount = 990,
            Description = "A girl follows a hurried rabbit down a hole and lands in a world where logic, size and manners all stop making sense." },
        new() { Id = "b06", Title = "The Picture of Dorian Gray", Author = "Oscar Wilde", Genre = "Classic", Year = 1890, Rating = 4.4, RatingCount = 760,
            Description = "A handsome young man stays untouched by time while a portrait of him slowly shows the cost of how he lives." },
        new() { Id = "b07", Title = "Moby-Dick", Author = "Herman Melville", Genre = "Adventure", Year = 1851, Rating = 3.9, RatingCount = 610,
            Description = "A sailor joins a whaling ship whose captain is driven by a single obsession: hunting one enormous white whale." },
        new() { Id = "b08", Title = "The Time Machine", Author = "H. G. Wells", Genre = "Science Fiction", Year = 1895, Rating = 4.0, RatingCount = 540,
            Description = "An inventor travels far into the future and finds a world that has changed in ways he never expected." },
        new() { Id = "b09", Title = "Treasure Island", Author = "Robert Louis Stevenson", Genre = "Adventure", Year = 1883, Rating = 4.2, RatingCount = 700,
            Description = "A boy finds a treasure map and sails out with a crew, only to learn that some of his shipmates are pirates." },
        new() { Id = "b10", Title = "Jane Eyre", Author = "Charlotte Brontë", Genre = "Romance", Year = 1847, Rating = 4.5, RatingCount = 830,
            Description = "An orphaned governess with a strong will takes a job at a mysterious house and has to decide what she is willing to give up for love." },
        new() { Id = "b11", Title = "The Hound of the Baskervilles", Author = "Arthur Conan Doyle", Genre = "Mystery", Year = 1902, Rating = 4.3, RatingCount = 650,
            Description = "A family curse and a ghostly hound on a foggy moor bring the famous detective to the countryside." },
        new() { Id = "b12", Title = "Little Women", Author = "Louisa May Alcott", Genre = "Classic", Year = 1868, Rating = 4.4, RatingCount = 910,
            Description = "Four sisters grow up together, chasing different dreams while their family faces hard times." },
        new() { Id = "b13", Title = "The Wonderful Wizard of Oz", Author = "L. Frank Baum", Genre = "Fantasy", Year = 1900, Rating = 4.1, RatingCount = 580,
            Description = "A farm girl is swept away to a colorful land and must find the wizard who might send her home." },
        new() { Id = "b14", Title = "Wuthering Heights", Author = "Emily Brontë", Genre = "Romance", Year = 1847, Rating = 4.0, RatingCount = 720,
            Description = "A wild, stormy tale of two families on the Yorkshire moors and a bond that turns into a lifelong grudge." }
    ];

    public async Task<IReadOnlyList<DiscoverBook>> GetBooksAsync(
        string? genre,
        DiscoverSort sort,
        CancellationToken cancellationToken = default)
    {
        // A short pause, so the loading spinner behaves like it will with a real server.
        await Task.Delay(250, cancellationToken);

        IEnumerable<DiscoverBook> query = Books;

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(b => string.Equals(b.Genre, genre, StringComparison.OrdinalIgnoreCase));
        }

        query = sort switch
        {
            DiscoverSort.Newest => query.OrderByDescending(b => b.Year).ThenBy(b => b.Title),
            DiscoverSort.TitleAZ => query.OrderBy(b => b.Title),
            _ => query.OrderByDescending(b => b.Rating).ThenByDescending(b => b.RatingCount)
        };

        return query.ToList();
    }

    public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);

        return Books
            .Select(b => b.Genre)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g)
            .ToList();
    }

    public Task<DiscoverBook?> GetBookAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Books.FirstOrDefault(b => b.Id == id));
    }

    public async Task<IReadOnlyList<DiscoverBook>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        await Task.Delay(150, cancellationToken);

        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var q = query.Trim();
        return Books
            .Where(b =>
                b.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Genre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                b.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(b => b.Rating)
            .ThenBy(b => b.Title)
            .ToList();
    }
}