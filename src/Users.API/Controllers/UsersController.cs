using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Services;

namespace Users.API.Controllers;

/// <summary>
/// Registro y autenticacion de usuarios.
/// </summary>
[ApiController]
[Route("api/users")]
[Tags("Users")]
public class UsersController(IUserService service) : ControllerBase
{
    /// <summary>Registra un nuevo usuario.</summary>
    /// <response code="201">Usuario registrado exitosamente</response>
    /// <response code="400">Datos invalidos (USR-002)</response>
    /// <response code="409">Email ya registrado (USR-001)</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            throw new ValidationException("USR-002", string.Join("; ", errors));
        }

        var user = await service.RegisterAsync(req);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }

    /// <summary>Autentica un usuario con email y password.</summary>
    /// <response code="200">Login exitoso</response>
    /// <response code="400">Datos invalidos (USR-002)</response>
    /// <response code="401">Credenciales incorrectas (USR-003)</response>
    /// <response code="403">Usuario bloqueado (USR-004 / USR-005)</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            throw new ValidationException("USR-002", string.Join("; ", errors));
        }

        var user = await service.LoginAsync(req);
        return Ok(user);
    }
}
