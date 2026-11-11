using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.AddFoodEntry;

public record AddFoodEntryCommand(DateOnly Date, string Meal, string Name, int Calories)
    : IRequest<FoodEntryItem>;
