namespace HealthTracker.Api;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error,
        Message = "Az adatbázis-migráció nem sikerült. Fut a PostgreSQL? Indítsd: docker compose up -d db")]
    public static partial void MigrationFailed(ILogger logger, Exception exception);
}
