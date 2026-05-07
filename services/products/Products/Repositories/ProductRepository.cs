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

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Product>("""
            SELECT id, name, description, price, stock,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products ORDER BY id DESC
        """);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Product>("""
            SELECT id, name, description, price, stock,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products WHERE id = @Id
        """, new { Id = id });
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        using var conn = CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO products (name, description, price, stock)
            VALUES (@Name, @Description, @Price, @Stock);
            SELECT last_insert_rowid();
        """, request);
        return (await GetByIdAsync(id))!;
    }

    public async Task<Product?> UpdateStockAsync(int id, int newStock)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE products SET stock = @Stock, updated_at = datetime('now')
            WHERE id = @Id
        """, new { Id = id, Stock = newStock });
        return await GetByIdAsync(id);
    }
}