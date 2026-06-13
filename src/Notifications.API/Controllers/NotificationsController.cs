using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

/// <summary>
/// Envio y consulta de notificaciones.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Tags("Notifications")]
public class NotificationsController(INotificationService service) : ControllerBase
{
    private string CorrelationId => HttpContext.Items["CorrelationId"] as string ?? HttpContext.TraceIdentifier;

    /// <summary>Registra y simula el envio de una notificacion.</summary>
    /// <response code="201">Notificacion enviada</response>
    /// <response code="400">Datos invalidos (NTF-002)</response>
    /// <response code="404">Usuario no encontrado (NTF-001)</response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            throw new ValidationException("NTF-002", string.Join("; ", errors));
        }
        var notification = await service.SendAsync(req, CorrelationId);
        return CreatedAtAction(nameof(GetByUserId), new { userId = notification.UsuarioId }, notification);
    }

    /// <summary>Lista las notificaciones de un usuario.</summary>
    /// <response code="200">Listado de notificaciones</response>
    /// <response code="404">No se encontraron notificaciones (NTF-003)</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var notifications = await service.GetByUserIdAsync(userId);
        return Ok(notifications);
    }
}
