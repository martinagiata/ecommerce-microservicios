namespace Orders.Models;

public record Order
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string Status { get; init; } = "pending";
    public double Total { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public string? UpdatedAt { get; init; }
}

public record CreateOrderRequest
{
    public long UserId { get; init; }
    public double Total { get; init; }
}

public record UpdateOrderStatusRequest
{
    public string Status { get; init; } = string.Empty;
}

public record UserInfo
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
