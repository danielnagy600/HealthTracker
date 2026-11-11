namespace HealthTracker.Api;

// Csak azonosítók kerülnek a logba – a cím és a jegyzet személyes adat lehet.
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Activity {ActivityId} created")]
    public static partial void ActivityCreated(ILogger logger, Guid activityId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Activity {ActivityId} updated")]
    public static partial void ActivityUpdated(ILogger logger, Guid activityId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Activity {ActivityId} deleted")]
    public static partial void ActivityDeleted(ILogger logger, Guid activityId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Action}: activity {ActivityId} not found")]
    public static partial void ActivityNotFound(ILogger logger, string action, Guid activityId);
}
