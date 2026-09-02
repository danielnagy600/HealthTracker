using HealthTracker.Api;
using HealthTracker.Modules.Calories;
using HealthTracker.Modules.Identity;
using HealthTracker.Modules.Schedule;
using HealthTracker.Modules.Water;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

// A három CRUD-modul (Water, Schedule, Calories) controller-alapú végpontjainak
// felfedezéséhez kellenek a típusai – az assembly-jüket adjuk hozzá application
// part-ként lentebb (a controllerek nem ebben, hanem a saját modul-projektjükben
// laknak, ott nem fedezné fel őket a keretrendszer automatikusan).
using HealthTracker.Modules.Calories.Controllers;
using HealthTracker.Modules.Schedule.Controllers;
using HealthTracker.Modules.Water.Controllers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Missing 'ConnectionStrings:Postgres'. Set it in appsettings.json, appsettings.Development.json, " +
        "or the ConnectionStrings__Postgres environment variable (see docker-compose.yml).");

builder.Services.AddIdentityModule(connectionString);
builder.Services.AddWaterModule(connectionString);
builder.Services.AddScheduleModule(connectionString);
builder.Services.AddCaloriesModule(connectionString);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// A Water/Schedule/Calories modulok controllerei külön projektben (assembly-ben)
// élnek, ezért explicit application part-ként kell hozzáadni őket – enélkül az
// AddControllers() csak a jelen (Api) assembly-t vizsgálná, a modulokét nem.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(WaterController).Assembly)
    .AddApplicationPart(typeof(ScheduleController).Assembly)
    .AddApplicationPart(typeof(CaloriesController).Assembly);

var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";
const string DevCors = "dev-cors";
builder.Services.AddCors(options => options.AddPolicy(DevCors, policy =>
    policy.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HealthTracker API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Illeszd be a /api/auth/login válaszából kapott accessToken-t (a \"Bearer \" előtagot a mező maga teszi hozzá)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document, null), [] }
    });
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
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "HealthTracker API fut. Auth: /api/auth/login, Water: /api/water/summary");

// Identity marad minimal API (MapIdentityApi beépített végpontjai); a Water,
// Schedule és Calories modulok controllerei egyetlen MapControllers()-szel
// kerülnek be – az útvonalaikat az [ApiController]/[Route] attribútumok adják.
app.MapIdentityModule();
app.MapControllers();

app.Run();

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error,
        Message = "Az adatbázis-migráció nem sikerült. Fut a PostgreSQL? Indítsd: docker compose up -d db")]
    public static partial void MigrationFailed(ILogger logger, Exception exception);
}
