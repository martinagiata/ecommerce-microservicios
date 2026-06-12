using MiniApi.Models;
using MiniApi.Repositories;

namespace MiniApi.Extensions;

public static class EndpointsExtensions
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        app.MapGet("/items", async (ItemRepository repo) =>
        {
            var items = await repo.GetAllAsync();
            return Results.Ok(items);
        }).WithTags("Items");

        app.MapGet("/items/{id}", async (long id, ItemRepository repo) =>
        {
            var item = await repo.GetByIdAsync(id);
            if (item is null)
                return Results.NotFound(new { errorCode = "ITEM_NOT_FOUND", errorMessage = $"Item con id {id} no encontrado" });
            return Results.Ok(item);
        }).WithTags("Items");

        app.MapPost("/items", async (CreateItemRequest req, ItemRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { errorCode = "INVALID_NAME", errorMessage = "El nombre es obligatorio" });
            if (req.Price < 0)
                return Results.BadRequest(new { errorCode = "INVALID_PRICE", errorMessage = "El precio no puede ser negativo" });
            if (req.Stock < 0)
                return Results.BadRequest(new { errorCode = "INVALID_STOCK", errorMessage = "El stock no puede ser negativo" });

            var item = await repo.CreateAsync(req);
            return Results.Created($"/items/{item.Id}", item);
        }).WithTags("Items");

        app.MapPut("/items/{id}", async (long id, UpdateItemRequest req, ItemRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { errorCode = "INVALID_NAME", errorMessage = "El nombre es obligatorio" });
            if (req.Price < 0)
                return Results.BadRequest(new { errorCode = "INVALID_PRICE", errorMessage = "El precio no puede ser negativo" });
            if (req.Stock < 0)
                return Results.BadRequest(new { errorCode = "INVALID_STOCK", errorMessage = "El stock no puede ser negativo" });

            var item = await repo.UpdateAsync(id, req);
            if (item is null)
                return Results.NotFound(new { errorCode = "ITEM_NOT_FOUND", errorMessage = $"Item con id {id} no encontrado" });
            return Results.Ok(item);
        }).WithTags("Items");

        app.MapDelete("/items/{id}", async (long id, ItemRepository repo) =>
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound(new { errorCode = "ITEM_NOT_FOUND", errorMessage = $"Item con id {id} no encontrado" });
            return Results.Ok(new { message = $"Item con id {id} eliminado correctamente" });
        }).WithTags("Items");
    }
}
