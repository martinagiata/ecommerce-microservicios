using Cart.Models;
using Cart.Repositories;

namespace Cart.Extensiones;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        app.MapGet("/cart/{userId}", async (long userId, CartRepository repo) =>
        {
            var items = await repo.GetByUserIdAsync(userId);
            return Results.Ok(items);
        }).WithTags("Cart");

        app.MapPost("/cart", async (AddCartItemRequest req, CartRepository repo) =>
        {
            if (req.UserId <= 0)
                return Results.BadRequest(new { message = "UserId es obligatorio" });
            if (req.ProductId <= 0)
                return Results.BadRequest(new { message = "ProductId es obligatorio" });
            if (string.IsNullOrWhiteSpace(req.ProductName))
                return Results.BadRequest(new { message = "El nombre del producto es obligatorio" });
            if (req.Quantity <= 0)
                return Results.BadRequest(new { message = "La cantidad debe ser mayor a 0" });

            var item = await repo.AddAsync(req);
            return Results.Created($"/cart/{item.UserId}", item);
        }).WithTags("Cart");

        app.MapPut("/cart/{id}", async (long id, UpdateCartItemRequest req, CartRepository repo) =>
        {
            if (req.Quantity <= 0)
                return Results.BadRequest(new { message = "La cantidad debe ser mayor a 0" });

            var item = await repo.UpdateQuantityAsync(id, req.Quantity);
            if (item is null)
                return Results.NotFound(new { message = $"Item con id {id} no encontrado" });
            return Results.Ok(item);
        }).WithTags("Cart");

        app.MapDelete("/cart/{id}", async (long id, CartRepository repo) =>
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound(new { message = $"Item con id {id} no encontrado" });
            return Results.Ok(new { message = $"Item con id {id} eliminado del carrito" });
        }).WithTags("Cart");

        app.MapDelete("/cart/clear/{userId}", async (long userId, CartRepository repo) =>
        {
            await repo.ClearByUserIdAsync(userId);
            return Results.Ok(new { message = $"Carrito del usuario {userId} vaciado" });
        }).WithTags("Cart");
    }
}
