using FluentValidation;
using HealthTracker.Domain.Entities;

namespace HealthTracker.Application.Features.Calories.Commands.UpdateCalorieGoal;

public sealed class UpdateCalorieGoalCommandValidator : AbstractValidator<UpdateCalorieGoalCommand>
{
    public UpdateCalorieGoalCommandValidator()
    {
        RuleFor(x => x.DailyTargetKcal)
            .InclusiveBetween(CalorieGoal.MinTargetKcal, CalorieGoal.MaxTargetKcal)
            .WithMessage(
                $"The daily target must be between {CalorieGoal.MinTargetKcal} and {CalorieGoal.MaxTargetKcal} kcal.");
    }
}
