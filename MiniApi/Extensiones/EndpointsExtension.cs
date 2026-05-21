using MiniApi.Models;

namespace MiniApi.Extensions;

public static class EndpointsExtensions
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        var items = new List<Item>();
        var idCounter = 1L;

        // GET all
        app.MapGet("/items", () =>
        {
            return Results.Ok(items);
        })
        .WithTags("Items");

        // GET by id
        app.MapGet("/items/{id}", (long id) =>
        {
            var item = items.FirstOrDefault(i => i.Id == id);

            if (item is null)
            {
                return Results.NotFound(new
                {
                    message = $"Item con id {id} no encontrado"
                });
            }

            return Results.Ok(item);
        })
        .WithTags("Items");

        // POST
        app.MapPost("/items", (CreateItemRequest req) =>
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(req.Name))
            {
                return Results.BadRequest(new
                {
                    message = "El nombre es obligatorio"
                });
            }

            if (req.Price < 0)
            {
                return Results.BadRequest(new
                {
                    message = "El precio no puede ser negativo"
                });
            }

            if (req.Stock < 0)
            {
                return Results.BadRequest(new
                {
                    message = "El stock no puede ser negativo"
                });
            }

            var item = new Item
            {
                Id = idCounter++,
                Name = req.Name,
                Description = req.Description,
                Price = (double)req.Price,
                Stock = req.Stock,
                CreatedAt = DateTime.UtcNow.ToString("o")
            };

            items.Add(item);

            return Results.Created($"/items/{item.Id}", item);
        })
        .WithTags("Items");

        // PUT
        app.MapPut("/items/{id}", (long id, UpdateItemRequest req) =>
        {
            var existing = items.FirstOrDefault(i => i.Id == id);

            if (existing is null)
            {
                return Results.NotFound(new
                {
                    message = $"Item con id {id} no encontrado"
                });
            }

            // Validaciones
            if (string.IsNullOrWhiteSpace(req.Name))
            {
                return Results.BadRequest(new
                {
                    message = "El nombre es obligatorio"
                });
            }

            if (req.Price < 0)
            {
                return Results.BadRequest(new
                {
                    message = "El precio no puede ser negativo"
                });
            }

            if (req.Stock < 0)
            {
                return Results.BadRequest(new
                {
                    message = "El stock no puede ser negativo"
                });
            }

            var updated = existing with
            {
                Name = req.Name,
                Description = req.Description,
                Price = (double)req.Price,
                Stock = req.Stock,
                UpdatedAt = DateTime.UtcNow.ToString("o")
            };

            items.Remove(existing);
            items.Add(updated);

            return Results.Ok(updated);
        })
        .WithTags("Items");

        // DELETE
        app.MapDelete("/items/{id}", (long id) =>
        {
            var item = items.FirstOrDefault(i => i.Id == id);

            if (item is null)
            {
                return Results.NotFound(new
                {
                    message = $"Item con id {id} no encontrado"
                });
            }

            items.Remove(item);

            return Results.Ok(new
            {
                message = $"Item con id {id} eliminado correctamente"
            });
        })
        .WithTags("Items");
    }
}