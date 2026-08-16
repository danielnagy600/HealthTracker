using HealthTracker.Modules.Schedule.Domain;

namespace HealthTracker.Modules.Schedule.Application;

/// <summary>
/// A tárolás absztrakciója. Az Application réteg csak ezt az interfészt ismeri,
/// a konkrét EF Core + PostgreSQL megvalósítást nem – így a tesztek egy egyszerű,
/// memóriabeli hamis implementációt adhatnak be helyette.
///
/// Minden művelet a felhasználóra szűr, hogy a profilok adatai ne keveredjenek.
/// </summary>
public interface IActivityRepository
{
    Task<IReadOnlyList<Activity>> GetForDateAsync(Guid userId, DateOnly date, CancellationToken ct = default);

    /// <summary>Egy elfoglaltság azonosító alapján, a felhasználóra szűrve (null, ha nem az övé).</summary>
    Task<Activity?> FindAsync(Guid userId, Guid activityId, CancellationToken ct = default);

    Task AddAsync(Activity activity, CancellationToken ct = default);

    Task UpdateAsync(Activity activity, CancellationToken ct = default);

    Task RemoveAsync(Activity activity, CancellationToken ct = default);
}
