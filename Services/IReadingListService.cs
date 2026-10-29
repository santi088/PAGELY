using PAGELY.Models;

namespace PAGELY.Services;

/// <summary>Books the reader saved from Discover to read later.</summary>
public interface IReadingListService
{
    /// <summary>Raised whenever a book is added or removed.</summary>
    event EventHandler? Changed;

    Task<IReadOnlyList<DiscoverBook>> GetAsync();

    Task<bool> ContainsAsync(string bookId);

    Task AddAsync(DiscoverBook book);

    Task RemoveAsync(string bookId);
}