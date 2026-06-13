using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs;

public record CreateProductRequest
{
    [Required, MaxLength(100)]
    public string Nombre { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; init; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Precio { get; init; }

    [Required, Range(0, int.MaxValue)]
    public int Stock { get; init; }

    [Required]
    public string Categoria { get; init; } = string.Empty;
}

public record UpdateProductRequest
{
    [Required, MaxLength(100)]
    public string Nombre { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; init; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Precio { get; init; }

    [Required, Range(0, int.MaxValue)]
    public int Stock { get; init; }

    [Required]
    public string Categoria { get; init; } = string.Empty;
}

public record ProductResponse
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal Precio { get; init; }
    public int Stock { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
}
