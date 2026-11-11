using MediatR;

namespace HealthTracker.Application.Features.Water.Commands.DeleteIntake;

/// <summary>Igaz, ha volt mit törölni; hamis, ha nincs ilyen (vagy nem a felhasználóé).</summary>
public record DeleteIntakeCommand(Guid Id) : IRequest<bool>;
