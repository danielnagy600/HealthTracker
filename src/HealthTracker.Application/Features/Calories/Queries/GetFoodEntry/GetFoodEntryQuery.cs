using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Queries.GetFoodEntry;

/// <summary>Null, ha nincs ilyen bejegyzés (vagy nem a felhasználóé).</summary>
public record GetFoodEntryQuery(Guid Id) : IRequest<FoodEntryItem?>;
