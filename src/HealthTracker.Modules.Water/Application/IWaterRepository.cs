using HealthTracker.Modules.Water.Domain;

namespace HealthTracker.Modules.Water.Application;

public interface IWaterRepository
{
    Task AddIntakeAsync(WaterIntake intake, CancellationToken ct = default);
    Task<IReadOnlyList<WaterIntake>> GetIntakesForDateAsync(Guid userId, DateOnly onDate, CancellationToken ct = default);
    Task<WaterSettings> GetOrCreateSettingsAsync(Guid userId, CancellationToken ct = default);
    Task UpdateSettingsAsync(WaterSettings settings, CancellationToken ct = default);
}
