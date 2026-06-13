namespace Orders.API.ExceptionHandlers;

public static class ProblemResponseFactory
{
    public static object Build(HttpContext context, int status, string errorCode, string errorMessage)
    {
        var (type, title, detail) = status switch
        {
            400 => ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "Los datos enviados son inválidos."),
            404 => ("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado."),
            409 => ("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede modificar el estado."),
            422 => ("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
            _ => ("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error inesperado.")
        };

        var correlationId = context.Items["CorrelationId"] as string ?? context.TraceIdentifier;
        return new { type, title, status, detail, instance = context.Request.Path.Value, errorCode, errorMessage, correlationId };
    }
}
