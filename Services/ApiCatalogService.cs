using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PAGELY.Models;

namespace PAGELY.Services;

public sealed class ApiBookDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("author")] public string Author { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("coverImageUrl")] public string? CoverImageUrl { get; set; }
    [JsonPropertyName("publicationYear")] public int? PublicationYear { get; set; }
    [JsonPropertyName("averageRating")] public double AverageRating { get; set; }
    [JsonPropertyName("reviewCount")] public int ReviewCount { get; set; }
    [JsonPropertyName("genres")] public List<string> Genres { get; set; } = new();
}

public sealed class ApiPagedResult<T>
{
    [JsonPropertyName("items")] public List<T> Items { get; set; } = new();
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
    [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
}

public sealed class ApiGenreDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public sealed class ApiCatalogService : IOnlineCatalogService
{
    private static HttpClient CreateClient()
    {
        var baseUrl = Preferences.Default.Get("api_base_url", "http://192.168.100.166:5242");
        var client = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        client.Timeout = TimeSpan.FromSeconds(15);
        return client;
    }

    public async Task<IReadOnlyList<DiscoverBook>> GetBooksAsync(string? genre = null, DiscoverSort sort = DiscoverSort.TopRated, CancellationToken cancellationToken = default)
    {
        var sortBy = sort switch
        {
            DiscoverSort.Newest => "created",
            DiscoverSort.TitleAZ => "title",
            _ => "rating"
        };
        var sortOrder = sort == DiscoverSort.TitleAZ ? "asc" : "desc";

        var url = $"api/books?sortBy={Uri.EscapeDataString(sortBy)}&sortOrder={sortOrder}&pageSize=50";
        if (!string.IsNullOrWhiteSpace(genre))
        {
            url += $"&genre={Uri.EscapeDataString(genre)}";
        }

        var client = CreateClient();
        var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"GET {client.BaseAddress}{url} -> {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiBookDto>>(cancellationToken);
        return (result?.Items ?? new()).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        var response = await client.GetAsync("api/genres", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"GET {client.BaseAddress}api/genres -> {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }
        var genres = await response.Content.ReadFromJsonAsync<List<ApiGenreDto>>(cancellationToken);
        return (genres ?? new()).Select(g => g.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();
    }

    public async Task<DiscoverBook?> GetBookAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            return null;
        }

        var client = CreateClient();
        var response = await client.GetAsync($"api/books/{guid}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"GET {client.BaseAddress}api/books/{guid} -> {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }
        var dto = await response.Content.ReadFromJsonAsync<ApiBookDto>(cancellationToken);
        return dto is null ? null : Map(dto);
    }

    public async Task<IReadOnlyList<DiscoverBook>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<DiscoverBook>();
        }

        var url = $"api/books?search={Uri.EscapeDataString(query)}&sortBy=rating&sortOrder=desc&pageSize=50";
        var client = CreateClient();
        var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"GET {client.BaseAddress}{url} -> {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiBookDto>>(cancellationToken);
        return (result?.Items ?? new()).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<DiscoverBook>> GetDiscoveryAsync(string shelf, CancellationToken cancellationToken = default)
    {
        var url = $"api/discovery?shelf={Uri.EscapeDataString(shelf)}&pageSize=12";
        var client = CreateClient();
        var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"GET {client.BaseAddress}{url} -> {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiBookDto>>(cancellationToken);
        return (result?.Items ?? new()).Select(Map).ToList();
    }

    private static DiscoverBook Map(ApiBookDto dto) => new()
    {
        Id = dto.Id.ToString(),
        Title = dto.Title,
        Author = dto.Author,
        Description = dto.Description ?? string.Empty,
        Genre = dto.Genres.FirstOrDefault() ?? "Uncategorized",
        Rating = dto.AverageRating,
        RatingCount = dto.ReviewCount,
        Year = dto.PublicationYear ?? 0,
        CoverUrl = dto.CoverImageUrl
    };
}
