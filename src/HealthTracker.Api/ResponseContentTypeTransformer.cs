using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HealthTracker.Api;

// Az MVC minden regisztrált output formatterét (text/plain, application/json,
// text/json) felsorolja minden válasznál, és a hibaválaszokat is application/json-ként
// írja le. Valójában a sikeres válaszok JSON-ok, a hibák pedig ProblemDetails-ek
// application/problem+json típussal (lásd AddProblemDetails a Program.cs-ben).
// A spec ezt a valós viselkedést írja le; a futásidejű content negotiation nem változik.
internal sealed class ResponseContentTypeTransformer : IOpenApiOperationTransformer
{
    private const string Json = "application/json";
    private const string ProblemJson = "application/problem+json";

    public Task TransformAsync(
        OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var (statusCode, response) in operation.Responses ?? [])
        {
            if (response is not OpenApiResponse { Content: { Count: > 0 } content } concrete)
                continue;

            var isError = int.TryParse(statusCode, out var code) && code >= 400;
            var schema = content.TryGetValue(ProblemJson, out var problem) ? problem
                : content.TryGetValue(Json, out var json) ? json
                : null;

            if (schema is not null)
                concrete.Content = new Dictionary<string, OpenApiMediaType> { [isError ? ProblemJson : Json] = schema };
        }

        return Task.CompletedTask;
    }
}
