namespace HealthTracker.Modules.Calories.Domain;

public class CalorieGoal
{
    public const int MinTargetKcal = 500;
    public const int MaxTargetKcal = 10000;

    public Guid UserId { get; set; }

    public int DailyTargetKcal { get; set; } = 2000;

    public static CalorieGoal CreateDefault(Guid userId) => new() { UserId = userId };

    public static string? Validate(int dailyTargetKcal) =>
        dailyTargetKcal is < MinTargetKcal or > MaxTargetKcal
            ? $"The daily target must be between {MinTargetKcal} and {MaxTargetKcal} kcal."
            : null;
}
