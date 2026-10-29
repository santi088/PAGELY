namespace PAGELY.Models;

public class Review
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public int LikeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
