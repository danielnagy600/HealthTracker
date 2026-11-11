using HealthTracker.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Api;

// Egy validator (Application réteg) elutasította a kérést: 400-as ProblemDetails
// lesz belőle, ugyanabban a formában, ahogy korábban a Controllerek válaszoltak.
internal sealed class ValidationExceptionHandler(
    IProblemDetailsService problemDetails, ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validation)
            return false;

        var detail = string.Join(" ", validation.Errors);
        Log.RequestRejected(logger, httpContext.Request.Method, httpContext.Request.Path.Value, detail);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = detail
            }
        });
    }
}
