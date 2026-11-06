namespace Orders.API.Exceptions;

public static class ErrorCodes
{
    public const string ORD_001 = "ORD-001"; // Orden no encontrada
    public const string ORD_002 = "ORD-002"; // Datos de la orden invalidos
    public const string ORD_003 = "ORD-003"; // Usuario no encontrado al crear orden
    public const string ORD_004 = "ORD-004"; // Producto no encontrado al crear orden
    public const string ORD_005 = "ORD-005"; // Stock insuficiente
    public const string ORD_006 = "ORD-006"; // Transicion de estado invalida
    public const string ORD_007 = "ORD-007"; // Error interno
}
