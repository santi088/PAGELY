using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PAGELY.Models;

namespace PAGELY.Services;

public sealed class ApiReadingListEntryDto
{
    [JsonPropertyName("bookId")] public Guid BookId { get; set; }
    [JsonPropertyName("bookTitle")] public string BookTitle { get; set; } = string.Empty;
    [JsonPropertyName("bookAuthor")] public string BookAuthor { get; set; } = string.Empty;
    [JsonPropertyName("coverImageUrl")] public string? CoverImageUrl { get; set; }
}

public sealed class ApiReadingListService : IReadingListService
{
    private readonly AuthService _auth;

    public ApiReadingListService(AuthService auth)
    {
        _auth = auth;
    }

    public event EventHandler? Changed;

    private static HttpClient CreateClient()
    {
        return ApiConfiguration.CreateClient();
    }

    private HttpRequestMessage NewRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        if (_auth.Token is not null)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);
        }

        return request;
    }

    public async Task<IReadOnlyList<DiscoverBook>> GetAsync()
    {
        EnsureAuthenticated();
        using var request = NewRequest(HttpMethod.Get, "api/me/reading-list?pageSize=100");
        var response = await CreateClient().SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiReadingListEntryDto>>();
        return (result?.Items ?? new()).Select(e => new DiscoverBook
        {
            Id = e.BookId.ToString(),
            Title = e.BookTitle,
            Author = e.BookAuthor,
            CoverUrl = e.CoverImageUrl
        }).ToList();
    }

    public async Task<bool> ContainsAsync(string bookId)
    {
        if (!_auth.IsAuthenticated || string.IsNullOrWhiteSpace(bookId))
        {
            return false;
        }

        var items = await GetAsync();
        return items.Any(b => string.Equals(b.Id, bookId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(DiscoverBook book)
    {
        EnsureAuthenticated();
        if (string.IsNullOrWhiteSpace(book.Id))
        {
            throw new InvalidOperationException("Invalid book id.");
        }

        using var request = NewRequest(HttpMethod.Put, $"api/books/{Uri.EscapeDataString(book.Id)}/reading-list");
        request.Content = JsonContent.Create(new { status = 0 });
        var response = await CreateClient().SendAsync(request);
        response.EnsureSuccessStatusCode();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoveAsync(string bookId)
    {
        EnsureAuthenticated();
        if (string.IsNullOrWhiteSpace(bookId))
        {
            return;
        }

        using var request = NewRequest(HttpMethod.Delete, $"api/books/{Uri.EscapeDataString(bookId)}/reading-list");
        var response = await CreateClient().SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        response.EnsureSuccessStatusCode();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureAuthenticated()
    {
        if (!_auth.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Sign in to use the reading list.");
        }
    }
}
