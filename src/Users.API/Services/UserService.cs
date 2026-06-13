using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(RegisterRequest req);
    Task<UserResponse> LoginAsync(LoginRequest req);
}

public class UserService : IUserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=users.db";
        Initialize();
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                id                TEXT    PRIMARY KEY,
                nombre            TEXT    NOT NULL,
                apellido          TEXT    NOT NULL,
                email             TEXT    NOT NULL UNIQUE,
                password_hash     TEXT    NOT NULL,
                fecha_registro    TEXT    NOT NULL,
                activo            INTEGER NOT NULL DEFAULT 1,
                intentos_fallidos INTEGER NOT NULL DEFAULT 0
            );
        """);
    }

    private static string HashPassword(string password)
        => Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(password)));

    private static UserResponse ToResponse(dynamic row) => new()
    {
        Id = Guid.Parse((string)row.Id),
        Nombre = (string)row.Nombre,
        Apellido = (string)row.Apellido,
        Email = (string)row.Email,
        FechaRegistro = DateTime.Parse((string)row.FechaRegistro),
        Activo = (long)row.Activo == 1
    };

    public async Task<UserResponse> RegisterAsync(RegisterRequest req)
    {
        using var conn = CreateConnection();

        var existing = await conn.ExecuteScalarAsync<string>(
            "SELECT id FROM users WHERE email = @Email", new { req.Email });

        if (existing is not null)
            throw new BusinessRuleException("USR-001", $"El email '{req.Email}' ya está registrado.", statusCode: 409);

        var id = Guid.NewGuid();
        var hash = HashPassword(req.Password);
        var now = DateTime.UtcNow.ToString("o");

        await conn.ExecuteAsync("""
            INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
            VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, 1, 0)
        """, new { Id = id.ToString(), req.Nombre, req.Apellido, req.Email, PasswordHash = hash, FechaRegistro = now });

        var row = await conn.QueryFirstAsync<dynamic>("""
            SELECT id AS Id, nombre AS Nombre, apellido AS Apellido, email AS Email,
                   fecha_registro AS FechaRegistro, activo AS Activo
            FROM users WHERE id = @Id
        """, new { Id = id.ToString() });

        return ToResponse(row);
    }

    public async Task<UserResponse> LoginAsync(LoginRequest req)
    {
        using var conn = CreateConnection();

        var row = await conn.QueryFirstOrDefaultAsync<dynamic>("""
            SELECT id AS Id, nombre AS Nombre, apellido AS Apellido, email AS Email,
                   password_hash AS PasswordHash, fecha_registro AS FechaRegistro,
                   activo AS Activo, intentos_fallidos AS IntentosFallidos
            FROM users WHERE email = @Email
        """, new { req.Email });

        if (row is null)
            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");

        if ((long)row.Activo == 0)
        {
            if ((long)row.IntentosFallidos >= 3)
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
            throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
        }

        var hash = HashPassword(req.Password);
        if ((string)row.PasswordHash != hash)
        {
            var intentos = (long)row.IntentosFallidos + 1;
            var bloquear = intentos >= 3;

            await conn.ExecuteAsync("""
                UPDATE users SET intentos_fallidos = @Intentos, activo = @Activo WHERE email = @Email
            """, new { Intentos = intentos, Activo = bloquear ? 0 : 1, req.Email });

            if (bloquear)
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        await conn.ExecuteAsync(
            "UPDATE users SET intentos_fallidos = 0 WHERE email = @Email", new { req.Email });

        return ToResponse(row);
    }
}
