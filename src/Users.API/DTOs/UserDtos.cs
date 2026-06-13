using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

public record RegisterRequest
{
    [Required]
    public string Nombre { get; init; } = string.Empty;

    [Required]
    public string Apellido { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; init; } = string.Empty;
}

public record LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record UserResponse
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime FechaRegistro { get; init; }
    public bool Activo { get; init; }
}
