using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.Models;

namespace Notifications.Repositories;

public class NotificationRepository
{
    private readonly string _connectionString;

    public NotificationRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=notifications.db";
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    public void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS notifications (
                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id    INTEGER NOT NULL,
                type       TEXT    NOT NULL,
                message    TEXT    NOT NULL,
                status     TEXT    NOT NULL DEFAULT 'pending',
                created_at TEXT    NOT NULL DEFAULT (datetime('now')),
                sent_at    TEXT
            );
        """);
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Notification>("""
            SELECT id, user_id AS UserId, type, message, status,
                   created_at AS CreatedAt, sent_at AS SentAt
            FROM notifications ORDER BY id DESC
        """);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(long userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Notification>("""
            SELECT id, user_id AS UserId, type, message, status,
                   created_at AS CreatedAt, sent_at AS SentAt
            FROM notifications WHERE user_id = @UserId ORDER BY id DESC
        """, new { UserId = userId });
    }

    public async Task<Notification?> GetByIdAsync(long id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Notification>("""
            SELECT id, user_id AS UserId, type, message, status,
                   created_at AS CreatedAt, sent_at AS SentAt
            FROM notifications WHERE id = @Id
        """, new { Id = id });
    }

    public async Task<Notification> CreateAsync(CreateNotificationRequest req)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<long>("""
            INSERT INTO notifications (user_id, type, message)
            VALUES (@UserId, @Type, @Message);
            SELECT last_insert_rowid();
        """, req);
        return (await GetByIdAsync(id))!;
    }

    public async Task<Notification?> MarkAsSentAsync(long id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE notifications SET status = 'sent', sent_at = datetime('now')
            WHERE id = @Id
        """, new { Id = id });
        return await GetByIdAsync(id);
    }
}
