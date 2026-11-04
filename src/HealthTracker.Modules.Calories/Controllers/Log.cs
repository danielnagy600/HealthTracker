using Microsoft.Extensions.Logging;

namespace HealthTracker.Modules.Calories;

// Csak azonosítók kerülnek a logba – ételnév, kalória egészségügyi adat, az nem.
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Food entry {EntryId} created")]
    public static partial void FoodEntryCreated(ILogger logger, Guid entryId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Food entry {EntryId} updated")]
    public static partial void FoodEntryUpdated(ILogger logger, Guid entryId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Food entry {EntryId} deleted")]
    public static partial void FoodEntryDeleted(ILogger logger, Guid entryId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Action}: food entry {EntryId} not found")]
    public static partial void FoodEntryNotFound(ILogger logger, string action, Guid entryId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Calorie goal updated")]
    public static partial void GoalUpdated(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Action}: request rejected: {Reason}")]
    public static partial void RequestRejected(ILogger logger, string action, string reason);
}
