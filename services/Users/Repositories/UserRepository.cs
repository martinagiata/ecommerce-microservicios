using Dapper;
using Microsoft.Data.Sqlite;
using Users.Models;

namespace Users.Repositories;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=users.db";
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    public void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                name        TEXT    NOT NULL,
                email       TEXT    NOT NULL UNIQUE,
                phone       TEXT,
                created_at  TEXT    NOT NULL DEFAULT (datetime('now')),
                updated_at  TEXT
            );
        """);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<User>("""
            SELECT id, name, email, phone,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users ORDER BY id DESC
        """);
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>("""
            SELECT id, name, email, phone,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users WHERE id = @Id
        """, new { Id = id });
    }

    public async Task<User> CreateAsync(CreateUserRequest req)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<long>("""
            INSERT INTO users (name, email, phone)
            VALUES (@Name, @Email, @Phone);
            SELECT last_insert_rowid();
        """, req);
        return (await GetByIdAsync(id))!;
    }

    public async Task<User?> UpdateAsync(long id, UpdateUserRequest req)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE users SET name = @Name, email = @Email,
                phone = @Phone, updated_at = datetime('now')
            WHERE id = @Id
        """, new { Id = id, req.Name, req.Email, req.Phone });
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM users WHERE id = @Id", new { Id = id });
        return rows > 0;
    }
}
