using Dapper;
using Microsoft.Data.Sqlite;
using Cart.API.DTOs;
using Cart.API.Exceptions;

namespace Cart.API.Services;

public interface ICartService
{
    Task<CartResponse> GetByUserIdAsync(Guid userId, string correlationId);
    Task<CartResponse> AddItemAsync(Guid userId, AddItemRequest req, string correlationId);
    Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateItemRequest req, string correlationId);
    Task DeleteItemAsync(Guid userId, Guid productId);
    Task DeleteCartAsync(Guid userId);
}

public class CartService : ICartService
{
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;
    private readonly string _productsUrl;

    public CartService(IConfiguration config, HttpClient httpClient)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db";
        _httpClient = httpClient;
        _productsUrl = config["ServicesUrls:Products"] ?? "http://localhost:5280";
        Initialize();
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute(@"CREATE TABLE IF NOT EXISTS cart_items (
            usuario_id   TEXT NOT NULL,
            producto_id  TEXT NOT NULL,
            cantidad     INTEGER NOT NULL,
            fecha_actualizacion TEXT NOT NULL,
            PRIMARY KEY (usuario_id, producto_id)
        );");
    }

    private async Task<ProductInfo?> GetProductAsync(Guid productId, string correlationId)
    {
        _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
        _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
        var resp = await _httpClient.GetAsync($"{_productsUrl}/api/products/{productId}");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ProductInfo>();
    }

    private async Task<CartResponse> BuildResponseAsync(Guid userId)
    {
        using var conn = CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            @"SELECT producto_id AS ProductoId, cantidad AS Cantidad, fecha_actualizacion AS FechaActualizacion
              FROM cart_items WHERE usuario_id = @UserId",
            new { UserId = userId.ToString() });

        var list = rows.ToList();
        if (!list.Any())
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        return new CartResponse
        {
            UsuarioId = userId,
            Items = list.Select(r => new CartItemResponse
            {
                ProductoId = Guid.Parse((string)r.ProductoId),
                Cantidad = (int)(long)r.Cantidad
            }).ToList(),
            FechaActualizacion = DateTime.Parse((string)list.First().FechaActualizacion)
        };
    }

    public async Task<CartResponse> GetByUserIdAsync(Guid userId, string correlationId)
    {
        return await BuildResponseAsync(userId);
    }

    public async Task<CartResponse> AddItemAsync(Guid userId, AddItemRequest req, string correlationId)
    {
        if (req.Cantidad <= 0)
            throw new ValidationException("CRT-004", "Cantidad invalida.");

        var product = await GetProductAsync(req.ProductoId, correlationId);
        if (product is null)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        if (product.Stock < req.Cantidad)
            throw new BusinessRuleException("CRT-003",
                $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {req.Cantidad}.", statusCode: 422);

        using var conn = CreateConnection();
        var now = DateTime.UtcNow.ToString("o");

        await conn.ExecuteAsync(@"INSERT INTO cart_items (usuario_id, producto_id, cantidad, fecha_actualizacion)
            VALUES (@UserId, @ProductoId, @Cantidad, @FechaActualizacion)
            ON CONFLICT(usuario_id, producto_id) DO UPDATE SET
                cantidad = cantidad + @Cantidad,
                fecha_actualizacion = @FechaActualizacion",
            new { UserId = userId.ToString(), ProductoId = req.ProductoId.ToString(), req.Cantidad, FechaActualizacion = now });

        return await BuildResponseAsync(userId);
    }

    public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateItemRequest req, string correlationId)
    {
        if (req.Cantidad <= 0)
            throw new ValidationException("CRT-004", "Cantidad invalida.");

        var product = await GetProductAsync(productId, correlationId);
        if (product is null)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        if (product.Stock < req.Cantidad)
            throw new BusinessRuleException("CRT-003",
                $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {req.Cantidad}.", statusCode: 422);

        using var conn = CreateConnection();
        var now = DateTime.UtcNow.ToString("o");

        var rows = await conn.ExecuteAsync(@"UPDATE cart_items SET cantidad = @Cantidad, fecha_actualizacion = @FechaActualizacion
            WHERE usuario_id = @UserId AND producto_id = @ProductoId",
            new { UserId = userId.ToString(), ProductoId = productId.ToString(), req.Cantidad, FechaActualizacion = now });

        if (rows == 0)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        return await BuildResponseAsync(userId);
    }

    public async Task DeleteItemAsync(Guid userId, Guid productId)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM cart_items WHERE usuario_id = @UserId AND producto_id = @ProductoId",
            new { UserId = userId.ToString(), ProductoId = productId.ToString() });

        if (rows == 0)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
    }

    public async Task DeleteCartAsync(Guid userId)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM cart_items WHERE usuario_id = @UserId",
            new { UserId = userId.ToString() });

        if (rows == 0)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
    }
}
