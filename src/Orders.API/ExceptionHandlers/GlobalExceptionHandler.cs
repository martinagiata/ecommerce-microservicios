using Microsoft.AspNetCore.Diagnostics;

namespace Orders.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error inesperado. path={Path}", context.Request.Path);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(ProblemResponseFactory.Build(context, 500, "ORD-007", "Error interno al procesar la orden."), cancellationToken: cancellationToken);
        return true;
    }
}
