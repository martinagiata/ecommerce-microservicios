using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException ex) return false;
        logger.LogWarning("Acceso prohibido. errorCode={ErrorCode} path={Path}", ex.ErrorCode, context.Request.Path);
        context.Response.StatusCode = 403;
        await context.Response.WriteAsJsonAsync(ProblemResponseFactory.Build(context, 403, ex.ErrorCode, ex.Message), cancellationToken: cancellationToken);
        return true;
    }
}
