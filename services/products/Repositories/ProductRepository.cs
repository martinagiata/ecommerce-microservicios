using Dapper;
using Microsoft.Data.Sqlite;
using Products.Models;

namespace Products.Repositories;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    public void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS products (
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

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Product>("""
            SELECT id, name, description, price, stock,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products ORDER BY id DESC
        """);
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Product>("""
            SELECT id, name, description, price, stock,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products WHERE id = @Id
        """, new { Id = id });
    }

    public async Task<Product> CreateAsync(CreateProductRequest req)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<long>("""
            INSERT INTO products (name, description, price, stock)
            VALUES (@Name, @Description, @Price, @Stock);
            SELECT last_insert_rowid();
        """, req);
        return (await GetByIdAsync(id))!;
    }

    public async Task<Product?> UpdateAsync(long id, UpdateProductRequest req)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE products SET name = @Name, description = @Description,
                price = @Price, stock = @Stock, updated_at = datetime('now')
            WHERE id = @Id
        """, new { Id = id, req.Name, req.Description, req.Price, req.Stock });
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM products WHERE id = @Id", new { Id = id });
        return rows > 0;
    }
}
