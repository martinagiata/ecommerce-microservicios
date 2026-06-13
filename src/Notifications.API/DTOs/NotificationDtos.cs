using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs;

public record SendNotificationRequest
{
    [Required]
    public Guid UsuarioId { get; init; }

    [Required, MaxLength(500)]
    public string Mensaje { get; init; } = string.Empty;

    [Required]
    public string Tipo { get; init; } = string.Empty;
}

public record NotificationResponse
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaEnvio { get; init; }
}
