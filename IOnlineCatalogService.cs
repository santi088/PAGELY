using PAGELY.Models;

namespace PAGELY.Services;

public enum DiscoverSort
{
    TopRated,
    Newest,
    TitleAZ
}

public interface IOnlineCatalogService
{
    /// <summary>Books for the Discover tab. A null genre means all genres.</summary>
    Task<IReadOnlyList<DiscoverBook>> GetBooksAsync(
        string? genre,
        DiscoverSort sort,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default);
}