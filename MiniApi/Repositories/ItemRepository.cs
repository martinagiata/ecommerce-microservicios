using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Repositories;

public class ItemRepository
{
    private readonly string _connectionString;

    public ItemRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=app.db";
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    public void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS items (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                name        TEXT    NOT NULL,
                description TEXT,
                price       REAL    NOT NULL DEFAULT 0,
                stock       INTEGER NOT NULL DEFAULT 0,
                created_at  TEXT    NOT NULL DEFAULT (datetime('now')),
                updated_at  TEXT
            );
        """);
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Item>("""
            SELECT id, name, description, price, stock,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM items ORDER BY id DESC
        """);
    }

    public async Task<Item?> GetByIdAsync(long id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Item>("""
            SELECT id, name, description, price, stock,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM items WHERE id = @Id
        """, new { Id = id });
    }

    public async Task<Item> CreateAsync(CreateItemRequest req)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<long>("""
            INSERT INTO items (name, description, price, stock)
            VALUES (@Name, @Description, @Price, @Stock);
            SELECT last_insert_rowid();
        """, req);
        return (await GetByIdAsync(id))!;
    }

    public async Task<Item?> UpdateAsync(long id, UpdateItemRequest req)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE items SET name = @Name, description = @Description,
                price = @Price, stock = @Stock, updated_at = datetime('now')
            WHERE id = @Id
        """, new { Id = id, req.Name, req.Description, req.Price, req.Stock });
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM items WHERE id = @Id", new { Id = id });
        return rows > 0;
    }
}
