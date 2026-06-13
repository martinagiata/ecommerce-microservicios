using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException ex) return false;
        logger.LogWarning("Validacion fallida. errorCode={ErrorCode} path={Path}", ex.ErrorCode, context.Request.Path);
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(ProblemResponseFactory.Build(context, 400, ex.ErrorCode, ex.Message), cancellationToken: cancellationToken);
        return true;
    }
}
