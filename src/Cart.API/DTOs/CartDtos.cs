using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

public record AddItemRequest
{
    [Required]
    public Guid ProductoId { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int Cantidad { get; init; }
}

public record UpdateItemRequest
{
    [Required, Range(1, int.MaxValue)]
    public int Cantidad { get; init; }
}

public record CartItemResponse
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
}

public record CartResponse
{
    public Guid UsuarioId { get; init; }
    public List<CartItemResponse> Items { get; init; } = new();
    public DateTime FechaActualizacion { get; init; }
}

public record ProductInfo
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
}
