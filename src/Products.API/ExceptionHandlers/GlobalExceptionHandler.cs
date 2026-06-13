using Microsoft.AspNetCore.Diagnostics;

namespace Products.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error inesperado. path={Path}", context.Request.Path);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(
            ProblemResponseFactory.Build(context, 500, "PRD-005", "Error interno al procesar el producto."),
            cancellationToken: cancellationToken);
        return true;
    }
}
