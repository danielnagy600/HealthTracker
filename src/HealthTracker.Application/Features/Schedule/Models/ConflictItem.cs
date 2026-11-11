namespace HealthTracker.Application.Features.Schedule.Models;

public record ConflictItem(
    Guid FirstId,
    Guid SecondId,
    string FirstTitle,
    string SecondTitle,
    TimeOnly OverlapStart,
    TimeOnly OverlapEnd,
    int OverlapMinutes);
