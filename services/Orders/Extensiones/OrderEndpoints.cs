using Orders.Models;
using Orders.Repositories;
using Orders.Services;

namespace Orders.Extensiones;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapGet("/orders", async (OrderRepository repo) =>
        {
            var orders = await repo.GetAllAsync();
            return Results.Ok(orders);
        }).WithTags("Orders");

        app.MapGet("/orders/{id}", async (long id, OrderRepository repo) =>
        {
            var order = await repo.GetByIdAsync(id);
            if (order is null)
                return Results.NotFound(new { message = $"Order con id {id} no encontrada" });
            return Results.Ok(order);
        }).WithTags("Orders");

        app.MapGet("/orders/user/{userId}", async (long userId, OrderRepository repo) =>
        {
            var orders = await repo.GetByUserIdAsync(userId);
            return Results.Ok(orders);
        }).WithTags("Orders");

        app.MapPost("/orders", async (CreateOrderRequest req, OrderRepository repo, UsersService usersService) =>
        {
            if (req.UserId <= 0)
                return Results.BadRequest(new { message = "UserId es obligatorio" });
            if (req.Total < 0)
                return Results.BadRequest(new { message = "El total no puede ser negativo" });

            // Consulta datos del usuario via HTTP (cumple restriccion de independencia)
            var user = await usersService.GetUserByIdAsync(req.UserId);
            if (user is null)
                return Results.NotFound(new { message = $"Usuario con id {req.UserId} no encontrado" });

            var order = await repo.CreateAsync(req, user);
            return Results.Created($"/orders/{order.Id}", order);
        }).WithTags("Orders");

        app.MapPatch("/orders/{id}/status", async (long id, UpdateOrderStatusRequest req, OrderRepository repo) =>
        {
            var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
            if (!validStatuses.Contains(req.Status))
                return Results.BadRequest(new { message = "Estado inválido" });

            var order = await repo.UpdateStatusAsync(id, req.Status);
            if (order is null)
                return Results.NotFound(new { message = $"Order con id {id} no encontrada" });
            return Results.Ok(order);
        }).WithTags("Orders");
    }
}
