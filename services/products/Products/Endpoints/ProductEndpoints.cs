using Products.Models;
using Products.Repositories;

namespace Products.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", async (ProductRepository repo) =>
        {
            var products = await repo.GetAllAsync();
            return Results.Ok(products);
        });

        app.MapGet("/products/{id}", async (int id, ProductRepository repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        app.MapPost("/products", async (CreateProductRequest request, ProductRepository repo) =>
        {
            var product = await repo.CreateAsync(request);
            return Results.Created($"/products/{product.Id}", product);
        });

        app.MapPatch("/products/{id}/stock", async (int id, int newStock, ProductRepository repo) =>
        {
            var product = await repo.UpdateStockAsync(id, newStock);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });
    }
}
