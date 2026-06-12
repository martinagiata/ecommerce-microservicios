using Users.Models;
using Users.Repositories;

namespace Users.Extensiones;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users", async (UserRepository repo) =>
        {
            var users = await repo.GetAllAsync();
            return Results.Ok(users);
        }).WithTags("Users");

        app.MapGet("/users/{id}", async (long id, UserRepository repo) =>
        {
            var user = await repo.GetByIdAsync(id);
            if (user is null)
                return Results.NotFound(new { errorCode = "USER_NOT_FOUND", errorMessage = $"User con id {id} no encontrado" });
            return Results.Ok(user);
        }).WithTags("Users");

        app.MapPost("/users", async (CreateUserRequest req, UserRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { errorCode = "INVALID_NAME", errorMessage = "El nombre es obligatorio" });
            if (string.IsNullOrWhiteSpace(req.Email))
                return Results.BadRequest(new { errorCode = "INVALID_EMAIL", errorMessage = "El email es obligatorio" });

            var user = await repo.CreateAsync(req);
            return Results.Created($"/users/{user.Id}", user);
        }).WithTags("Users");

        app.MapPut("/users/{id}", async (long id, UpdateUserRequest req, UserRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { errorCode = "INVALID_NAME", errorMessage = "El nombre es obligatorio" });
            if (string.IsNullOrWhiteSpace(req.Email))
                return Results.BadRequest(new { errorCode = "INVALID_EMAIL", errorMessage = "El email es obligatorio" });

            var user = await repo.UpdateAsync(id, req);
            if (user is null)
                return Results.NotFound(new { errorCode = "USER_NOT_FOUND", errorMessage = $"User con id {id} no encontrado" });
            return Results.Ok(user);
        }).WithTags("Users");

        app.MapDelete("/users/{id}", async (long id, UserRepository repo) =>
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound(new { errorCode = "USER_NOT_FOUND", errorMessage = $"User con id {id} no encontrado" });
            return Results.Ok(new { message = $"User con id {id} eliminado correctamente" });
        }).WithTags("Users");
    }
}
