using Users.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Users.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error inesperado. path={Path}", context.Request.Path);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(ProblemResponseFactory.Build(context, 500, ErrorCodes.USR_006, "Error interno al procesar el usuario."), cancellationToken: cancellationToken);
        return true;
    }
}
