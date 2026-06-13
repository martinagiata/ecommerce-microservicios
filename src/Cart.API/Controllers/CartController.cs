using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Services;

namespace Cart.API.Controllers;

/// <summary>
/// Gestion del carrito de compras.
/// </summary>
[ApiController]
[Route("api/cart")]
[Tags("Cart")]
public class CartController(ICartService service) : ControllerBase
{
    private string CorrelationId => HttpContext.Items["CorrelationId"] as string ?? HttpContext.TraceIdentifier;

    /// <summary>Obtiene el carrito del usuario.</summary>
    /// <response code="200">Carrito encontrado</response>
    /// <response code="404">Carrito no encontrado (CRT-001)</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCart(Guid userId)
    {
        var cart = await service.GetByUserIdAsync(userId, CorrelationId);
        return Ok(cart);
    }

    /// <summary>Agrega un producto al carrito.</summary>
    /// <response code="200">Item agregado</response>
    /// <response code="400">Cantidad invalida (CRT-004)</response>
    /// <response code="404">Producto no encontrado (CRT-002)</response>
    /// <response code="422">Stock insuficiente (CRT-003)</response>
    [HttpPost("{userId}/items")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> AddItem(Guid userId, [FromBody] AddItemRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            throw new ValidationException("CRT-004", string.Join("; ", errors));
        }
        var cart = await service.AddItemAsync(userId, req, CorrelationId);
        return Ok(cart);
    }

    /// <summary>Actualiza la cantidad de un item del carrito.</summary>
    /// <response code="200">Cantidad actualizada</response>
    /// <response code="400">Cantidad invalida (CRT-004)</response>
    /// <response code="404">Carrito o producto no encontrado (CRT-001 / CRT-002)</response>
    /// <response code="422">Stock insuficiente (CRT-003)</response>
    [HttpPut("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateItemRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            throw new ValidationException("CRT-004", string.Join("; ", errors));
        }
        var cart = await service.UpdateItemAsync(userId, productId, req, CorrelationId);
        return Ok(cart);
    }

    /// <summary>Quita un producto del carrito.</summary>
    /// <response code="204">Producto eliminado del carrito</response>
    /// <response code="404">Carrito no encontrado (CRT-001)</response>
    [HttpDelete("{userId}/items/{productId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteItem(Guid userId, Guid productId)
    {
        await service.DeleteItemAsync(userId, productId);
        return NoContent();
    }

    /// <summary>Vacia el carrito completo del usuario.</summary>
    /// <response code="204">Carrito vaciado</response>
    /// <response code="404">Carrito no encontrado (CRT-001)</response>
    [HttpDelete("{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCart(Guid userId)
    {
        await service.DeleteCartAsync(userId);
        return NoContent();
    }
}
