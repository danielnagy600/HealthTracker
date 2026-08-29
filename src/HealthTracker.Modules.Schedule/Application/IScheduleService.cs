namespace HealthTracker.Modules.Schedule.Application;

/// <summary>
/// A Schedule modul üzleti belépési pontja. A HTTP-végpontok ezt hívják.
/// A bejelentkezett felhasználót a szolgáltatás maga oldja fel (ICurrentUser),
/// ezért a metódusok nem kérnek userId paramétert.
/// </summary>
public interface IScheduleService
{
    /// <summary>Egy nap teljes képe. Ha nincs dátum megadva, a mai napot adja.</summary>
    Task<DayScheduleResponse> GetDayAsync(DateOnly? onDate = null, CancellationToken ct = default);

    Task<ActivityItem> AddAsync(SaveActivityRequest request, CancellationToken ct = default);

    /// <summary>Módosítás; null, ha nincs ilyen elfoglaltsága a felhasználónak.</summary>
    Task<ActivityItem?> UpdateAsync(Guid id, SaveActivityRequest request, CancellationToken ct = default);

    /// <summary>Törlés; false, ha nincs ilyen elfoglaltsága a felhasználónak.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
