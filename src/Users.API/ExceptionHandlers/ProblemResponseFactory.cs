namespace Users.API.ExceptionHandlers;

public static class ProblemResponseFactory
{
    public static object Build(HttpContext context, int status, string errorCode, string errorMessage)
    {
        var (type, title, detail) = status switch
        {
            400 => ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "Los datos enviados son inválidos."),
            401 => ("https://tools.ietf.org/html/rfc7235#section-3.1", "Unauthorized", "Las credenciales no son válidas."),
            403 => ("https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden", "El acceso está prohibido."),
            404 => ("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado."),
            409 => ("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "Ya existe un recurso con esos datos."),
            _ => ("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error inesperado.")
        };

        var correlationId = context.Items["CorrelationId"] as string ?? context.TraceIdentifier;

        return new { type, title, status, detail, instance = context.Request.Path.Value, errorCode, errorMessage, correlationId };
    }
}
