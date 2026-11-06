namespace PAGELY.Models;

public class Review
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsLiking { get; set; }
    public DateTime CreatedAt { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string UserInitial =>
        string.IsNullOrWhiteSpace(UserName) ? "R" : UserName.Trim()[..1].ToUpperInvariant();

    [System.Text.Json.Serialization.JsonIgnore]
    public string DisplayName =>
        string.IsNullOrWhiteSpace(UserName) ? "Reader" : UserName;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsOwnReview { get; set; }
}
