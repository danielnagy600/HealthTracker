using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Api;

// Az entitások guard-jai ArgumentException-t dobnak érvénytelen adatra. Ez kliens-
// hiba, ezért 400-as ProblemDetails lesz belőle 500 helyett. Minden más kivételt
// továbbengedünk: azokból az AddProblemDetails általános, részletek nélküli 500-at ad.
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ArgumentException argument)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = MessageWithoutParameter(argument)
            }
        });
    }

    // Az ArgumentException.Message a végére fűzi: " (Parameter 'x')" – ez a kliensnek zaj.
    private static string MessageWithoutParameter(ArgumentException exception)
    {
        var suffix = $" (Parameter '{exception.ParamName}')";
        return exception.ParamName is not null && exception.Message.EndsWith(suffix, StringComparison.Ordinal)
            ? exception.Message[..^suffix.Length]
            : exception.Message;
    }
}
