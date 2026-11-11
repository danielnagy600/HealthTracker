using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.UpdateFoodEntry;

/// <summary>Null, ha nincs ilyen bejegyzés (vagy nem a felhasználóé).</summary>
public record UpdateFoodEntryCommand(Guid Id, DateOnly Date, string Meal, string Name, int Calories)
    : IRequest<FoodEntryItem?>;
