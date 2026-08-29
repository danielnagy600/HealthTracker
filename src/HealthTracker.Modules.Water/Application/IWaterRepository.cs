using HealthTracker.Modules.Water.Domain;

namespace HealthTracker.Modules.Water.Application;

/// <summary>
/// A tárolás absztrakciója. Az Application réteg csak ezt az interfészt ismeri,
/// a konkrét EF Core + PostgreSQL megvalósítást nem – így a tesztek egy egyszerű,
/// memóriabeli hamis implementációt adhatnak be helyette.
///
/// Minden művelet a felhasználóra szűr, hogy a profilok adatai ne keveredjenek.
/// </summary>
public interface IWaterRepository
{
    Task AddIntakeAsync(WaterIntake intake, CancellationToken ct = default);

    Task<IReadOnlyList<WaterIntake>> GetIntakesForDateAsync(Guid userId, DateOnly onDate, CancellationToken ct = default);

    /// <summary>A felhasználó beállításai; ha még nincs, alapértelmezettet hoz létre és ment.</summary>
    Task<WaterSettings> GetOrCreateSettingsAsync(Guid userId, CancellationToken ct = default);

    Task UpdateSettingsAsync(WaterSettings settings, CancellationToken ct = default);
}
