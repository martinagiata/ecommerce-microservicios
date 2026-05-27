using Dapper;
using Microsoft.Data.Sqlite;
using Orders.Models;

namespace Orders.Repositories;

public class OrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=orders.db";
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    public void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS orders (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id     INTEGER NOT NULL,
                user_name   TEXT    NOT NULL,
                user_email  TEXT    NOT NULL,
                status      TEXT    NOT NULL DEFAULT 'pending',
                total       REAL    NOT NULL DEFAULT 0,
                created_at  TEXT    NOT NULL DEFAULT (datetime('now')),
                updated_at  TEXT
            );
        """);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Order>("""
            SELECT id, user_id AS UserId, user_name AS UserName,
                   user_email AS UserEmail, status, total,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM orders ORDER BY id DESC
        """);
    }

    public async Task<Order?> GetByIdAsync(long id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Order>("""
            SELECT id, user_id AS UserId, user_name AS UserName,
                   user_email AS UserEmail, status, total,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM orders WHERE id = @Id
        """, new { Id = id });
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(long userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Order>("""
            SELECT id, user_id AS UserId, user_name AS UserName,
                   user_email AS UserEmail, status, total,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM orders WHERE user_id = @UserId ORDER BY id DESC
        """, new { UserId = userId });
    }

    public async Task<Order> CreateAsync(CreateOrderRequest req, UserInfo user)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<long>("""
            INSERT INTO orders (user_id, user_name, user_email, total)
            VALUES (@UserId, @UserName, @UserEmail, @Total);
            SELECT last_insert_rowid();
        """, new { req.UserId, UserName = user.Name, UserEmail = user.Email, req.Total });
        return (await GetByIdAsync(id))!;
    }

    public async Task<Order?> UpdateStatusAsync(long id, string status)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE orders SET status = @Status, updated_at = datetime('now')
            WHERE id = @Id
        """, new { Id = id, Status = status });
        return await GetByIdAsync(id);
    }
}
