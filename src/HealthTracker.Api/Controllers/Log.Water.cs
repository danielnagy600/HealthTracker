namespace HealthTracker.Api;

// Csak azonosítók kerülnek a logba – a bevitt mennyiség egészségügyi adat, az nem.
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Intake {IntakeId} added")]
    public static partial void IntakeAdded(ILogger logger, Guid intakeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Intake {IntakeId} deleted")]
    public static partial void IntakeDeleted(ILogger logger, Guid intakeId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Action}: intake {IntakeId} not found")]
    public static partial void IntakeNotFound(ILogger logger, string action, Guid intakeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Water settings updated")]
    public static partial void WaterSettingsUpdated(ILogger logger);
}
