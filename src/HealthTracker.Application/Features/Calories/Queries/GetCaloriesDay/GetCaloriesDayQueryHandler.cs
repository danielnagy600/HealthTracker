using AutoMapper;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Features.Calories.Models;
using HealthTracker.Domain.Calculators;
using MediatR;

namespace HealthTracker.Application.Features.Calories.Queries.GetCaloriesDay;

public sealed class GetCaloriesDayQueryHandler(
    IFoodEntryRepository repository, IClock clock, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetCaloriesDayQuery, DayCaloriesResponse>
{
    public async Task<DayCaloriesResponse> Handle(GetCaloriesDayQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var day = request.Date ?? DateOnly.FromDateTime(clock.Now.DateTime);

        var goal = await repository.GetOrCreateGoalAsync(userId, cancellationToken);
        var entries = await repository.GetForDateAsync(userId, day, cancellationToken);

        var daily = CalorieCalculator.Calculate(entries, goal.DailyTargetKcal);

        var groups = daily.Meals
            .Select(meal => new MealGroup(
                meal.Meal.ToString(),
                meal.Kcal,
                meal.EntryCount,
                entries
                    .Where(e => e.Meal == meal.Meal)
                    .OrderBy(e => e.RecordedAt)
                    .Select(e => mapper.Map<FoodEntryItem>(e))
                    .ToList()))
            .ToList();

        return new DayCaloriesResponse(
            day,
            daily.ConsumedKcal,
            daily.TargetKcal,
            daily.RemainingKcal,
            daily.OverKcal,
            daily.PercentOfTarget,
            daily.Status.ToString(),
            daily.Message,
            daily.LargestMeal?.Meal.ToString(),
            groups);
    }
}
