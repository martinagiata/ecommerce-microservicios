using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

public class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException ex) return false;

        logger.LogWarning("Recurso no encontrado. errorCode={ErrorCode} path={Path}", ex.ErrorCode, context.Request.Path);

        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(
            ProblemResponseFactory.Build(context, 404, ex.ErrorCode, ex.Message),
            cancellationToken: cancellationToken);
        return true;
    }
}
