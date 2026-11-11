using MediatR;

namespace HealthTracker.Application.Features.Schedule.Commands.DeleteActivity;

/// <summary>Igaz, ha volt mit törölni; hamis, ha nincs ilyen (vagy nem a felhasználóé).</summary>
public record DeleteActivityCommand(Guid Id) : IRequest<bool>;
