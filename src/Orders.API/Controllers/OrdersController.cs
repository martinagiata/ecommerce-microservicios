using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Services;

namespace Orders.API.Controllers;

/// <summary>
/// Gestion de ordenes de compra.
/// </summary>
[ApiController]
[Route("api/orders")]
[Tags("Orders")]
public class OrdersController(IOrderService service) : ControllerBase
{
    /// <summary>Lista las ordenes, con filtro opcional por usuarioId.</summary>
    /// <response code="200">Listado de ordenes</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? usuarioId)
    {
        var orders = await service.GetAllAsync(usuarioId);
        return Ok(orders);
    }

    /// <summary>Obtiene el detalle de una orden por su Id.</summary>
    /// <response code="200">Orden encontrada</response>
    /// <response code="404">Orden no encontrada (ORD-001)</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await service.GetByIdAsync(id);
        return Ok(order);
    }

    /// <summary>Crea una nueva orden.</summary>
    /// <response code="201">Orden creada</response>
    /// <response code="400">Datos invalidos (ORD-002)</response>
    /// <response code="404">Usuario o producto no encontrado (ORD-003 / ORD-004)</response>
    /// <response code="422">Stock insuficiente (ORD-005)</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            throw new ValidationException("ORD-002", string.Join("; ", errors));
        }

        var correlationId = HttpContext.Items["CorrelationId"] as string ?? HttpContext.TraceIdentifier;
        var order = await service.CreateAsync(req, correlationId);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Actualiza el estado de una orden.</summary>
    /// <response code="200">Estado actualizado</response>
    /// <response code="400">Estado invalido (ORD-002)</response>
    /// <response code="404">Orden no encontrada (ORD-001)</response>
    /// <response code="409">Transicion de estado invalida (ORD-006)</response>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            throw new ValidationException("ORD-002", string.Join("; ", errors));
        }

        var order = await service.UpdateStatusAsync(id, req.Estado);
        return Ok(order);
    }
}
