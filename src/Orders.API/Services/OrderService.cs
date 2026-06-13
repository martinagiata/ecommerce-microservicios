using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderResponse>> GetAllAsync(Guid? usuarioId);
    Task<OrderResponse> GetByIdAsync(Guid id);
    Task<OrderResponse> CreateAsync(CreateOrderRequest req, string correlationId);
    Task<OrderResponse> UpdateStatusAsync(Guid id, string estado);
}

public class OrderService : IOrderService
{
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;
    private readonly string _usersUrl;
    private readonly string _productsUrl;

    private static readonly string[] ValidStatuses = ["Pendiente", "Confirmada", "Enviada", "Entregada", "Cancelada"];
    private static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        ["Pendiente"]  = ["Confirmada", "Cancelada"],
        ["Confirmada"] = ["Enviada", "Cancelada"],
        ["Enviada"]    = ["Entregada"],
        ["Entregada"]  = [],
        ["Cancelada"]  = []
    };

    public OrderService(IConfiguration config, HttpClient httpClient)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";
        _httpClient = httpClient;
        _usersUrl = config["ServicesUrls:Users"] ?? "http://localhost:5012";
        _productsUrl = config["ServicesUrls:Products"] ?? "http://localhost:5280";
        Initialize();
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS orders (
                id                  TEXT PRIMARY KEY,
                usuario_id          TEXT NOT NULL,
                total               REAL NOT NULL,
                estado              TEXT NOT NULL DEFAULT 'Pendiente',
                fecha_creacion      TEXT NOT NULL,
                fecha_actualizacion TEXT
            );
            CREATE TABLE IF NOT EXISTS order_items (
                order_id          TEXT NOT NULL,
                producto_id       TEXT NOT NULL,
                cantidad          INTEGER NOT NULL,
                precio_unitario   REAL NOT NULL,
                PRIMARY KEY (order_id, producto_id)
            );
        """);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync(Guid? usuarioId)
    {
        using var conn = CreateConnection();
        var sql = "SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, estado AS Estado, fecha_creacion AS FechaCreacion, fecha_actualizacion AS FechaActualizacion FROM orders";
        if (usuarioId.HasValue) sql += " WHERE usuario_id = @UsuarioId";

        var rows = await conn.QueryAsync<dynamic>(sql, usuarioId.HasValue ? new { UsuarioId = usuarioId.ToString() } : null);
        var result = new List<OrderResponse>();

        foreach (var row in rows)
        {
            var items = await GetItemsAsync(conn, (string)row.Id);
            result.Add(MapResponse(row, items));
        }
        return result;
    }

    public async Task<OrderResponse> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>("""
            SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, estado AS Estado,
                   fecha_creacion AS FechaCreacion, fecha_actualizacion AS FechaActualizacion
            FROM orders WHERE id = @Id
        """, new { Id = id.ToString() });

        if (row is null) throw new NotFoundException("ORD-001", "Orden no encontrada.");

        var items = await GetItemsAsync(conn, (string)row.Id);
        return MapResponse(row, items);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest req, string correlationId)
    {
        if (req.Items == null || req.Items.Count == 0)
            throw new ValidationException("ORD-002", "Los datos de la orden son inválidos.");

        // Verificar usuario via HTTP
        _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
        _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

        var userResp = await _httpClient.GetAsync($"{_usersUrl}/api/users/{req.UsuarioId}");
        if (!userResp.IsSuccessStatusCode)
            throw new NotFoundException("ORD-003", "Usuario no encontrado al crear la orden.");

        // Verificar productos y stock via HTTP
        var itemsData = new List<(OrderItemRequest req, ProductInfo product)>();
        foreach (var item in req.Items)
        {
            var prodResp = await _httpClient.GetAsync($"{_productsUrl}/api/products/{item.ProductoId}");
            if (!prodResp.IsSuccessStatusCode)
                throw new NotFoundException("ORD-004", $"Producto no encontrado al crear la orden.");

            var product = await prodResp.Content.ReadFromJsonAsync<ProductInfo>();
            if (product!.Stock < item.Cantidad)
                throw new BusinessRuleException("ORD-005",
                    $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {item.Cantidad}.",
                    statusCode: 422);

            itemsData.Add((item, product));
        }

        var total = itemsData.Sum(x => x.product.Precio * x.req.Cantidad);
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow.ToString("o");

        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
            VALUES (@Id, @UsuarioId, @Total, 'Pendiente', @FechaCreacion)
        """, new { Id = id.ToString(), UsuarioId = req.UsuarioId.ToString(), Total = total, FechaCreacion = now });

        foreach (var (item, product) in itemsData)
        {
            await conn.ExecuteAsync("""
                INSERT INTO order_items (order_id, producto_id, cantidad, precio_unitario)
                VALUES (@OrderId, @ProductoId, @Cantidad, @PrecioUnitario)
            """, new { OrderId = id.ToString(), ProductoId = item.ProductoId.ToString(), item.Cantidad, PrecioUnitario = product.Precio });
        }

        return await GetByIdAsync(id);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid id, string estado)
    {
        if (!ValidStatuses.Contains(estado))
            throw new ValidationException("ORD-002", "Los datos de la orden son inválidos.");

        var order = await GetByIdAsync(id);

        if (!ValidTransitions[order.Estado].Contains(estado))
            throw new BusinessRuleException("ORD-006",
                $"Una orden en estado '{order.Estado}' no puede pasar a '{estado}'.", statusCode: 409);

        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE orders SET estado = @Estado, fecha_actualizacion = @FechaActualizacion WHERE id = @Id
        """, new { Id = id.ToString(), Estado = estado, FechaActualizacion = DateTime.UtcNow.ToString("o") });

        return await GetByIdAsync(id);
    }

    private static async Task<List<OrderItemResponse>> GetItemsAsync(SqliteConnection conn, string orderId)
    {
        var rows = await conn.QueryAsync<dynamic>("""
            SELECT producto_id AS ProductoId, cantidad AS Cantidad, precio_unitario AS PrecioUnitario
            FROM order_items WHERE order_id = @OrderId
        """, new { OrderId = orderId });

        return rows.Select(r => new OrderItemResponse
        {
            ProductoId = Guid.Parse((string)r.ProductoId),
            Cantidad = (int)(long)r.Cantidad,
            PrecioUnitario = (decimal)(double)r.PrecioUnitario
        }).ToList();
    }

    private static OrderResponse MapResponse(dynamic row, List<OrderItemResponse> items) => new()
    {
        Id = Guid.Parse((string)row.Id),
        UsuarioId = Guid.Parse((string)row.UsuarioId),
        Items = items,
        Total = (decimal)(double)row.Total,
        Estado = (string)row.Estado,
        FechaCreacion = DateTime.Parse((string)row.FechaCreacion),
        FechaActualizacion = row.FechaActualizacion is null ? null : DateTime.Parse((string)row.FechaActualizacion)
    };
}
