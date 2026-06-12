using Products.Models;
using Products.Repositories;

namespace Products.Extensiones;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", async (ProductRepository repo) =>
        {
            var products = await repo.GetAllAsync();
            return Results.Ok(products);
        }).WithTags("Products");

        app.MapGet("/products/{id}", async (long id, ProductRepository repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            if (product is null)
                return Results.NotFound(new { errorCode = "PRODUCT_NOT_FOUND", errorMessage = $"Product con id {id} no encontrado" });
            return Results.Ok(product);
        }).WithTags("Products");

        app.MapPost("/products", async (CreateProductRequest req, ProductRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { errorCode = "INVALID_NAME", errorMessage = "El nombre es obligatorio" });
            if (req.Price < 0)
                return Results.BadRequest(new { errorCode = "INVALID_PRICE", errorMessage = "El precio no puede ser negativo" });
            if (req.Stock < 0)
                return Results.BadRequest(new { errorCode = "INVALID_STOCK", errorMessage = "El stock no puede ser negativo" });

            var product = await repo.CreateAsync(req);
            return Results.Created($"/products/{product.Id}", product);
        }).WithTags("Products");

        app.MapPut("/products/{id}", async (long id, UpdateProductRequest req, ProductRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { errorCode = "INVALID_NAME", errorMessage = "El nombre es obligatorio" });
            if (req.Price < 0)
                return Results.BadRequest(new { errorCode = "INVALID_PRICE", errorMessage = "El precio no puede ser negativo" });
            if (req.Stock < 0)
                return Results.BadRequest(new { errorCode = "INVALID_STOCK", errorMessage = "El stock no puede ser negativo" });

            var product = await repo.UpdateAsync(id, req);
            if (product is null)
                return Results.NotFound(new { errorCode = "PRODUCT_NOT_FOUND", errorMessage = $"Product con id {id} no encontrado" });
            return Results.Ok(product);
        }).WithTags("Products");

        app.MapDelete("/products/{id}", async (long id, ProductRepository repo) =>
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound(new { errorCode = "PRODUCT_NOT_FOUND", errorMessage = $"Product con id {id} no encontrado" });
            return Results.Ok(new { message = $"Product con id {id} eliminado correctamente" });
        }).WithTags("Products");
    }
}
