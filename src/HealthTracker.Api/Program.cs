using HealthTracker.Api;
using HealthTracker.Modules.Calories;
using HealthTracker.Modules.Identity;
using HealthTracker.Modules.Schedule;
using HealthTracker.Modules.Water;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=healthtracker;Username=postgres;Password=postgres";

builder.Services.AddIdentityModule(connectionString);
builder.Services.AddWaterModule(connectionString);
builder.Services.AddScheduleModule(connectionString);
builder.Services.AddCaloriesModule(connectionString);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
const string DevCors = "dev-cors";
builder.Services.AddCors(options => options.AddPolicy(DevCors, policy =>
    policy.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod()));

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
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "HealthTracker API fut. Auth: /api/auth/login, Water: /api/water/summary");

app.MapIdentityModule();
app.MapWaterModule();
app.MapScheduleModule();
app.MapCaloriesModule();

app.Run();

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error,
        Message = "Az adatbázis-migráció nem sikerült. Fut a PostgreSQL? Indítsd: docker compose up -d db")]
    public static partial void MigrationFailed(ILogger logger, Exception exception);
}
