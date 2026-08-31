namespace HealthTracker.Modules.Water.Domain;

public class WaterSettings
{
    public Guid UserId { get; set; }

    public int DailyTargetMl { get; set; } = 2000;

    public TimeOnly WakeTime { get; set; } = new(7, 0);

    public TimeOnly SleepTime { get; set; } = new(22, 0);

    public static WaterSettings CreateDefault(Guid userId) => new() { UserId = userId };
}
