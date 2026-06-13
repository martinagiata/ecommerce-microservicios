using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex) return false;
        logger.LogWarning("Regla de negocio violada. errorCode={ErrorCode} path={Path}", ex.ErrorCode, context.Request.Path);
        context.Response.StatusCode = ex.StatusCode;
        await context.Response.WriteAsJsonAsync(ProblemResponseFactory.Build(context, ex.StatusCode, ex.ErrorCode, ex.Message), cancellationToken: cancellationToken);
        return true;
    }
}
