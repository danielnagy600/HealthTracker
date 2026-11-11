using FluentValidation;
using HealthTracker.Domain.Entities;
using HealthTracker.Domain.Enums;

namespace HealthTracker.Application.Features.Calories.Commands.UpdateFoodEntry;

// Ugyanazok a szabályok, mint az AddFoodEntryCommandValidator-ban.
public sealed class UpdateFoodEntryCommandValidator : AbstractValidator<UpdateFoodEntryCommand>
{
    public UpdateFoodEntryCommandValidator()
    {
        // Egy mezőn belül az első hibánál álljon meg (pl. üres névnél már ne nézze a hosszát).
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Meal)
            .Must(MealTypeParser.IsKnown)
            .WithMessage(x =>
                $"Unknown meal: '{x.Meal}'. Available: {string.Join(", ", Enum.GetNames<MealType>())}.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The food name cannot be empty.")
            .Must(name => name.Trim().Length <= FoodEntry.MaxNameLength)
            .WithMessage($"The food name can be at most {FoodEntry.MaxNameLength} characters.");

        RuleFor(x => x.Calories)
            .GreaterThan(0).WithMessage("Calories must be positive.")
            .LessThanOrEqualTo(FoodEntry.MaxCalories)
            .WithMessage($"A single entry can be at most {FoodEntry.MaxCalories} kcal.");
    }
}
