using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Services;

namespace Products.API.Controllers;

/// <summary>
/// Gestion del catalogo de productos.
/// </summary>
[ApiController]
[Route("api/products")]
[Tags("Products")]
public class ProductsController(IProductService service) : ControllerBase
{
    /// <summary>Lista los productos, con filtros opcionales por categoria y nombre.</summary>
    /// <response code="200">Listado de productos</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? categoria, [FromQuery] string? nombre)
    {
        var products = await service.GetAllAsync(categoria, nombre);
        return Ok(products);
    }

    /// <summary>Obtiene un producto por su Id.</summary>
    /// <response code="200">Producto encontrado</response>
    /// <response code="404">Producto no encontrado (PRD-001)</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await service.GetByIdAsync(id);
        return Ok(product);
    }

    /// <summary>Crea un nuevo producto.</summary>
    /// <response code="201">Producto creado</response>
    /// <response code="400">Datos invalidos (PRD-002)</response>
    /// <response code="409">Producto duplicado en la categoria (PRD-003)</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
    {
        if (!ModelState.IsValid)
            throw new ValidationException("PRD-002", BuildValidationMessage());

        var product = await service.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Actualiza un producto existente.</summary>
    /// <response code="200">Producto actualizado</response>
    /// <response code="400">Datos invalidos (PRD-002)</response>
    /// <response code="404">Producto no encontrado (PRD-001)</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest req)
    {
        if (!ModelState.IsValid)
            throw new ValidationException("PRD-002", BuildValidationMessage());

        var product = await service.UpdateAsync(id, req);
        return Ok(product);
    }

    /// <summary>Elimina un producto.</summary>
    /// <response code="204">Producto eliminado</response>
    /// <response code="404">Producto no encontrado (PRD-001)</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    private string BuildValidationMessage()
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage);
        return string.Join("; ", errors);
    }
}
