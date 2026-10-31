using System.Text.Json;
using PAGELY.Models;

namespace PAGELY.Services;

/// <summary>Keeps the reading list on the phone until accounts exist online.</summary>
public class LocalReadingListService : IReadingListService
{
    private const string StorageKey = "reading_list_v1";

    public event EventHandler? Changed;

    public Task<IReadOnlyList<DiscoverBook>> GetAsync() =>
        Task.FromResult<IReadOnlyList<DiscoverBook>>(Load());

    public Task<bool> ContainsAsync(string bookId) =>
        Task.FromResult(Load().Any(b => b.Id == bookId));

    public Task AddAsync(DiscoverBook book)
    {
        var list = Load();
        if (list.All(b => b.Id != book.Id))
        {
            list.Insert(0, book);
            Save(list);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string bookId)
    {
        var list = Load();
        if (list.RemoveAll(b => b.Id == bookId) > 0)
        {
            Save(list);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        return Task.CompletedTask;
    }

    private static List<DiscoverBook> Load()
    {
        try
        {
            var json = Preferences.Default.Get(StorageKey, "[]");
            return JsonSerializer.Deserialize<List<DiscoverBook>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static void Save(List<DiscoverBook> list) =>
        Preferences.Default.Set(StorageKey, JsonSerializer.Serialize(list));
}