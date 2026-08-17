namespace HealthTracker.Modules.Calories.Application;

/// <summary>
/// A Calories modul üzleti belépési pontja. A HTTP-végpontok ezt hívják.
/// A bejelentkezett felhasználót a szolgáltatás maga oldja fel (ICurrentUser),
/// ezért a metódusok nem kérnek userId paramétert.
/// </summary>
public interface ICalorieService
{
    /// <summary>Egy nap teljes képe étkezésenkénti bontásban. Dátum nélkül a mai nap.</summary>
    Task<DayCaloriesResponse> GetDayAsync(DateOnly? date = null, CancellationToken ct = default);

    Task<FoodEntryItem> AddAsync(SaveFoodEntryRequest request, CancellationToken ct = default);

    /// <summary>Módosítás; null, ha nincs ilyen bejegyzése a felhasználónak.</summary>
    Task<FoodEntryItem?> UpdateAsync(Guid id, SaveFoodEntryRequest request, CancellationToken ct = default);

    /// <summary>Törlés; false, ha nincs ilyen bejegyzése a felhasználónak.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<GoalResponse> GetGoalAsync(CancellationToken ct = default);

    Task<GoalResponse> UpdateGoalAsync(UpdateGoalRequest request, CancellationToken ct = default);
}
