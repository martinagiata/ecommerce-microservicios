namespace Cart.Models;

public record CartItem
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public long ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public double Price { get; init; }
    public int Quantity { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public string? UpdatedAt { get; init; }
}

public record AddCartItemRequest
{
    public long UserId { get; init; }
    public long ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public double Price { get; init; }
    public int Quantity { get; init; }
}

public record UpdateCartItemRequest
{
    public int Quantity { get; init; }
}
