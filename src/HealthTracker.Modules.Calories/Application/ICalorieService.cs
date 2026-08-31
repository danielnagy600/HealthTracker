namespace HealthTracker.Modules.Calories.Application;

public interface ICalorieService
{
    Task<DayCaloriesResponse> GetDayAsync(DateOnly? onDate = null, CancellationToken ct = default);

    Task<FoodEntryItem> AddAsync(SaveFoodEntryRequest request, CancellationToken ct = default);

    Task<FoodEntryItem?> UpdateAsync(Guid id, SaveFoodEntryRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<GoalResponse> GetGoalAsync(CancellationToken ct = default);

    Task<GoalResponse> UpdateGoalAsync(UpdateGoalRequest request, CancellationToken ct = default);
}
