using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class UnauthorizedExceptionHandler(ILogger<UnauthorizedExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedException ex) return false;
        logger.LogWarning("Credenciales incorrectas. errorCode={ErrorCode} path={Path}", ex.ErrorCode, context.Request.Path);
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(ProblemResponseFactory.Build(context, 401, ex.ErrorCode, ex.Message), cancellationToken: cancellationToken);
        return true;
    }
}
