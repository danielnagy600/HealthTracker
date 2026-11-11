namespace HealthTracker.Application.Features.Water.Models;

public record IntakeItem(Guid Id, DateTimeOffset RecordedAt, int AmountMl);
