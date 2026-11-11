using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Queries.GetCaloriesDay;

/// <param name="Date">A lekérdezett nap; null esetén a mai nap.</param>
public record GetCaloriesDayQuery(DateOnly? Date = null) : IRequest<DayCaloriesResponse>;
