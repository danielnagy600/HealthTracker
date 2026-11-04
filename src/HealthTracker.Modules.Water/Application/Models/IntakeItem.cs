namespace HealthTracker.Modules.Water.Application.Models;

public record IntakeItem(Guid Id, DateTimeOffset RecordedAt, int AmountMl);
