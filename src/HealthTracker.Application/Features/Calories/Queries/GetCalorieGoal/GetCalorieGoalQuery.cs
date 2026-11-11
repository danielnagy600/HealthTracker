using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Queries.GetCalorieGoal;

public record GetCalorieGoalQuery : IRequest<GoalResponse>;
