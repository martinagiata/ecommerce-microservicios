using Notifications.Models;
using Notifications.Repositories;

namespace Notifications.Extensiones;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        app.MapGet("/notifications", async (NotificationRepository repo) =>
        {
            var notifications = await repo.GetAllAsync();
            return Results.Ok(notifications);
        }).WithTags("Notifications");

        app.MapGet("/notifications/{id}", async (long id, NotificationRepository repo) =>
        {
            var notification = await repo.GetByIdAsync(id);
            if (notification is null)
                return Results.NotFound(new { message = $"Notification con id {id} no encontrada" });
            return Results.Ok(notification);
        }).WithTags("Notifications");

        app.MapGet("/notifications/user/{userId}", async (long userId, NotificationRepository repo) =>
        {
            var notifications = await repo.GetByUserIdAsync(userId);
            return Results.Ok(notifications);
        }).WithTags("Notifications");

        app.MapPost("/notifications", async (CreateNotificationRequest req, NotificationRepository repo) =>
        {
            if (req.UserId <= 0)
                return Results.BadRequest(new { message = "UserId es obligatorio" });
            if (string.IsNullOrWhiteSpace(req.Type))
                return Results.BadRequest(new { message = "El tipo es obligatorio" });
            if (string.IsNullOrWhiteSpace(req.Message))
                return Results.BadRequest(new { message = "El mensaje es obligatorio" });

            var notification = await repo.CreateAsync(req);
            return Results.Created($"/notifications/{notification.Id}", notification);
        }).WithTags("Notifications");

        app.MapPatch("/notifications/{id}/send", async (long id, NotificationRepository repo) =>
        {
            var notification = await repo.MarkAsSentAsync(id);
            if (notification is null)
                return Results.NotFound(new { message = $"Notification con id {id} no encontrada" });
            return Results.Ok(notification);
        }).WithTags("Notifications");
    }
}
