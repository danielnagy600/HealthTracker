using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.DeleteFoodEntry;

/// <summary>Igaz, ha volt mit törölni; hamis, ha nincs ilyen (vagy nem a felhasználóé).</summary>
public record DeleteFoodEntryCommand(Guid Id) : IRequest<bool>;
