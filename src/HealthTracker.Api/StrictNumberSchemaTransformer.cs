using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HealthTracker.Api;

// A webes JSON-alapbeállítás (NumberHandling = AllowReadingFromString) miatt a
// generátor minden számot "integer | string"-ként ír le, mintamintával együtt.
// A szerver ugyan elfogad stringként küldött számot is, de mindig számot ad
// vissza, és a kliensnek is számot kell küldenie: a specben csak a szám
// típust hagyjuk meg, különben a generált TS típusok `number | string`-ek lennének.
internal sealed class StrictNumberSchemaTransformer : IOpenApiSchemaTransformer
{
    private const JsonSchemaType Numeric = JsonSchemaType.Integer | JsonSchemaType.Number;

    public Task TransformAsync(
        OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Type is { } type && (type & Numeric) != 0 && type.HasFlag(JsonSchemaType.String))
        {
            schema.Type = type & ~JsonSchemaType.String;
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}
