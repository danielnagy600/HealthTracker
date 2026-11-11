namespace HealthTracker.Api;

// A partial osztály többi része a Controllers/Log.*.cs fájlokban van (funkciónként).
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error,
        Message = "Az adatbázis-migráció nem sikerült. Fut a PostgreSQL? Indítsd: docker compose up -d db")]
    public static partial void MigrationFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Method} {Path}: request rejected: {Reason}")]
    public static partial void RequestRejected(ILogger logger, string method, string? path, string reason);
}
