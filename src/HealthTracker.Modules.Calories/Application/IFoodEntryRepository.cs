using HealthTracker.Modules.Calories.Domain;

namespace HealthTracker.Modules.Calories.Application;

/// <summary>
/// A tárolás absztrakciója. Az Application réteg csak ezt az interfészt ismeri,
/// a konkrét EF Core + PostgreSQL megvalósítást nem – így a tesztek egy egyszerű,
/// memóriabeli hamis implementációt adhatnak be helyette.
///
/// Minden művelet a felhasználóra szűr, hogy a profilok adatai ne keveredjenek.
/// </summary>
public interface IFoodEntryRepository
{
    Task<IReadOnlyList<FoodEntry>> GetForDateAsync(Guid userId, DateOnly onDate, CancellationToken ct = default);

    /// <summary>Egy bejegyzés azonosító alapján, a felhasználóra szűrve (null, ha nem az övé).</summary>
    Task<FoodEntry?> FindAsync(Guid userId, Guid entryId, CancellationToken ct = default);

    Task AddAsync(FoodEntry entry, CancellationToken ct = default);

    Task UpdateAsync(FoodEntry entry, CancellationToken ct = default);

    Task RemoveAsync(FoodEntry entry, CancellationToken ct = default);

    /// <summary>A felhasználó napi kerete; ha még nincs, alapértelmezettet hoz létre és ment.</summary>
    Task<CalorieGoal> GetOrCreateGoalAsync(Guid userId, CancellationToken ct = default);

    Task UpdateGoalAsync(CalorieGoal goal, CancellationToken ct = default);
}
