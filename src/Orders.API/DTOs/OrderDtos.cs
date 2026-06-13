using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public record CreateOrderRequest
{
    [Required]
    public Guid UsuarioId { get; init; }

    [Required, MinLength(1)]
    public List<OrderItemRequest> Items { get; init; } = new();
}

public record OrderItemRequest
{
    [Required]
    public Guid ProductoId { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int Cantidad { get; init; }
}

public record UpdateOrderStatusRequest
{
    [Required]
    public string Estado { get; init; } = string.Empty;
}

public record OrderItemResponse
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}

public record OrderResponse
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public List<OrderItemResponse> Items { get; init; } = new();
    public decimal Total { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaActualizacion { get; init; }
}

public record UserInfo
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record ProductInfo
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
}
