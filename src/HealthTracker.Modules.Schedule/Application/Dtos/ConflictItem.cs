namespace HealthTracker.Modules.Schedule.Application.Dtos;

public record ConflictItem(
    Guid FirstId,
    Guid SecondId,
    string FirstTitle,
    string SecondTitle,
    TimeOnly OverlapStart,
    TimeOnly OverlapEnd,
    int OverlapMinutes);
