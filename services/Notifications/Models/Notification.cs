namespace Notifications.Models;

public record Notification
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = "pending";
    public string CreatedAt { get; init; } = string.Empty;
    public string? SentAt { get; init; }
}

public record CreateNotificationRequest
{
    public long UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
