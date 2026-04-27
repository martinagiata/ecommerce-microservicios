namespace MiniApi.Models;

public record UpdateItemRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock
);