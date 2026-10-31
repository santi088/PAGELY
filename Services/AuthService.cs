using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PAGELY.Services;

public sealed class AuthResponseDto
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("userId")] public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("expiresAt")] public DateTime ExpiresAt { get; set; }
}

public sealed class UserDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("firstName")] public string? FirstName { get; set; }
    [JsonPropertyName("lastName")] public string? LastName { get; set; }
    [JsonPropertyName("isSuspended")] public bool IsSuspended { get; set; }
    [JsonPropertyName("roles")] public List<string> Roles { get; set; } = new();
}

public sealed class AuthService
{
    private const string TokenKey = "auth_token";
    private const string EmailKey = "auth_email";
    private const string UserIdKey = "auth_user_id";

    private static HttpClient CreateClient()
    {
        var baseUrl = Preferences.Default.Get("api_base_url", "http://192.168.100.166:5242");
        var client = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        client.Timeout = TimeSpan.FromSeconds(15);
        return client;
    }

    public string? Token => Preferences.Default.Get(TokenKey, string.Empty) is { Length: > 0 } t ? t : null;
    public string? Email => Preferences.Default.Get(EmailKey, string.Empty) is { Length: > 0 } e ? e : null;
    public string? UserId => Preferences.Default.Get(UserIdKey, string.Empty) is { Length: > 0 } u ? u : null;
    public bool IsAuthenticated => Token is not null;

    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await CreateClient().PostAsJsonAsync("api/auth/login", new { email, password }, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ReadErrorAsync(response));
        }

        var dto = await response.Content.ReadFromJsonAsync<AuthResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty login response.");
        Store(dto);
        return dto;
    }

    public async Task<AuthResponseDto> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default)
    {
        var response = await CreateClient().PostAsJsonAsync("api/auth/register", new { email, password, firstName, lastName }, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ReadErrorAsync(response));
        }

        var dto = await response.Content.ReadFromJsonAsync<AuthResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty register response.");
        Store(dto);
        return dto;
    }

    public async Task<UserDto?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (Token is null)
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var response = await CreateClient().SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserDto>(ct);
        }
        catch
        {
            return null;
        }
    }

    public void Logout()
    {
        Preferences.Default.Remove(TokenKey);
        Preferences.Default.Remove(EmailKey);
        Preferences.Default.Remove(UserIdKey);
    }


    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? response.ReasonPhrase ?? "Request failed.";
            }

            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                return string.Join(" ", errors.EnumerateArray().Select(e => e.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)));
            }
        }
        catch
        {
        }

        return response.ReasonPhrase ?? "Request failed.";
    }

    private static void Store(AuthResponseDto dto)
    {
        Preferences.Default.Set(TokenKey, dto.Token);
        Preferences.Default.Set(EmailKey, dto.Email);
        Preferences.Default.Set(UserIdKey, dto.UserId);
    }
}
