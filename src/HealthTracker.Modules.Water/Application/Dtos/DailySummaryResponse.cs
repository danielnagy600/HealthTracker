namespace HealthTracker.Modules.Water.Application.Dtos;

public record DailySummaryResponse(
    DateOnly Date,
    int TargetMl,
    int ConsumedMl,
    int RemainingMl,
    double PercentComplete,
    IReadOnlyList<IntakeItem> Intakes);
