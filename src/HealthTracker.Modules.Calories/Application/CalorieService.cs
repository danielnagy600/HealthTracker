using HealthTracker.Modules.Calories.Domain;
using HealthTracker.SharedKernel.Abstractions;

namespace HealthTracker.Modules.Calories.Application;

/// <summary>
/// A Calories modul üzleti logikája. Összeköti a tárolót (IFoodEntryRepository),
/// az órát (IClock) és a bejelentkezett felhasználót (ICurrentUser) a tiszta
/// domain-számítással (CalorieCalculator).
/// </summary>
public sealed class CalorieService : ICalorieService
{
    private readonly IFoodEntryRepository _repository;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public CalorieService(IFoodEntryRepository repository, IClock clock, ICurrentUser currentUser)
    {
        _repository = repository;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<DayCaloriesResponse> GetDayAsync(DateOnly? date = null, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var day = date ?? DateOnly.FromDateTime(_clock.Now.DateTime);

        var goal = await _repository.GetOrCreateGoalAsync(userId, ct);
        var entries = await _repository.GetForDateAsync(userId, day, ct);

        // A tiszta domain-számítás – ezt a tesztek külön is ellenőrzik.
        var daily = CalorieCalculator.Calculate(entries, goal.DailyTargetKcal);

        var groups = daily.Meals
            .Select(meal => new MealGroup(
                meal.Meal.ToString(),
                meal.Kcal,
                meal.EntryCount,
                entries
                    .Where(e => e.Meal == meal.Meal)
                    .OrderBy(e => e.RecordedAt)
                    .Select(ToItem)
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

    public async Task<FoodEntryItem> AddAsync(SaveFoodEntryRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var entry = new FoodEntry(
            Guid.NewGuid(), userId, request.Date, ParseMeal(request.Meal),
            request.Name, request.Calories, _clock.Now.ToUniversalTime());

        await _repository.AddAsync(entry, ct);
        return ToItem(entry);
    }

    public async Task<FoodEntryItem?> UpdateAsync(
        Guid id, SaveFoodEntryRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var entry = await _repository.FindAsync(userId, id, ct);
        if (entry is null)
            return null;

        entry.Update(request.Date, ParseMeal(request.Meal), request.Name, request.Calories);

        await _repository.UpdateAsync(entry, ct);
        return ToItem(entry);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var entry = await _repository.FindAsync(userId, id, ct);
        if (entry is null)
            return false;

        await _repository.RemoveAsync(entry, ct);
        return true;
    }

    public async Task<GoalResponse> GetGoalAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var goal = await _repository.GetOrCreateGoalAsync(userId, ct);
        return new GoalResponse(goal.DailyTargetKcal);
    }

    public async Task<GoalResponse> UpdateGoalAsync(UpdateGoalRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var error = CalorieGoal.Validate(request.DailyTargetKcal);
        if (error is not null)
            throw new ArgumentException(error);

        var goal = await _repository.GetOrCreateGoalAsync(userId, ct);
        goal.DailyTargetKcal = request.DailyTargetKcal;

        await _repository.UpdateGoalAsync(goal, ct);
        return new GoalResponse(goal.DailyTargetKcal);
    }

    /// <summary>Az étkezés nevének feloldása; ismeretlen név esetén a nasi.</summary>
    public static MealType ParseMeal(string? meal) =>
        Enum.TryParse<MealType>(meal, ignoreCase: true, out var parsed) ? parsed : MealType.Snack;

    /// <summary>Igaz, ha a megadott étkezés szerepel a felsorolásban.</summary>
    public static bool IsKnownMeal(string? meal) =>
        Enum.TryParse<MealType>(meal, ignoreCase: true, out _);

    private static FoodEntryItem ToItem(FoodEntry e) =>
        new(e.Id, e.Date, e.Meal.ToString(), e.Name, e.Calories, e.RecordedAt);
}
