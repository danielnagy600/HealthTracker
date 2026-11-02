using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HealthTracker.Api;

// A beépített OpenAPI-generátor nem tudja magától, hogy az API bearer tokennel
// védett. Ez a transformer felveszi a "Bearer" sémát a dokumentumba, és
// dokumentumszinten kötelezővé teszi, így a Scalar UI-ban megadható a token.
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "Bearer";

    public Task TransformAsync(
        OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Description = "Illeszd be a /api/auth/login válaszából kapott accessToken-t (a \"Bearer \" előtag nélkül)."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference(SchemeName, document), [] }
        });

        return Task.CompletedTask;
    }
}
