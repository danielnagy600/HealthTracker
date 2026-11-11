using HealthTracker.Application.Features.Calories.Models;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Commands.UpdateCalorieGoal;

public record UpdateCalorieGoalCommand(int DailyTargetKcal) : IRequest<GoalResponse>;
