using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre);
    Task<ProductResponse> GetByIdAsync(Guid id);
    Task<ProductResponse> CreateAsync(CreateProductRequest req);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest req);
    Task DeleteAsync(Guid id);
}

public class ProductService : IProductService
{
    private readonly string _connectionString;

    public ProductService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=products.db";
        Initialize();
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                id            TEXT    PRIMARY KEY,
                nombre        TEXT    NOT NULL,
                descripcion   TEXT,
                precio        REAL    NOT NULL,
                stock         INTEGER NOT NULL,
                categoria     TEXT    NOT NULL,
                fecha_creacion TEXT   NOT NULL
            );
        """);
    }

    private static ProductResponse ToResponse(Product p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Precio = p.Precio,
        Stock = p.Stock,
        Categoria = p.Categoria,
        FechaCreacion = p.FechaCreacion
    };

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre)
    {
        using var conn = CreateConnection();
        var sql = """
            SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion,
                   precio AS Precio, stock AS Stock, categoria AS Categoria,
                   fecha_creacion AS FechaCreacion
            FROM products WHERE 1=1
        """;
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            sql += " AND categoria = @Categoria";
            parameters.Add("Categoria", categoria);
        }
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            sql += " AND nombre LIKE @Nombre";
            parameters.Add("Nombre", $"%{nombre}%");
        }

        var rows = await conn.QueryAsync<dynamic>(sql, parameters);
        return rows.Select(r => new ProductResponse
        {
            Id = Guid.Parse((string)r.Id),
            Nombre = (string)r.Nombre,
            Descripcion = r.Descripcion is null ? null : (string)r.Descripcion,
            Precio = (decimal)(double)r.Precio,
            Stock = (int)(long)r.Stock,
            Categoria = (string)r.Categoria,
            FechaCreacion = DateTime.Parse((string)r.FechaCreacion)
        });
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await FindAsync(id);
        if (product is null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        return product;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest req)
    {
        using var conn = CreateConnection();

        var existingId = await conn.ExecuteScalarAsync<string>(
            "SELECT id FROM products WHERE nombre = @Nombre AND categoria = @Categoria",
            new { req.Nombre, req.Categoria });

        if (existingId is not null)
            throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{req.Categoria}'.", statusCode: 409);

        var id = Guid.NewGuid();
        var now = DateTime.UtcNow.ToString("o");

        await conn.ExecuteAsync("""
            INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion)
            VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
        """, new
        {
            Id = id.ToString(),
            req.Nombre,
            req.Descripcion,
            req.Precio,
            req.Stock,
            req.Categoria,
            FechaCreacion = now
        });

        return (await FindAsync(id))!;
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest req)
    {
        var existing = await FindAsync(id);
        if (existing is null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE products SET nombre = @Nombre, descripcion = @Descripcion,
                precio = @Precio, stock = @Stock, categoria = @Categoria
            WHERE id = @Id
        """, new
        {
            Id = id.ToString(),
            req.Nombre,
            req.Descripcion,
            req.Precio,
            req.Stock,
            req.Categoria
        });

        return (await FindAsync(id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await FindAsync(id);
        if (existing is null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        using var conn = CreateConnection();
        await conn.ExecuteAsync("DELETE FROM products WHERE id = @Id", new { Id = id.ToString() });
    }

    private async Task<ProductResponse?> FindAsync(Guid id)
    {
        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>("""
            SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion,
                   precio AS Precio, stock AS Stock, categoria AS Categoria,
                   fecha_creacion AS FechaCreacion
            FROM products WHERE id = @Id
        """, new { Id = id.ToString() });

        if (row is null) return null;

        return new ProductResponse
        {
            Id = Guid.Parse((string)row.Id),
            Nombre = (string)row.Nombre,
            Descripcion = row.Descripcion is null ? null : (string)row.Descripcion,
            Precio = (decimal)(double)row.Precio,
            Stock = (int)(long)row.Stock,
            Categoria = (string)row.Categoria,
            FechaCreacion = DateTime.Parse((string)row.FechaCreacion)
        };
    }
}
