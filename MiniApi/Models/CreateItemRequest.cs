namespace MiniApi.Models;

public record CreateItemRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock
);