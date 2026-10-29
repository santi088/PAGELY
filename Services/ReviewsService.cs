using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PAGELY.Models;

namespace PAGELY.Services;

public sealed class ApiReviewDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("userId")] public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("userEmail")] public string UserEmail { get; set; } = string.Empty;
    [JsonPropertyName("bookId")] public Guid BookId { get; set; }
    [JsonPropertyName("rating")] public int Rating { get; set; }
    [JsonPropertyName("reviewText")] public string? ReviewText { get; set; }
    [JsonPropertyName("likeCount")] public int LikeCount { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
}

public sealed class ReviewsService
{
    private readonly AuthService _auth;

    public ReviewsService(AuthService auth)
    {
        _auth = auth;
    }

    private static HttpClient CreateClient()
    {
        var baseUrl = Preferences.Default.Get("api_base_url", "http://192.168.100.166:5242");
        var client = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        client.Timeout = TimeSpan.FromSeconds(15);
        return client;
    }

    public async Task<IReadOnlyList<Review>> GetReviewsAsync(string bookId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(bookId, out var guid))
        {
            return Array.Empty<Review>();
        }

        var response = await CreateClient().GetAsync($"api/books/{guid}/reviews?pageSize=50", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiReviewDto>>(ct);
        return (result?.Items ?? new()).Select(Map).ToList();
    }

    public async Task<Review> CreateReviewAsync(string bookId, int rating, string? text, CancellationToken ct = default)
    {
        if (!_auth.IsAuthenticated || _auth.Token is null)
        {
            throw new UnauthorizedAccessException("Sign in to write a review.");
        }

        if (!Guid.TryParse(bookId, out var guid))
        {
            throw new InvalidOperationException("Invalid book id.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/books/{guid}/reviews");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);
        request.Content = JsonContent.Create(new { rating, reviewText = text });
        var response = await CreateClient().SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<ApiReviewDto>(ct)
            ?? throw new InvalidOperationException("Empty review response.");
        return Map(dto);
    }

    public async Task<IReadOnlyList<Review>> GetMyReviewsAsync(CancellationToken ct = default)
    {
        if (_auth.Token is null)
        {
            throw new UnauthorizedAccessException("Sign in to view your reviews.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/me/reviews?pageSize=50");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);
        var response = await CreateClient().SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiReviewDto>>(ct);
        return (result?.Items ?? new()).Select(Map).ToList();
    }

    private static Review Map(ApiReviewDto dto) => new()
    {
        Id = dto.Id,
        BookId = dto.BookId,
        UserId = dto.UserId,
        UserEmail = dto.UserEmail,
        Rating = dto.Rating,
        ReviewText = dto.ReviewText,
        LikeCount = dto.LikeCount,
        CreatedAt = dto.CreatedAt
    };
}
