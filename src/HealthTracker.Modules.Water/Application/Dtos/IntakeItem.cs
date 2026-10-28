namespace HealthTracker.Modules.Water.Application.Dtos;

public record IntakeItem(Guid Id, DateTimeOffset RecordedAt, int AmountMl);
