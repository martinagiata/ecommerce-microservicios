using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;

namespace Notifications.API.Services;

public interface INotificationService
{
    Task<NotificationResponse> SendAsync(SendNotificationRequest req, string correlationId);
    Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;
    private readonly string _usersUrl;

    public NotificationService(IConfiguration config, HttpClient httpClient)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db";
        _httpClient = httpClient;
        _usersUrl = config["ServicesUrls:Users"] ?? "http://localhost:5012";
        Initialize();
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute(@"CREATE TABLE IF NOT EXISTS notifications (
            id           TEXT PRIMARY KEY,
            usuario_id   TEXT NOT NULL,
            mensaje      TEXT NOT NULL,
            tipo         TEXT NOT NULL,
            estado       TEXT NOT NULL DEFAULT 'Enviada',
            fecha_envio  TEXT NOT NULL
        );");
    }

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest req, string correlationId)
    {
        var validTipos = new[] { "Email", "Push", "SMS" };
        if (!validTipos.Contains(req.Tipo))
            throw new ValidationException("NTF-002", "Los datos de la notificacion son invalidos.");

        _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
        _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

        var userResp = await _httpClient.GetAsync($"{_usersUrl}/api/users/{req.UsuarioId}");
        if (!userResp.IsSuccessStatusCode)
            throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

        var id = Guid.NewGuid();
        var now = DateTime.UtcNow.ToString("o");

        using var conn = CreateConnection();
        await conn.ExecuteAsync(@"INSERT INTO notifications (id, usuario_id, mensaje, tipo, estado, fecha_envio)
            VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, 'Enviada', @FechaEnvio)",
            new { Id = id.ToString(), UsuarioId = req.UsuarioId.ToString(), req.Mensaje, req.Tipo, FechaEnvio = now });

        return await FindAsync(id);
    }

    public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId)
    {
        using var conn = CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(@"SELECT id AS Id, usuario_id AS UsuarioId, mensaje AS Mensaje,
            tipo AS Tipo, estado AS Estado, fecha_envio AS FechaEnvio
            FROM notifications WHERE usuario_id = @UserId ORDER BY fecha_envio DESC",
            new { UserId = userId.ToString() });

        var list = rows.ToList();
        if (!list.Any())
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

        return list.Select(r => new NotificationResponse
        {
            Id = Guid.Parse((string)r.Id),
            UsuarioId = Guid.Parse((string)r.UsuarioId),
            Mensaje = (string)r.Mensaje,
            Tipo = (string)r.Tipo,
            Estado = (string)r.Estado,
            FechaEnvio = DateTime.Parse((string)r.FechaEnvio)
        });
    }

    private async Task<NotificationResponse> FindAsync(Guid id)
    {
        using var conn = CreateConnection();
        var row = await conn.QueryFirstAsync<dynamic>(@"SELECT id AS Id, usuario_id AS UsuarioId, mensaje AS Mensaje,
            tipo AS Tipo, estado AS Estado, fecha_envio AS FechaEnvio
            FROM notifications WHERE id = @Id",
            new { Id = id.ToString() });

        return new NotificationResponse
        {
            Id = Guid.Parse((string)row.Id),
            UsuarioId = Guid.Parse((string)row.UsuarioId),
            Mensaje = (string)row.Mensaje,
            Tipo = (string)row.Tipo,
            Estado = (string)row.Estado,
            FechaEnvio = DateTime.Parse((string)row.FechaEnvio)
        };
    }
}
