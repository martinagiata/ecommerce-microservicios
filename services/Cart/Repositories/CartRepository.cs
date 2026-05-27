using Dapper;
using Microsoft.Data.Sqlite;
using Cart.Models;

namespace Cart.Repositories;

public class CartRepository
{
    private readonly string _connectionString;

    public CartRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=cart.db";
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    public void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS cart_items (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id      INTEGER NOT NULL,
                product_id   INTEGER NOT NULL,
                product_name TEXT    NOT NULL,
                price        REAL    NOT NULL DEFAULT 0,
                quantity     INTEGER NOT NULL DEFAULT 1,
                created_at   TEXT    NOT NULL DEFAULT (datetime('now')),
                updated_at   TEXT
            );
        """);
    }

    public async Task<IEnumerable<CartItem>> GetByUserIdAsync(long userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<CartItem>("""
            SELECT id, user_id AS UserId, product_id AS ProductId,
                   product_name AS ProductName, price, quantity,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM cart_items WHERE user_id = @UserId
        """, new { UserId = userId });
    }

    public async Task<CartItem?> GetByIdAsync(long id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<CartItem>("""
            SELECT id, user_id AS UserId, product_id AS ProductId,
                   product_name AS ProductName, price, quantity,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM cart_items WHERE id = @Id
        """, new { Id = id });
    }

    public async Task<CartItem> AddAsync(AddCartItemRequest req)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<long>("""
            INSERT INTO cart_items (user_id, product_id, product_name, price, quantity)
            VALUES (@UserId, @ProductId, @ProductName, @Price, @Quantity);
            SELECT last_insert_rowid();
        """, req);
        return (await GetByIdAsync(id))!;
    }

    public async Task<CartItem?> UpdateQuantityAsync(long id, int quantity)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE cart_items SET quantity = @Quantity, updated_at = datetime('now')
            WHERE id = @Id
        """, new { Id = id, Quantity = quantity });
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM cart_items WHERE id = @Id", new { Id = id });
        return rows > 0;
    }

    public async Task<bool> ClearByUserIdAsync(long userId)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM cart_items WHERE user_id = @UserId", new { UserId = userId });
        return rows > 0;
    }
}
