using HealthTracker.Api;
using HealthTracker.Modules.Calories;
using HealthTracker.Modules.Identity;
using HealthTracker.Modules.Schedule;
using HealthTracker.Modules.Water;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Missing 'ConnectionStrings:Postgres'. Set it in appsettings.json, appsettings.Development.json, " +
        "or the ConnectionStrings__Postgres environment variable (see docker-compose.yml).");

builder.Services.AddIdentityModule(connectionString);
builder.Services.AddWaterModule(connectionString);
builder.Services.AddScheduleModule(connectionString);
builder.Services.AddCaloriesModule(connectionString);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(WaterModule).Assembly)
    .AddApplicationPart(typeof(ScheduleModule).Assembly)
    .AddApplicationPart(typeof(CaloriesModule).Assembly);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Minden hibaválasz (400, 404, 500, body nélküli státuszok) egységesen
// ProblemDetails (RFC 9457) formátumú legyen.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// A Data Protection kulcstár alapból efemer konténerekben (nincs hova
// perzisztálnia), ezért minden konténer-újraindítás érvénytelenít minden
// kiadott bearer tokent (401-et kapnál mindenhol bejelentkezés után).
// Perzisztens mappába irányítva a kulcsok túlélik az újraindítást.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"] ?? "keys"));

var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
const string DevCors = "dev-cors";
builder.Services.AddCors(options => options.AddPolicy(DevCors, policy =>
    policy.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new()
        {
            Title = "HealthTracker API",
            Version = "v1",
            Description = "HealthTracker REST API (Identity, Water, Schedule, Calories)",
            Contact = new()
            {
                Name = "Daniel Nagy",
                Email = "daniel.nagy@example.com"
            },
            License = new()
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddSchemaTransformer<StrictNumberSchemaTransformer>();
    options.AddOperationTransformer<ResponseContentTypeTransformer>();
});

var app = builder.Build();

try
{
    await app.Services.MigrateIdentityModuleAsync();
    await app.Services.MigrateWaterModuleAsync();
    await app.Services.MigrateScheduleModuleAsync();
    await app.Services.MigrateCaloriesModuleAsync();
}
catch (Exception ex)
{
    Log.MigrationFailed(app.Logger, ex);
}

app.UseCors(DevCors);
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // Spec: /openapi/v1.json, UI: /scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "HealthTracker API fut. Auth: /api/auth/login, Water: /api/water/day");

// Az Identity modul marad minimal API (beépített AddIdentityApiEndpoints/MapIdentityApi,
// a tokenkezelést kézi Controllerben újraírni biztonsági kockázat lenne).
app.MapIdentityModule();

// Water/Schedule/Calories Controller-alapú végpontjai.
app.MapControllers();

app.Run();
