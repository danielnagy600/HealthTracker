namespace HealthTracker.Application.Features.Water.Models;

public record DailySummaryResponse(
    DateOnly Date,
    int TargetMl,
    int ConsumedMl,
    int RemainingMl,
    double PercentComplete,
    IReadOnlyList<IntakeItem> Intakes);
