namespace Users.API.Exceptions;

public static class ErrorCodes
{
    public const string USR_001 = "USR-001"; // Email ya registrado
    public const string USR_002 = "USR-002"; // Datos de usuario invalidos / usuario no encontrado
    public const string USR_003 = "USR-003"; // Credenciales incorrectas
    public const string USR_004 = "USR-004"; // Cuenta bloqueada por intentos fallidos
    public const string USR_005 = "USR-005"; // Cuenta suspendida
    public const string USR_006 = "USR-006"; // Error interno
}
